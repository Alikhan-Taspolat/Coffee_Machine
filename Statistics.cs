namespace CoffeeMachine;

public class Statistics
{
    public Dictionary<string, int> DrinksMade { get; set; } = new Dictionary<string, int>();

    public void IncrementDrinkCount(string drinkName)
    {
        if (DrinksMade.ContainsKey(drinkName))
        {
            DrinksMade[drinkName]++;
        }
        else
        {
            DrinksMade[drinkName] = 1;
        }
    }

    public Dictionary<string, int> GetStatistics()
    {
        return DrinksMade;
    }
}