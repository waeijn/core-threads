using System.Collections;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] private ManaUI manaUI;

    private int currentMana;

    void Start()
    {
        currentMana = GameState.PlayerMaxMana;
        manaUI.UpdateManaText(currentMana);
    }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
        ActionSystem.AttachPerformer<GainManaGA>(GainManaPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<RefillManaGA>();
        ActionSystem.DetachPerformer<GainManaGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public bool HasEnoughMana(int mana)
    {
        return currentMana >= mana;
    }

    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        currentMana -= spendManaGA.Amount;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        currentMana = GameState.PlayerMaxMana;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }

    private IEnumerator GainManaPerformer(GainManaGA gainManaGA)
    {
        currentMana += gainManaGA.Amount;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);
    }
}