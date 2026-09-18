using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions 
{
    public static T Draw<T>(this List<T> list)
    {
        // 1. Safety check
        if (list.Count == 0) return default;
        
        // 2. Pop from the end of the list (top of the deck)
        int lastIndex = list.Count - 1;
        T t = list[lastIndex]; 
        
        // 3. Remove the item
        list.RemoveAt(lastIndex);
        
        // 4. Return the saved item
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