namespace CoffeeMachine.Models;

public class CoffeeMachineStorage
{
    public static Dictionary<string, Recipe> Recipes { get; } = new()
    {
        { "Espresso", new Recipe("Espresso", new Dictionary<string, int> { { "Water", 50 }, { "Coffee", 10 } }) },
        { "Latte", new Recipe("Latte", new Dictionary<string, int> { { "Water", 200 }, { "Milk", 100 }, { "Coffee", 20 } }) },
        {"Americano", new Recipe("Americano", new Dictionary<string, int> { { "Coffee", 50 }, { "Water", 100 } }) },
        {"Cappuccino", new Recipe("Cappuccino", new Dictionary<string, int> { { "Coffee", 50 }, { "Milk", 100 } }) }
    };
    public static List<string> Statistics { get; } = new();
}