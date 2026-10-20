using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int AttackPower { get; private set; }
    [field: SerializeField] public int BlockPower { get; private set; }
    [field: SerializeField] public int BuffAmount { get; private set; }
    [field: SerializeField] public Vector3 Scale { get; private set; } = Vector3.one;

    [Header("Enemy AI — Internal Energy & Skill Deck")]
    [Tooltip("Maximum internal energy replenished at the start of each enemy turn.")]
    [field: SerializeField] public int MaxEnergy { get; private set; } = 3;

    [Tooltip("Number of skills drawn into the enemy's internal hand each turn.")]
    [field: SerializeField] public int SkillDrawCount { get; private set; } = 3;

    /// <summary>
    /// Returns effective max energy (defaults to 3 if unconfigured in existing assets).
    /// </summary>
    public int GetEffectiveMaxEnergy() => MaxEnergy > 0 ? MaxEnergy : 3;

    /// <summary>
    /// Returns effective skill draw count per turn (defaults to 3 if unconfigured in existing assets).
    /// </summary>
    public int GetEffectiveSkillDrawCount() => SkillDrawCount > 0 ? SkillDrawCount : 3;

    /// <summary>
    /// The AnimatorController for this enemy type (Idle, Attack, TakeDamage, etc.).
    /// Assigned at runtime to the EnemyView's Animator component.
    /// </summary>
    [field: SerializeField] public RuntimeAnimatorController AnimatorController { get; private set; }

    /// <summary>
    /// Health-threshold FSM states. Evaluated top-to-bottom;
    /// the first state whose threshold condition is met is used.
    /// If none match, falls back to the default MovePool.
    /// </summary>
    [field: SerializeField] public List<EnemyFSMState> FSMStates { get; private set; } = new();

    /// <summary>
    /// Fallback move pool used when no FSM state matches
    /// or when no FSM states are configured.
    /// </summary>
    [field: SerializeField] public List<EnemyMove> MovePool { get; private set; } = new();
}

/// <summary>
/// An FSM state tied to a health threshold range.
/// Example: "Aggressive" when HP is above 75%, "Desperate" when below 25%.
/// </summary>
[System.Serializable]
public class EnemyFSMState
{
    public string StateName;

    /// <summary>
    /// This state is active when: HP% >= MinHPPercent AND HP% <= MaxHPPercent.
    /// Values are 0-100.
    /// </summary>
    [Range(0, 100)] public int MinHPPercent;
    [Range(0, 100)] public int MaxHPPercent = 100;

    /// <summary>
    /// The weighted move pool for this FSM state.
    /// </summary>
    public List<EnemyMove> Moves = new();
}

/// <summary>
/// Represents a single skill/move in an enemy's internal deck.
/// </summary>
[System.Serializable]
public class EnemyMove
{
    [Tooltip("Optional custom skill name for AI logs and Archive display.")]
    public string SkillName;

    public EnemyIntent Intent;

    /// <summary>
    /// Relative weight / copies in the enemy's internal skill deck. Higher = more copies / higher base priority.
    /// </summary>
    [Range(1, 10)] public int Weight = 1;

    /// <summary>
    /// Internal energy cost required for the enemy AI to execute this skill.
    /// </summary>
    [Range(1, 3)] public int EnergyCost = 2;

    /// <summary>
    /// Override value for this move (e.g. custom damage amount).
    /// If 0, uses the enemy's default AttackPower/BlockPower.
    /// </summary>
    public int OverrideValue;

    [Header("Advanced Skill Effects")]
    [Tooltip("Number of times the attack portion hits (multi-hit). Strength applies per hit.")]
    [Range(1, 5)] public int Hits = 1;

    [Tooltip("Block gained by this skill. For AttackAndDefend this replaces the enemy's default BlockPower; for other intents it is extra block.")]
    public int BonusBlock;

    [Tooltip("Extra Strength gained by this skill on top of its primary effect.")]
    public int BonusStrength;

    [Tooltip("Vulnerable stacks applied to the player after this skill resolves.")]
    public int ApplyVulnerable;

    [Tooltip("If true, the enemy heals for the unblocked damage dealt by this skill.")]
    public bool LifeDrain;

    /// <summary>Hits clamped to at least 1 (older assets may serialize 0).</summary>
    public int GetEffectiveHits() => Mathf.Max(1, Hits);

    public bool IsAttack => Intent == EnemyIntent.Attack || Intent == EnemyIntent.AttackAndDefend;

    /// <summary>
    /// Short human-readable summary of what this skill does, given the owning enemy's base stats.
    /// Used by the Archive and tooltips.
    /// </summary>
    public string GetEffectSummary(int attackPower, int blockPower, int buffAmount)
    {
        var parts = new List<string>();
        int hits = GetEffectiveHits();
        switch (Intent)
        {
            case EnemyIntent.Attack:
            {
                int dmg = OverrideValue > 0 ? OverrideValue : attackPower;
                parts.Add(hits > 1 ? $"{dmg}x{hits} DMG" : $"{dmg} DMG");
                if (BonusBlock > 0) parts.Add($"{BonusBlock} BLK");
                break;
            }
            case EnemyIntent.Defend:
                parts.Add($"{(OverrideValue > 0 ? OverrideValue : blockPower) + BonusBlock} BLK");
                break;
            case EnemyIntent.AttackAndDefend:
            {
                int dmg = OverrideValue > 0 ? OverrideValue : attackPower;
                parts.Add(hits > 1 ? $"{dmg}x{hits} DMG" : $"{dmg} DMG");
                parts.Add($"{(BonusBlock > 0 ? BonusBlock : blockPower)} BLK");
                break;
            }
            case EnemyIntent.Buff:
                parts.Add($"+{(OverrideValue > 0 ? OverrideValue : buffAmount) + BonusStrength} STR");
                if (BonusBlock > 0) parts.Add($"{BonusBlock} BLK");
                break;
            case EnemyIntent.Debuff:
                parts.Add($"{ApplyVulnerable} VULN");
                if (BonusBlock > 0) parts.Add($"{BonusBlock} BLK");
                break;
        }
        if (Intent != EnemyIntent.Buff && BonusStrength > 0) parts.Add($"+{BonusStrength} STR");
        if (Intent != EnemyIntent.Debuff && ApplyVulnerable > 0) parts.Add($"{ApplyVulnerable} VULN");
        if (LifeDrain) parts.Add("DRAIN");
        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Returns the effective energy cost, falling back to a smart default based on Intent
    /// if the asset was serialized before EnergyCost was added (value == 0).
    /// </summary>
    public int GetEffectiveEnergyCost()
    {
        if (EnergyCost > 0) return EnergyCost;
        return Intent switch
        {
            EnemyIntent.AttackAndDefend => 3,
            EnemyIntent.Buff => 2,
            EnemyIntent.Attack => 2,
            EnemyIntent.Defend => 1,
            _ => 2
        };
    }

    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(SkillName)) return SkillName;
        return Intent switch
        {
            EnemyIntent.Attack => "Strike Protocol",
            EnemyIntent.Defend => "Firewall Guard",
            EnemyIntent.AttackAndDefend => "Counter-Breach",
            EnemyIntent.Buff => "Overclock Core",
            EnemyIntent.Debuff => "System Exploit",
            _ => Intent.ToString()
        };
    }
}

public enum EnemyIntent
{
    Attack,
    Defend,
    AttackAndDefend,
    Buff,
    Debuff
}
