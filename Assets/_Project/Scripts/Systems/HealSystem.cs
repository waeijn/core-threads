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
        AudioSystem.Instance?.PlayHeal();
        var heroView = HeroSystem.Instance.HeroView;
        heroView.Heal(healGA.Amount);

        var feedback = CombatFeedbackSystem.Instance;
        if (feedback != null)
        {
            feedback.FlashSprite(heroView.spriteRenderer, new Color(0.2f, 1f, 0.3f, 1f));
            feedback.PlayHealVFX(heroView.transform.position);
            feedback.SpawnFloatingText(heroView.SpriteTransform.position, $"+{healGA.Amount} HP", Color.green);
        }

        yield return null;
    }
}
