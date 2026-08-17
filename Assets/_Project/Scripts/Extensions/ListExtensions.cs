using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions 
{
    public static T Draw<T>(this List<T> list)
    {
        // 1. Safety check
        if (list.Count == 0) return default;
        
        // 2. Pick a random index
        int r = Random.Range(0, list.Count);
        
        // 3. Save the item at that index to a variable 't'
        T t = list[r]; 
        
        // 4. Remove the item from the list using its index
        list.RemoveAt(r);
        
        // 5. Return the saved item
        return t; 
    }

    /// <summary>
    /// Fisher-Yates shuffle — randomizes the list in-place.
    /// Used when reshuffling the discard pile back into the draw pile.
    /// </summary>
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}