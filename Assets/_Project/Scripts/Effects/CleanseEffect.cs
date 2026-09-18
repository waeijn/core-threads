using UnityEngine;

[System.Serializable]
public class CleanseEffect : Effect
{
    public override GameAction GetGameAction()
    {
        return new CleanseGA();
    }
}
