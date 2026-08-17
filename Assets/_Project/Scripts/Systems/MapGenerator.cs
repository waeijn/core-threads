using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a procedural DAG map for a given act.
/// Layers are ordered from bottom (Layer 0) to top (Boss layer).
/// </summary>
public static class MapGenerator
{
    private static MapConfig _cfg;

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a fresh DAG map for the given act and stores it in GameState.GeneratedMap.
    /// Returns the generated map.
    /// </summary>
    public static List<List<MapNodeRuntime>> GenerateMap(int act, MapConfig config)
    {
        _cfg = config;

        int startingPaths  = GetStartingPaths(act);
        int middleLayers   = GetMiddleLayers(act);
        int bossSpriteIdx  = GetBossSpriteIndex(act);

        // Total layers: Layer0 + middleLayers + BossLayer
        int totalLayers = 1 + middleLayers + 1;

        List<List<MapNodeRuntime>> map = new();

        // ── Layer 0: All Coupler (forced Combat) ───────────────────────────
        var layer0 = new List<MapNodeRuntime>();
        for (int col = 0; col < startingPaths; col++)
        {
            var node = new MapNodeRuntime
            {
                Type    = NodeType.Combat,
                Layer   = 0,
                Column  = col,
                Enemies = new[] { GetLayer0Enemy(act) },
                IsUnlocked = true   // Layer 0 always starts unlocked
            };
            layer0.Add(node);
        }
        map.Add(layer0);

        // ── Middle Layers ──────────────────────────────────────────────────
        for (int l = 1; l <= middleLayers; l++)
        {
            bool isLastMiddleLayer = (l == middleLayers);
            int nodeCount = startingPaths;

            var layer = new List<MapNodeRuntime>();

            if (act == 1)
            {
                // Act 1 Exception: Layer 1 = Corrupted (Combat), Layer 2 = Rest vs Treasure
                if (isLastMiddleLayer)
                {
                    bool swap = Random.value > 0.5f;
                    for (int col = 0; col < nodeCount; col++)
                    {
                        bool isRest = (col % 2 == 0) ? !swap : swap;
                        NodeType type = isRest ? NodeType.Rest : NodeType.Treasure;
                        layer.Add(new MapNodeRuntime { Type = type, Layer = l, Column = col });
                    }
                }
                else
                {
                    for (int col = 0; col < nodeCount; col++)
                    {
                        layer.Add(new MapNodeRuntime
                        {
                            Type    = NodeType.Combat,
                            Layer   = l,
                            Column  = col,
                            Enemies = new[] { GetLayer1Enemy(act) }
                        });
                    }
                }
            }
            else
            {
                // Acts 2 & 3: Flexible Rest & Treasure placement allowing early choices and multiple Rest/Treasure per run
                // Determine if this layer features Non-Combat choices (e.g. early layer 1/2 or late layer N-1/N)
                bool isNonCombatLayer = isLastMiddleLayer || (l == 1 && Random.value < 0.65f) || (l == 2 && Random.value < 0.5f);

                if (isNonCombatLayer)
                {
                    bool swap = Random.value > 0.5f;

                    for (int col = 0; col < nodeCount; col++)
                    {
                        // Check previous layer same-column type to avoid back-to-back Rest
                        NodeType prevType = map[l - 1][Mathf.Clamp(col, 0, map[l - 1].Count - 1)].Type;

                        NodeType type;
                        if (col % 3 == 0)
                            type = swap ? NodeType.Rest : NodeType.Treasure;
                        else if (col % 3 == 1)
                            type = swap ? NodeType.Treasure : NodeType.Rest;
                        else
                            type = NodeType.Combat;

                        // Prevent consecutive Rest nodes on same column
                        if (type == NodeType.Rest && prevType == NodeType.Rest)
                        {
                            type = NodeType.Treasure;
                        }

                        layer.Add(new MapNodeRuntime
                        {
                            Type    = type,
                            Layer   = l,
                            Column  = col,
                            Enemies = type == NodeType.Combat ? new[] { GetLayer1Enemy(act) } : null
                        });
                    }
                }
                else
                {
                    // Combat layer
                    for (int col = 0; col < nodeCount; col++)
                    {
                        layer.Add(new MapNodeRuntime
                        {
                            Type    = NodeType.Combat,
                            Layer   = l,
                            Column  = col,
                            Enemies = new[] { GetLayer1Enemy(act) }
                        });
                    }
                }
            }

            map.Add(layer);
            ConnectLayers(map[l - 1], layer, allowCrossConnect: !isLastMiddleLayer);
        }

        // ── Boss Layer ─────────────────────────────────────────────────────
        var bossLayer = new List<MapNodeRuntime>();
        var bossNode = new MapNodeRuntime
        {
            Type            = NodeType.Boss,
            Layer           = totalLayers - 1,
            Column          = 0,
            BossEnemy       = GetBossEnemy(act),
            BossSpriteIndex = bossSpriteIdx
        };
        bossLayer.Add(bossNode);
        map.Add(bossLayer);

        // Connect last middle layer → boss
        var lastMiddle = map[map.Count - 2];
        foreach (var parent in lastMiddle)
        {
            parent.Children.Add(bossNode);
            bossNode.Parents.Add(parent);
        }

        GameState.GeneratedMap = map;
        return map;
    }

