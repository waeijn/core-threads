using System.Collections;
using UnityEngine;

public class HealSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<HealGA>(HealPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<HealGA>();
    }

    private IEnumerator HealPerformer(HealGA healGA)
    {
        // Heal the hero
        HeroSystem.Instance.HeroView.Heal(healGA.Amount);
        yield return null;
    }
}
