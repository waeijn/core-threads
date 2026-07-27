using UnityEngine;

[System.Serializable]
public class BlockEffect : Effect
{
    [SerializeField] private int blockAmount;

    public override GameAction GetGameAction()
    {
        GainBlockGA gainBlockGA = new(blockAmount);
        return gainBlockGA;
    }
}
