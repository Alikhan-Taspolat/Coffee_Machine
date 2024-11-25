using Microsoft.AspNetCore.Mvc;
using CoffeeMachine.Models;

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

        [HttpPost("make")]
        public IActionResult MakeCoffee([FromBody] string recipeName)
        {
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
}