using System.Collections.Generic;

public class ApplyVulnerableGA : GameAction
{
    public int Amount { get; private set; }
    public List<CombatantView> Targets { get; private set; }

    public ApplyVulnerableGA(int amount, List<CombatantView> targets)
    {
        Amount = amount;
        Targets = targets;
    }
}
