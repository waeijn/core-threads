using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;

    /// <summary>
    /// Exposes enemy views so card effects (e.g. DealDamageEffect) can target them.
    /// </summary>
    public List<EnemyView> EnemyViews => enemyBoardView.EnemyViews;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<EnemyDrainGA>(EnemyDrainPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<EnemyDrainGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        foreach(var enemyData in enemyDatas)
        {
            enemyBoardView.AddEnemy(enemyData);
        }
    }

    // Performers

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        // Reset enemy block at start of enemy turn
        foreach (var e in enemyBoardView.EnemyViews) { if (!e.IsDead) e.ResetBlock(); }

        // Tick down Vulnerable on all enemies
        foreach (var e in enemyBoardView.EnemyViews) { if (!e.IsDead) e.TickVulnerable(); }

        // Tick down Vulnerable on hero
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null)
            HeroSystem.Instance.HeroView.TickVulnerable();

        // 1. Hide all intents at the start of the turn
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            if (!enemy.IsDead) enemy.HideIntent();
        }
        
        yield return new WaitForSeconds(0.2f);

        // 2. Perform AI-selected skills sequentially
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            if (enemy.IsDead) continue;

            if (enemy.PlannedMoves != null && enemy.PlannedMoves.Count > 0)
            {
                foreach (var move in enemy.PlannedMoves)
                {
                    if (enemy.IsDead) break;
                    yield return ExecuteEnemyMove(enemy, move);
                }
            }
            else
            {
                var fallback = new EnemyMove { Intent = enemy.CurrentIntent, OverrideValue = enemy.GetIntentValue(), Hits = 1 };
                yield return ExecuteEnemyMove(enemy, fallback);
            }
        }
        
        yield return new WaitForSeconds(0.2f);
    }

    /// <summary>
    /// Executes a single AI-selected skill: block/strength gains resolve immediately,
    /// attack hits (multi-hit, life drain) and player debuffs are queued as reactions in order.
    /// </summary>
    private IEnumerator ExecuteEnemyMove(EnemyView enemy, EnemyMove move)
    {
        int moveValue = enemy.ResolveMoveValue(move);
        int hits = move.GetEffectiveHits();
        var feedback = CombatFeedbackSystem.Instance;

        // 1. Defensive / pure-buff portions resolve first (enemy braces before striking)
        int blockGain = enemy.ResolveMoveBlock(move);
        if (blockGain > 0)
            enemy.GainBlock(blockGain);

        if (!move.IsAttack)
        {
            int strengthGain = move.Intent == EnemyIntent.Buff
                ? (moveValue > 0 ? moveValue : enemy.BuffAmount) + move.BonusStrength
                : move.BonusStrength;
            if (strengthGain > 0)
            {
                enemy.GainStrength(strengthGain);
                if (feedback != null)
                    feedback.SpawnFloatingText(enemy.SpriteTransform.position, $"+{strengthGain} STR", new Color(0.3f, 1f, 0.3f));
            }
        }

        // 2. Attack hits (any BonusStrength on an attack resolves right after the final hit)
        if (move.IsAttack)
        {
            for (int i = 0; i < hits; i++)
            {
                int bonusStr = (i == hits - 1) ? move.BonusStrength : 0;
                ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy, moveValue, move.LifeDrain, bonusStr));
            }
        }

        // 3. Debuffs applied to the player after the hits land
        if (move.ApplyVulnerable > 0 && HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null)
        {
            ActionSystem.Instance.AddReaction(new ApplyVulnerableGA(move.ApplyVulnerable, new List<CombatantView> { HeroSystem.Instance.HeroView }));
        }

        Debug.Log($"<color=#FF8800>[Enemy Skill]</color> {enemy.name} uses <b>{move.GetDisplayName()}</b> -> {move.GetEffectSummary(enemy.AttackPower, enemy.BlockPower, enemy.BuffAmount)}");

        yield return new WaitForSeconds(move.IsAttack ? 0.25f : 0.4f);
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        // 3. Roll next intents AT THE START of the player's next turn (which coincides with EnemyTurn POST reactions)
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            if (!enemy.IsDead)
            {
                enemy.RollNextIntent();
            }
        }
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        if (attacker == null || attacker.IsDead) yield break;

        // Play attack animation
        attacker.PlayAttackAnimation();
        // Move only the sprite so HP/ATK text stays in place
        Transform sprite = attacker.SpriteTransform;
        Tween tween = sprite.DOMoveX(sprite.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        sprite.DOMoveX(sprite.position.x + 1f, 0.25f);
        
        int baseDamage = attackHeroGA.OverrideBaseDamage ?? attacker.GetIntentValue();
        HeroView hero = HeroSystem.Instance.HeroView;
        int heroHpBefore = hero.CurrentHealth;
        DealDamageGA dealDamageGA = new(baseDamage + attacker.Strength, new() { hero });
        ActionSystem.Instance.AddReaction(dealDamageGA);

        if (attackHeroGA.LifeDrain)
            ActionSystem.Instance.AddReaction(new EnemyDrainGA(attacker, hero, heroHpBefore));

        if (attackHeroGA.BonusStrengthAfterHit > 0)
        {
            attacker.GainStrength(attackHeroGA.BonusStrengthAfterHit);
            var feedback = CombatFeedbackSystem.Instance;
            if (feedback != null)
                feedback.SpawnFloatingText(attacker.SpriteTransform.position, $"+{attackHeroGA.BonusStrengthAfterHit} STR", new Color(0.3f, 1f, 0.3f));
        }
    }

    private IEnumerator EnemyDrainPerformer(EnemyDrainGA drainGA)
    {
        if (drainGA.Enemy == null || drainGA.Enemy.IsDead || drainGA.Victim == null) yield break;

        int drained = Mathf.Max(0, drainGA.VictimHpBefore - drainGA.Victim.CurrentHealth);
        if (drained <= 0) yield break;

        drainGA.Enemy.Heal(drained);
        var feedback = CombatFeedbackSystem.Instance;
        if (feedback != null)
        {
            feedback.FlashSprite(drainGA.Enemy.spriteRenderer, new Color(0.2f, 1f, 0.3f, 1f));
            feedback.SpawnFloatingText(drainGA.Enemy.SpriteTransform.position, $"+{drained} DRAIN", new Color(0.4f, 1f, 0.4f));
        }
        yield return new WaitForSeconds(0.15f);
    }
}
