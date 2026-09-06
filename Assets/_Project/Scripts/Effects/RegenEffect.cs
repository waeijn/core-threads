using UnityEngine;

[System.Serializable]
public class RegenEffect : Effect
{
    [SerializeField] private int regenAmount;

    public override GameAction GetGameAction()
    {
        return new ApplyRegenGA(regenAmount);
    }
}
