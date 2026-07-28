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

    /// <summary>
    /// The weighted move pool for AI intent selection.
    /// Each entry is a move the enemy can perform with a relative weight.
    /// </summary>
    [field: SerializeField] public List<EnemyMove> MovePool { get; private set; } = new();
}

/// <summary>
/// Represents a single move an enemy can perform.
/// Slay the Spire style: enemies telegraph their intent each turn.
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