    // ── Connection Logic ───────────────────────────────────────────────────

    /// <summary>
    /// Connects each node in parentLayer to 1–2 nodes in childLayer.
    /// Ensures every child node has at least one parent (no orphans).
    /// Cross-connections (adjacent columns) are added for path switching.
    /// </summary>
    private static void ConnectLayers(
        List<MapNodeRuntime> parentLayer,
        List<MapNodeRuntime> childLayer,
        bool allowCrossConnect)
    {
        int pCount = parentLayer.Count;
        int cCount = childLayer.Count;

        // First pass: connect each parent to same-column child
        for (int i = 0; i < pCount; i++)
        {
            int childCol = Mathf.Clamp(i, 0, cCount - 1);
            AddEdge(parentLayer[i], childLayer[childCol]);
        }

        // Second pass: ensure every child has at least one parent
        for (int j = 0; j < cCount; j++)
        {
            if (childLayer[j].Parents.Count == 0)
            {
                int parentCol = Mathf.Clamp(j, 0, pCount - 1);
                AddEdge(parentLayer[parentCol], childLayer[j]);
            }
        }

        // Third pass: cross-connections for path switching (50% chance per parent)
        if (allowCrossConnect)
        {
            for (int i = 0; i < pCount; i++)
            {
                if (Random.value < 0.5f)
                {
                    // Connect to adjacent column child (left or right)
                    int crossCol = (i % 2 == 0)
                        ? Mathf.Min(i + 1, cCount - 1)
                        : Mathf.Max(i - 1, 0);

                    if (crossCol != i) // Avoid duplicate to same col
                    {
                        AddEdge(parentLayer[i], childLayer[crossCol]);
                    }
                }
            }
        }
    }

    private static void AddEdge(MapNodeRuntime parent, MapNodeRuntime child)
    {
        if (!parent.Children.Contains(child))
            parent.Children.Add(child);
        if (!child.Parents.Contains(parent))
            child.Parents.Add(parent);
    }

    // ── Act Config Helpers ─────────────────────────────────────────────────

    private static int GetStartingPaths(int act) => act switch
    {
        1 => _cfg.act1StartingPaths,
        2 => _cfg.act2StartingPaths,
        3 => _cfg.act3StartingPaths,
        _ => 2
    };

    private static int GetMiddleLayers(int act) => act switch
    {
        1 => _cfg.act1MiddleLayers,
        2 => _cfg.act2MiddleLayers,
        3 => _cfg.act3MiddleLayers,
        _ => 2
    };

    private static int GetBossSpriteIndex(int act) => act switch
    {
        1 => _cfg.act1BossSpriteIndex,
        2 => _cfg.act2BossSpriteIndex,
        3 => _cfg.act3BossSpriteIndex,
        _ => 3
    };

    private static EnemyData GetLayer0Enemy(int act) =>
        act == 1 ? _cfg.act1Layer0Enemy : _cfg.act1Layer0Enemy; // Placeholder same for all acts

    private static EnemyData GetLayer1Enemy(int act) =>
        act == 1 ? _cfg.act1Layer1Enemy : _cfg.act1Layer1Enemy; // Placeholder

    private static EnemyData GetBossEnemy(int act) => act switch
    {
        1 => _cfg.act1BossEnemy,
        2 => _cfg.act2BossEnemy,
        3 => _cfg.act3BossEnemy,
        _ => _cfg.act1BossEnemy
    };
}
