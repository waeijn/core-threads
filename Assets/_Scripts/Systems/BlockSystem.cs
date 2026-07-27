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
        // Grant block to the hero
        HeroSystem.Instance.HeroView.GainBlock(gainBlockGA.Amount);
        yield return null;
    }
}
