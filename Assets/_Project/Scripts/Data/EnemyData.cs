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

    [Header("Audio")]
    [field: SerializeField] public AudioClip AttackSound { get; private set; }
    [field: SerializeField] public AudioClip DamageSound { get; private set; }

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
/// Represents a single move an enemy can perform.
/// </summary>
[System.Serializable]
public class EnemyMove
{
    public EnemyIntent Intent;

    /// <summary>
    /// Relative weight for random selection. Higher = more likely.
    /// </summary>
    [Range(1, 10)] public int Weight = 1;

    /// <summary>
    /// Override value for this move (e.g. custom damage amount).
    /// If 0, uses the enemy's default AttackPower/BlockPower.
    /// </summary>
    public int OverrideValue;
}

public enum EnemyIntent
{
    Attack,
    Defend,
    AttackAndDefend,
    Buff
}
