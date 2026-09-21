using System.Collections;
using UnityEngine;

public class BlockSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<GainBlockGA>(GainBlockPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<GainBlockGA>();
    }

    private IEnumerator GainBlockPerformer(GainBlockGA gainBlockGA)
    {
        AudioSystem.Instance?.PlayBlock();
        var heroView = HeroSystem.Instance.HeroView;
        heroView.GainBlock(gainBlockGA.Amount);

        var feedback = CombatFeedbackSystem.Instance;
        if (feedback != null)
        {
            feedback.FlashSprite(heroView.spriteRenderer, new Color(0.2f, 0.8f, 1f, 1f));
            feedback.PlayBlockVFX(heroView.transform.position);
            // Block floating text
            feedback.SpawnFloatingText(heroView.SpriteTransform.position, $"+{gainBlockGA.Amount} Block", new Color(0.4f, 0.8f, 1f));
        }

        yield return null;
    }
}
