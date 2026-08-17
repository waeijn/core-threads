using UnityEngine;

public class PlayCardGA : GameAction
{
    // FIX: Changed 'card' to 'Card'
    public Card Card { get; set; }

    public PlayCardGA(Card card)
    {
        Card = card; // Now this works perfectly!
    }
}