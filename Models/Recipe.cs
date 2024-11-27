namespace CoffeeMachine.Models;

public class Recipe
{
    public string Name { get; set; }
    public Dictionary<string, int> Ingredients { get; set; }

    public Recipe(string name, Dictionary<string, int> ingredients)
    {
        Name = name;
        Ingredients = ingredients;
    }
}