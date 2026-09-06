public class ApplyBlockNextTurnGA : GameAction
{
    public int Amount { get; private set; }
    public ApplyBlockNextTurnGA(int amount) { Amount = amount; }
}
