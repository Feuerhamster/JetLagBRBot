namespace JetLagBRBot.Utils;

using System;
using System.Linq; // Provides LINQ functionality for easier operations on arrays and collections

// A class representing an item with an associated percentage weight
public class WeightedItem<TItem>
{
    // The item itself
    public TItem Item { get; set; }

    // The percentage weight associated with the item, representing its likelihood of being selected
    public int Weight { get; set; }

    // Constructor to initialize the item and its weight
    public WeightedItem(TItem item, int weight = 100) // Default weight is 100 (100%)
    {
        Item = item;
        Weight = weight;
    }
}

// A utility class to select a random item based on percentage weights with fairness
public class WeightedAndBalancedRandomSelector<TItem>
{
    // Instance Random generator
    private readonly Random RandomGenerator = new();

    // Items to manage
    private readonly WeightedItem<TItem>[] Items;

    // Tracks selection counts independently
    private readonly int[] SelectionCounts;

    // Constructor to initialize the selector with items
    public WeightedAndBalancedRandomSelector(WeightedItem<TItem>[] items)
    {
        if (items == null || items.Length == 0)
        {
            throw new ArgumentException("Items array must not be null or empty.");
        }

        Items = items;
        SelectionCounts = new int[items.Length];
        this.ResetSelectionCounts();
    }

    // Method to select a random item based on weights with adaptive fairness
    public WeightedItem<TItem> SelectRandomItem()
    {
        // Step 1: Calculate the adjusted weights by factoring in selection counts
        int totalWeight = Items.Select((item, index) => item.Weight / (SelectionCounts[index] + 1)).Sum();
        
        if (totalWeight <= 0)
        {
            throw new InvalidOperationException("Total weight must be greater than zero.");
        }
        
        int randomValue = RandomGenerator.Next(totalWeight);

        // Step 3: Iterate through the items to find the one corresponding to the random value
        int cumulativeWeight = 0;

        for (int i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            int adjustedWeight = item.Weight / (SelectionCounts[i] + 1);
            cumulativeWeight += adjustedWeight;

            // If the random value falls within the current cumulative range, select this item
            if (randomValue < cumulativeWeight)
            {
                SelectionCounts[i]++;
                return item;
            }
        }

        // This point should never be reached if the weights are valid
        throw new InvalidOperationException("Failed to select a random item. This should not happen.");
    }
    
    public void ResetSelectionCounts()
    {
        for (int i = 0; i < SelectionCounts.Length; i++)
        {
            SelectionCounts[i] = 0;
        }
    }
}
