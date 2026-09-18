using UnityEngine;

[System.Serializable]
public class EnergyPerTurnEffect : Effect
{
    [SerializeField] private int energyAmount;

    public override GameAction GetGameAction()
    {
        return new ApplyEnergyPerTurnGA(energyAmount);
    }
}
