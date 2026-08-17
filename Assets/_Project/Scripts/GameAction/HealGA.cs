using UnityEngine;

public class HealGA : GameAction
{
    public int Amount { get; private set; }

    public HealGA(int amount)
    {
        Amount = amount;
    }
}
