using Microsoft.AspNetCore.Mvc;
using CoffeeMachine.Models;
using Newtonsoft.Json;

namespace CoffeeMachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoffeeMachineController : ControllerBase
    {
        private static Dictionary<string, int> _ingredientsStock = new()
        {
            { "Coffee", 500 },
            { "Water", 1000 },
            { "Milk", 500 }
        };
        private static Statistics _statistics = new();

        private readonly IHttpClientFactory _httpClientFactory;
        
        public CoffeeMachineController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task<bool> IsMachineOperationalAsync()
        {
            var currentTime = DateTime.Now;

            if (currentTime.Hour < 8 || currentTime.Hour >= 17)
            {
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync("https://date.nager.at/Api/v2/PublicHolidays/2024/US");
            var publicHolidays = JsonConvert.DeserializeObject<List<PublicHoliday>>(response);

            var isHoliday = (publicHolidays ?? throw new InvalidOperationException()).Any(h => DateTime.Parse(h.Date).Date == currentTime.Date);

            if (currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday || isHoliday)
            {
                return false;
            }

            return true;
        }


        [HttpPost("make")]
        public async Task<IActionResult> MakeCoffee([FromBody] string recipeName)
        {
            if (!await IsMachineOperationalAsync())
            {
                return BadRequest("The coffee machine is closed. It operates only from 8:00 AM to 5:00 PM on weekdays.");
            }

            if (!CoffeeMachineStorage.Recipes.TryGetValue(recipeName, out var recipe))
            {
                return NotFound($"Recipe '{recipeName}' not found.");
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (!_ingredientsStock.TryGetValue(ingredient.Key, out var available) || available < ingredient.Value)
                {
                    return BadRequest($"Not enough {ingredient.Key} to make {recipeName}.");
                }
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                _ingredientsStock[ingredient.Key] -= ingredient.Value;
            }

            _statistics.IncrementDrinkCount(recipeName);
            CoffeeMachineStorage.Statistics.Add($"{recipeName} made at {DateTime.Now}");
            return Ok($"Enjoy your {recipeName}!");
        }

        [HttpPost("add-recipe")]
        public IActionResult AddRecipe([FromBody] Recipe recipe)
        {
            if (CoffeeMachineStorage.Recipes.ContainsKey(recipe.Name))
            {
                return Conflict($"Recipe '{recipe.Name}' already exists.");
            }

            CoffeeMachineStorage.Recipes[recipe.Name] = recipe;
            return Ok($"Recipe '{recipe.Name}' added successfully.");
        }

        [HttpGet("stats")]
        public IActionResult GetStatistics()
        {
            var stats = _statistics.GetStatistics();
            var mostPopularDrink = stats.OrderByDescending(d => d.Value).FirstOrDefault();

            return Ok(new
            {
                MostPopular = mostPopularDrink.Key,
                Stats = stats
            });
        }

        [HttpGet("stock")]
        public IActionResult GetIngredientsStock()
        {
            return Ok(_ingredientsStock);
        }

        [HttpPost("restock")]
        public IActionResult RestockIngredients([FromBody] Dictionary<string, int> restockItems)
        {
            foreach (var item in restockItems)
            {
                if (_ingredientsStock.ContainsKey(item.Key))
                {
                    _ingredientsStock[item.Key] += item.Value;
                }
                else
                {
                    _ingredientsStock[item.Key] = item.Value;
                }
            }

            return Ok("Ingredients restocked successfully.");
        }

        
        [HttpDelete("delete-recipe/{recipeName}")]
        public IActionResult DeleteRecipe(string recipeName)
        {
            if (!CoffeeMachineStorage.Recipes.ContainsKey(recipeName))
            {
                return NotFound($"Recipe '{recipeName}' not found.");
            }

            CoffeeMachineStorage.Recipes.Remove(recipeName);
            return Ok($"Recipe '{recipeName}' successfully deleted.");
        }
    }

    public class PublicHoliday
    {
        public PublicHoliday(string date)
        {
            Date = date;
        }

        public required string Date { get; set; }
    }
}