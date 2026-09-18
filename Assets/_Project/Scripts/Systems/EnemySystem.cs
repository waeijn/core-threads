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
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
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

        // 2. Perform actions sequentially (or queue them)
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            if (enemy.IsDead) continue;

            switch (enemy.CurrentIntent)
            {
                case EnemyIntent.Attack:
                    ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy));
                    break;

                case EnemyIntent.Defend:
                    enemy.GainBlock(enemy.GetIntentValue());
                    yield return new WaitForSeconds(0.4f);
                    break;

                case EnemyIntent.AttackAndDefend:
                    enemy.GainBlock(enemy.BlockPower);
                    ActionSystem.Instance.AddReaction(new AttackHeroGA(enemy));
                    break;

                case EnemyIntent.Buff:
                    enemy.GainStrength(enemy.BuffAmount);
                    yield return new WaitForSeconds(0.4f);
                    break;
            }
        }
        
        yield return new WaitForSeconds(0.2f);
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
        // Play attack animation
        attacker.PlayAttackAnimation();
        // Move only the sprite so HP/ATK text stays in place
        Transform sprite = attacker.SpriteTransform;
        Tween tween = sprite.DOMoveX(sprite.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        sprite.DOMoveX(sprite.position.x + 1f, 0.25f);
        
        int baseDamage = attacker.GetIntentValue();
        DealDamageGA dealDamageGA = new(baseDamage + attacker.Strength, new() { HeroSystem.Instance.HeroView });
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }
}