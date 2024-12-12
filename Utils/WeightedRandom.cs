namespace JetLagBRBot.Utils;

public class WeightedItem<TItem>(TItem item, int weight)
{
    public TItem Item { get; set; } = item;
    public int Weight { get; set; } = weight;
}

public class WeightedRandomSelector
{
    private static readonly Random RandomGenerator = new();

    public static WeightedItem<TItem> SelectRandomItem<TItem>(WeightedItem<TItem>[] items)
    {
        if (items == null || items.Length == 0)
            throw new ArgumentException("The array cannot be empty.");

        // Calculate the total sum of weights
        int totalWeight = items.Sum(item => item.Weight);
        if (totalWeight <= 0)
            throw new ArgumentException("All weights must be greater than 0.");

        // Generate a random number between 1 and totalWeight (inclusive)
        int randomNumber = RandomGenerator.Next(1, totalWeight + 1);

        // Find the item whose cumulative weight includes the random number
        int cumulativeWeight = 0;
        foreach (var item in items)
        {
            cumulativeWeight += item.Weight;
            if (randomNumber <= cumulativeWeight)
            {
                return item;
            }
        }

        // This should never be reached as the random number is always within a valid range
        throw new InvalidOperationException("No item could be selected.");
    }
}