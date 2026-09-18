using UnityEngine;

[System.Serializable]
public class RetainHandEffect : Effect
{
    public override GameAction GetGameAction()
    {
        return new RetainHandGA();
    }
}
