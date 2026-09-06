using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Static class that persists player state between scenes (combat ↔ map).
/// Reset on fresh game start via RuntimeInitializeOnLoadMethod.
/// </summary>
public static class GameState
{
    /// <summary>Minimum number of cards allowed in the deck. Purge is disabled at this threshold.</summary>
    public const int MIN_DECK_SIZE = 6;

    /// <summary>Current act (1, 2, or 3).</summary>
    public static int CurrentAct = 1;

    /// <summary>The node the player last clicked on the map.</summary>
    public static MapNodeRuntime SelectedNode = null;

    /// <summary>
    /// The generated DAG for the current run.
    /// Outer list = layers (bottom to top), inner list = nodes in that layer.
    /// Null until first map generation.
    /// </summary>
    public static List<List<MapNodeRuntime>> GeneratedMap = null;

    /// <summary>Player's current HP carried between fights.</summary>
    public static int PlayerCurrentHP = -1;

    /// <summary>Player's max HP.</summary>
    public static int PlayerMaxHP = 100;

    /// <summary>Whether the game has been initialized.</summary>
    public static bool IsInitialized = false;

    /// <summary>The hero data to use (set from first scene load).</summary>
    public static HeroData HeroData;

    /// <summary>
    /// The player's current deck composition, persisted across battles.
    /// Null until initialized from HeroData.Deck on first map load.
    /// Modified by Duplicate/Purge operations at rewards and map nodes.
    /// </summary>
    public static List<CardData> PlayerDeck = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        CurrentAct = 1;
        SelectedNode = null;
        GeneratedMap = null;
        PlayerCurrentHP = -1;
        PlayerMaxHP = 100;
        IsInitialized = false;
        HeroData = null;
        PlayerDeck = null;
    }

    /// <summary>
    /// Initializes PlayerDeck from HeroData.Deck if not already set.
    /// Called during first map load.
    /// </summary>
    public static void InitializeDeck(List<CardData> startingDeck)
    {
        if (PlayerDeck == null)
        {
            PlayerDeck = new List<CardData>(startingDeck);
        }
    }

    /// <summary>
    /// Adds a duplicate copy of the given card to the player's deck.
    /// </summary>
    public static void DuplicateCard(CardData card)
    {
        if (PlayerDeck != null && card != null)
        {
            PlayerDeck.Add(card);
            Debug.Log($"<color=lime>[GameState]</color> Duplicated '{card.Title}'. Deck size: {PlayerDeck.Count}");
        }
    }

    /// <summary>
    /// Removes one instance of the given card from the player's deck.
    /// Will not remove if doing so would drop below MIN_DECK_SIZE.
    /// </summary>
    public static bool PurgeCard(CardData card)
    {
        if (PlayerDeck == null || card == null) return false;
        if (!CanPurge())
        {
            Debug.LogWarning($"[GameState] Cannot purge — deck is at minimum size ({MIN_DECK_SIZE}).");
            return false;
        }
        bool removed = PlayerDeck.Remove(card);
        if (removed)
        {
            Debug.Log($"<color=red>[GameState]</color> Purged '{card.Title}'. Deck size: {PlayerDeck.Count}");
        }
        return removed;
    }

    /// <summary>
    /// Returns true if the deck has more cards than the minimum allowed.
    /// </summary>
    public static bool CanPurge()
    {
        return PlayerDeck != null && PlayerDeck.Count > MIN_DECK_SIZE;
    }

    /// <summary>
    /// Returns a deduplicated list of distinct CardData in the player's deck.
    /// Useful for displaying card selection UIs.
    /// </summary>
    public static List<CardData> GetDistinctCards()
    {
        if (PlayerDeck == null) return new List<CardData>();
        return PlayerDeck.Distinct().ToList();
    }

    /// <summary>
    /// Returns the full deck list (including duplicates), sorted by card name.
    /// Useful for the deck viewer overlay on the map.
    /// </summary>
    public static List<CardData> GetFullDeck()
    {
        if (PlayerDeck == null) return new List<CardData>();
        var sorted = new List<CardData>(PlayerDeck);
        sorted.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
        return sorted;
    }


    /// <summary>
    /// Returns how many copies of a specific card are in the deck.
    /// </summary>
    public static int GetCardCount(CardData card)
    {
        if (PlayerDeck == null || card == null) return 0;
        return PlayerDeck.Count(c => c == card);
    }

    /// <summary>
    /// Resets run progress (called on player defeat).
    /// </summary>
    public static void ResetRun()
    {
        GeneratedMap = null;
        SelectedNode = null;
        CurrentAct = 1;
        PlayerCurrentHP = -1;
        PlayerDeck = null;
    }

    /// <summary>
    /// Call after winning a combat or visiting a non-combat node.
    /// Marks the node as visited and saves player HP.
    /// </summary>
    public static void CompleteSelectedNode(int playerHPAfterFight)
    {
        PlayerCurrentHP = playerHPAfterFight;
        if (SelectedNode != null)
        {
            SelectedNode.IsVisited = true;
            SelectedNode.IsUnlocked = false;

            // Unlock direct children
            foreach (var child in SelectedNode.Children)
            {
                child.IsUnlocked = true;
            }
        }
    }

    /// <summary>
    /// Returns true if there is no unlocked, non-visited node in the current map
    /// (i.e., the run is complete or the boss has been defeated).
    /// </summary>
    public static bool IsRunComplete()
    {
        if (GeneratedMap == null) return false;
        foreach (var layer in GeneratedMap)
        {
            foreach (var node in layer)
            {
                if (!node.IsVisited && node.IsUnlocked) return false;
                if (!node.IsVisited && !node.IsLocked) return false;
            }
        }
        return true;
    }
}

