using UnityEngine;

[System.Serializable]
public class HealEffect : Effect
{
    [SerializeField] private int healAmount;

    public override GameAction GetGameAction()
    {
        HealGA healGA = new(healAmount);
        return healGA;
    }
}
