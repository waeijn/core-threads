using System.Collections;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
        ActionSystem.AttachPerformer<ApplyVulnerableGA>(ApplyVulnerablePerformer);
        ActionSystem.AttachPerformer<CleanseGA>(CleansePerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
        ActionSystem.DetachPerformer<ApplyVulnerableGA>();
        ActionSystem.DetachPerformer<CleanseGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)
        {
            int before = target.CurrentHealth + target.CurrentBlock;
            target.Damage(dealDamageGA.Amount);
            int actualDamage = before - (target.CurrentHealth + target.CurrentBlock);

            if (damageVFX != null)
                Instantiate(damageVFX, target.transform.position, Quaternion.identity);

            var feedback = CombatFeedbackSystem.Instance;
            if (feedback != null)
            {
                // Screen shake on every hit
                feedback.ScreenShake();

                // Red flash on the target sprite
                feedback.FlashSprite(target.spriteRenderer, new Color(1f, 0.2f, 0.2f, 1f));

                // Floating damage number (show actual damage dealt, minimum 0)
                if (actualDamage > 0)
                    feedback.SpawnFloatingText(target.SpriteTransform.position, $"-{actualDamage}", Color.red);
                else
                    feedback.SpawnFloatingText(target.SpriteTransform.position, "BLOCK", Color.cyan);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator ApplyVulnerablePerformer(ApplyVulnerableGA applyVulnerableGA)
    {
        foreach (var target in applyVulnerableGA.Targets)
        {
            target.ApplyVulnerable(applyVulnerableGA.Amount);

            var feedback = CombatFeedbackSystem.Instance;
            if (feedback != null)
            {
                feedback.SpawnFloatingText(target.SpriteTransform.position, $"VULNERABLE +{applyVulnerableGA.Amount}", new Color(1f, 0.5f, 0f));
            }

            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator CleansePerformer(CleanseGA cleanseGA)
    {
        var hero = HeroSystem.Instance.HeroView;
        if (hero != null)
        {
            hero.Cleanse();

            var feedback = CombatFeedbackSystem.Instance;
            if (feedback != null)
            {
                feedback.SpawnFloatingText(hero.SpriteTransform.position, "CLEANSED", Color.green);
            }
        }
        yield return null;
    }
}