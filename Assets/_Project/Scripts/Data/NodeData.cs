using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The type of encounter a map node represents.
/// </summary>
public enum NodeType
{
    Combat,
    Rest,
    Treasure,
    Boss
}

[CreateAssetMenu(menuName = "Data/Node")]
public class NodeData : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public NodeType Type { get; private set; }

    /// <summary>Regular combat nodes: one or more enemies to fight.</summary>
    [field: SerializeField] public List<EnemyData> Enemies { get; private set; } = new();

    /// <summary>Boss nodes: single boss enemy reference.</summary>
    [field: SerializeField] public EnemyData BossEnemy { get; private set; }

    [field: SerializeField] public Sprite MapBackground { get; private set; }
}
