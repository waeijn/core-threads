using UnityEngine;

public class SpendManaGA : GameAction
{
    public int Amount { get; set;  }

    public SpendManaGA(int amount)
    {
        Amount = amount;
    }
}
