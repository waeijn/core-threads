using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text intentText;

    public int AttackPower { get; set; }
    public int BlockPower { get; set; }
    public int BuffAmount { get; set; }
    public EnemyIntent CurrentIntent { get; private set; }

    private EnemyData enemyData;

    public void Setup(EnemyData data)
    {
        enemyData = data;
        AttackPower = data.AttackPower;
        BlockPower = data.BlockPower;
        BuffAmount = data.BuffAmount;
        UpdateAttackText();
        SetupBase(data.Health, data.Image);
        RollNextIntent();
    }

    /// <summary>
    /// Picks the next intent from the enemy's weighted move pool.
    /// If no move pool is configured, defaults to Attack.
    /// </summary>
    public void RollNextIntent()
    {
        if (enemyData.MovePool == null || enemyData.MovePool.Count == 0)
        {
            CurrentIntent = EnemyIntent.Attack;
            UpdateIntentDisplay();
            return;
        }

        // Weighted random selection
        int totalWeight = 0;
        foreach (var move in enemyData.MovePool)
            totalWeight += move.Weight;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var move in enemyData.MovePool)
        {
            cumulative += move.Weight;
            if (roll < cumulative)
            {
                CurrentIntent = move.Intent;
                UpdateIntentDisplay();
                return;
            }
        }

        // Fallback
        CurrentIntent = EnemyIntent.Attack;
        UpdateIntentDisplay();
    }

    /// <summary>
    /// Returns the value associated with the current intent's move,
    /// or falls back to the enemy's default stat if no override is set.
    /// </summary>
    public int GetIntentValue()
    {
        if (enemyData.MovePool != null)
        {
            foreach (var move in enemyData.MovePool)
            {
                if (move.Intent == CurrentIntent && move.OverrideValue > 0)
                    return move.OverrideValue;
            }
        }

        return CurrentIntent switch
        {
            EnemyIntent.Attack => AttackPower,
            EnemyIntent.Defend => BlockPower,
            EnemyIntent.Buff => BuffAmount,
            _ => AttackPower
        };
    }

    private void UpdateAttackText()
    {
        attackText.text = "ATK: " + AttackPower;
    }

    private void UpdateIntentDisplay()
    {
        if (intentText == null) return;

        switch (CurrentIntent)
        {
            case EnemyIntent.Attack:
                intentText.text = $"<color=#FF4444>⚔ ATK {AttackPower}</color>";
                break;
            case EnemyIntent.Defend:
                intentText.text = $"<color=#4488FF>🛡 DEF {BlockPower}</color>";
                break;
            case EnemyIntent.AttackAndDefend:
                intentText.text = $"<color=#FFAA00>⚔ {AttackPower} + 🛡 {BlockPower}</color>";
                break;
            case EnemyIntent.Buff:
                intentText.text = $"<color=#44FF44>▲ BUFF +{BuffAmount}</color>";
                break;
        }
    }
}