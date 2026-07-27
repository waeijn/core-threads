using UnityEngine;

public class GainBlockGA : GameAction
{
    public int Amount { get; private set; }

    public GainBlockGA(int amount)
    {
        Amount = amount;
    }
}
