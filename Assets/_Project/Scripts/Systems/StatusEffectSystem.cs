using System.Collections;
using UnityEngine;

public class StatusEffectSystem : Singleton<StatusEffectSystem>
{
    public int RegenStacks { get; private set; }
    public int BlockNextTurnStacks { get; private set; }
    public int EnergyPerTurnStacks { get; private set; }

    void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
        ActionSystem.AttachPerformer<ApplyRegenGA>(ApplyRegenPerformer);
        ActionSystem.AttachPerformer<ApplyBlockNextTurnGA>(ApplyBlockNextTurnPerformer);
        ActionSystem.AttachPerformer<ApplyEnergyPerTurnGA>(ApplyEnergyPerTurnPerformer);
    }

    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
        ActionSystem.DetachPerformer<ApplyRegenGA>();
        ActionSystem.DetachPerformer<ApplyBlockNextTurnGA>();
        ActionSystem.DetachPerformer<ApplyEnergyPerTurnGA>();
    }

    public void AddRegen(int amount)
    {
        RegenStacks += amount;
        Debug.Log($"Added {amount} Regen. Total: {RegenStacks}");
    }

    public void AddBlockNextTurn(int amount)
    {
        BlockNextTurnStacks += amount;
        Debug.Log($"Added {amount} Block Next Turn. Total: {BlockNextTurnStacks}");
    }

    public void AddEnergyPerTurn(int amount)
    {
        EnergyPerTurnStacks += amount;
        Debug.Log($"Added {amount} Energy Per Turn. Total: {EnergyPerTurnStacks}");
    }

    public void ClearAll()
    {
        RegenStacks = 0;
        BlockNextTurnStacks = 0;
        EnergyPerTurnStacks = 0;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        StartCoroutine(ProcessTurnStartEffects());
    }

    private IEnumerator ProcessTurnStartEffects()
    {
        // 1. Process Energy Per Turn
        if (EnergyPerTurnStacks > 0)
        {
            ActionSystem.Instance.AddReaction(new GainManaGA(EnergyPerTurnStacks));
        }

        // 2. Process Regen
        if (RegenStacks > 0)
        {
            ActionSystem.Instance.AddReaction(new HealGA(RegenStacks));
            
            // Slay the Spire style Regen might decrement each turn, but for now we'll just heal.
            // If it's a permanent buff (like Metallicize but for HP), it stays.
            // Wait, "Gain 2 HP at start of turn" - usually means it lasts forever unless specified.
        }

        // 3. Process Block Next Turn
        if (BlockNextTurnStacks > 0)
        {
            ActionSystem.Instance.AddReaction(new GainBlockGA(BlockNextTurnStacks));
            
            // Block Next Turn is a one-time thing, so we consume it
            BlockNextTurnStacks = 0;
        }

        yield return null;
    }

    private IEnumerator ApplyRegenPerformer(ApplyRegenGA applyRegenGA)
    {
        AddRegen(applyRegenGA.Amount);
        yield return null;
    }

    private IEnumerator ApplyBlockNextTurnPerformer(ApplyBlockNextTurnGA applyBlockNextTurnGA)
    {
        AddBlockNextTurn(applyBlockNextTurnGA.Amount);
        yield return null;
    }

    private IEnumerator ApplyEnergyPerTurnPerformer(ApplyEnergyPerTurnGA applyEnergyPerTurnGA)
    {
        AddEnergyPerTurn(applyEnergyPerTurnGA.Amount);
        yield return null;
    }
}

