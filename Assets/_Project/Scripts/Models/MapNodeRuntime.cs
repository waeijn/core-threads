using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime data for a single node in the procedurally generated map DAG.
/// Created by MapGenerator; persists in GameState.GeneratedMap for the entire run.
/// </summary>
public class MapNodeRuntime
{
    /// <summary>What kind of encounter this node holds.</summary>
    public NodeType Type;

    /// <summary>Layer index (0 = first layer, highest = boss).</summary>
    public int Layer;

    /// <summary>Column index within the layer.</summary>
    public int Column;

    /// <summary>
    /// Enemy data for Combat nodes. Null for Rest/Treasure.
    /// Boss nodes use BossEnemy instead.
    /// </summary>
    public EnemyData[] Enemies;

    /// <summary>Boss-specific enemy data (only set on Boss nodes).</summary>
    public EnemyData BossEnemy;

    /// <summary>Nodes this node connects to (towards the boss).</summary>
    public List<MapNodeRuntime> Children = new();

    /// <summary>Nodes that connect into this node (towards the start).</summary>
    public List<MapNodeRuntime> Parents = new();

    // State
    /// <summary>Player has completed this node.</summary>
    public bool IsVisited;

    /// <summary>Player may click this node.</summary>
    public bool IsUnlocked;

    /// <summary>Locked out because player chose a sibling node.</summary>
    public bool IsLocked;

    /// <summary>Sprite sheet index for network_icons.png.</summary>
    public int BossSpriteIndex = 3;

    /// <summary>Cached anchored position in NodesContainer local space. Set by MapSystem during BuildMapUI.</summary>
    public Vector2 ViewLocalPos;

    public int SpriteIndex
    {
        get
        {
            return Type switch
            {
                NodeType.Combat   => 0,
                NodeType.Rest     => 1,
                NodeType.Treasure => 2,
                NodeType.Boss     => BossSpriteIndex,
                _ => 0
            };
        }
    }

    public override string ToString() =>
        $"[Layer {Layer} Col {Column} | {Type} | Visited:{IsVisited} Unlocked:{IsUnlocked} Locked:{IsLocked}]";
}
