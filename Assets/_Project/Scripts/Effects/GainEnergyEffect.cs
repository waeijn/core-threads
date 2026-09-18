using UnityEngine;

[System.Serializable]
public class GainEnergyEffect : Effect
{
    [SerializeField] private int energyAmount;

    public override GameAction GetGameAction()
    {
        return new GainManaGA(energyAmount);
    }
}
