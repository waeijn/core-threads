using UnityEngine;

public class DrawCardsGA : GameAction
{
    public int Amount { get; set; }

    public DrawCardsGA(int amount)
    {
        Amount = amount;    
    }
}
