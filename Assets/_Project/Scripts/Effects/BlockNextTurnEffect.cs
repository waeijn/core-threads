using UnityEngine;

[System.Serializable]
public class BlockNextTurnEffect : Effect
{
    [SerializeField] private int blockAmount;

    public override GameAction GetGameAction()
    {
        return new ApplyBlockNextTurnGA(blockAmount);
    }
}
