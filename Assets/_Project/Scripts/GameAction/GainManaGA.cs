public class GainManaGA : GameAction
{
    public int Amount { get; private set; }

    public GainManaGA(int amount)
    {
        Amount = amount;
    }
}
