using System.Collections;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }

    // Performers

private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
{
    // Safety Check 1: Did the card actually have an effect attached?
    if (performEffectGA.Effect == null)
    {
        Debug.LogError("CRASH PREVENTED: The card tried to play an effect, but the effect data was null! Check your Card Data in the Unity Inspector.");
        yield break; // Stops the method here so it doesn't crash
    }

    GameAction effectAction = performEffectGA.Effect.GetGameAction();

    // Safety Check 2: Did the effect successfully generate an action?
    if (effectAction == null)
    {
        Debug.LogError("CRASH PREVENTED: GetGameAction() returned nothing. Check the script for this specific effect.");
        yield break;
    }

    // Safety Check 3: Is the Action System in the scene?
    if (ActionSystem.Instance == null)
    {
        Debug.LogError("CRASH PREVENTED: ActionSystem.Instance is null! Did you forget to put the Action System in your scene?");
        yield break;
    }

    // If it survives all the checks, it's safe to run!
    ActionSystem.Instance.AddReaction(effectAction);
    yield return null;
}
}