using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a procedural DAG map for a given act using strict spacing and progression rules.
/// </summary>
public static class MapGenerator
{
    private static MapConfig _cfg;

    public static List<List<MapNodeRuntime>> GenerateMap(int act, MapConfig config)
    {
        _cfg = config;

        int startingPaths  = GetStartingPaths(act);
        int middleLayers   = GetMiddleLayers(act);
        int bossSpriteIdx  = GetBossSpriteIndex(act);

        int totalLayers = 1 + middleLayers + 1;
        List<List<MapNodeRuntime>> map = new();

        // ── 1. BUILD SKELETON ──────────────────────────────────────────────
        
        // Layer 0
        var layer0 = new List<MapNodeRuntime>();
        for (int col = 0; col < startingPaths; col++)
        {
            layer0.Add(new MapNodeRuntime { Type = NodeType.Combat, Layer = 0, Column = col, IsUnlocked = true });
        }
        map.Add(layer0);

        // Middle Layers
        for (int l = 1; l <= middleLayers; l++)
        {
            var layer = new List<MapNodeRuntime>();
            for (int col = 0; col < startingPaths; col++)
            {
                layer.Add(new MapNodeRuntime { Type = NodeType.Combat, Layer = l, Column = col });
            }
            map.Add(layer);
            ConnectLayers(map[l - 1], layer, allowCrossConnect: (l < middleLayers));
        }

        // Boss Layer
        var bossLayer = new List<MapNodeRuntime>();
        var bossNode = new MapNodeRuntime
        {
            Type = NodeType.Boss,
            Layer = totalLayers - 1,
            Column = 0,
            BossEnemy = GetBossEnemy(act),
            BossSpriteIndex = bossSpriteIdx
        };
        bossLayer.Add(bossNode);
        map.Add(bossLayer);

        // Connect last middle layer to Boss
        foreach (var parent in map[map.Count - 2])
        {
            AddEdge(parent, bossNode);
        }

        // ── 2. APPLY STRICT NODE TYPING RULES ──────────────────────────────
        AssignNodeTypes(map, middleLayers);

        // ── 3. POPULATE ENCOUNTERS ─────────────────────────────────────────
        EncounterPoolManager.AssignEncounters(map, act, GetRoster(act), GetRoster(act - 1));

        GameState.GeneratedMap = map;
        return map;
    }

    private static void AssignNodeTypes(List<List<MapNodeRuntime>> map, int middleLayers)
    {
        for (int l = 1; l <= middleLayers; l++)
        {
            bool isLastMiddleLayer = (l == middleLayers);
            bool isPenultimate = (l == middleLayers - 1);

            foreach (var node in map[l])
            {
                bool hasNonCombatParent = false;
                int consecutiveCombatParents = 0;

                foreach (var p in node.Parents)
                {
                    if (p.Type == NodeType.Rest || p.Type == NodeType.Treasure)
                    {
                        hasNonCombatParent = true;
                    }
                    else if (p.Type == NodeType.Combat)
                    {
                        bool gpCombat = true;
                        foreach (var gp in p.Parents)
                        {
                            if (gp.Type != NodeType.Combat) gpCombat = false;
                        }
                        if (gpCombat) consecutiveCombatParents++;
                    }
                }

                // RULE: No Double Safety (Rest/Treasure cannot connect to Rest/Treasure)
                if (hasNonCombatParent)
                {
                    node.Type = NodeType.Combat;
                }
                // RULE: Pre-Boss Guarantee Setup (Force penultimate to combat so last layer can be safe)
                else if (isPenultimate && middleLayers > 1)
                {
                    node.Type = NodeType.Combat;
                }
                // RULE: Pre-Boss Guarantee (Ensure a mix of Rest and Treasure, no identical choices)
                else if (isLastMiddleLayer)
                {
                    if (node.Column == map[l].Count - 1 && node.Column > 0)
                    {
                        // On the final node of the layer, check if all previous nodes were the same type
                        bool allRest = true;
                        bool allTreasure = true;
                        for (int i = 0; i < node.Column; i++)
                        {
                            if (map[l][i].Type != NodeType.Rest) allRest = false;
                            if (map[l][i].Type != NodeType.Treasure) allTreasure = false;
                        }

                        if (allRest) node.Type = NodeType.Treasure;
                        else if (allTreasure) node.Type = NodeType.Rest;
                        else node.Type = (Random.value < 0.75f) ? NodeType.Rest : NodeType.Treasure;
                    }
                    else
                    {
                        node.Type = (Random.value < 0.75f) ? NodeType.Rest : NodeType.Treasure;
                    }
                }
                // Standard Middle Layer Logic
                else
                {
                    float nonCombatChance = 0.35f;
                    if (Random.value < nonCombatChance)
                    {
                        // RULE: High Risk/Reward Lane (Weight Database if preceded by consecutive bugs)
                        float treasureChance = (consecutiveCombatParents > 0) ? 0.7f : 0.3f;
                        node.Type = (Random.value < treasureChance) ? NodeType.Treasure : NodeType.Rest;
                    }
                    else
                    {
                        node.Type = NodeType.Combat;
                    }
                }
            }
        }
    }

    private static void ConnectLayers(List<MapNodeRuntime> parentLayer, List<MapNodeRuntime> childLayer, bool allowCrossConnect)
    {
        int pCount = parentLayer.Count;
        int cCount = childLayer.Count;

        // Pass 1: Direct same-column
        for (int i = 0; i < pCount; i++)
        {
            int childCol = Mathf.Clamp(i, 0, cCount - 1);
            AddEdge(parentLayer[i], childLayer[childCol]);
        }

        // Pass 2: Ensure no orphans
        for (int j = 0; j < cCount; j++)
        {
            if (childLayer[j].Parents.Count == 0)
            {
                int parentCol = Mathf.Clamp(j, 0, pCount - 1);
                AddEdge(parentLayer[parentCol], childLayer[j]);
            }
        }

        // Pass 3: Cross-connections (RULE: Max 2 outbound connections)
        if (allowCrossConnect)
        {
            for (int i = 0; i < pCount; i++)
            {
                if (Random.value < 0.5f && parentLayer[i].Children.Count < 2)
                {
                    int crossCol = (i % 2 == 0)
                        ? Mathf.Min(i + 1, cCount - 1)
                        : Mathf.Max(i - 1, 0);

                    if (crossCol != i)
                    {
                        AddEdge(parentLayer[i], childLayer[crossCol]);
                    }
                }
            }
        }
    }

    private static void AddEdge(MapNodeRuntime parent, MapNodeRuntime child)
    {
        if (!parent.Children.Contains(child)) parent.Children.Add(child);
        if (!child.Parents.Contains(parent)) child.Parents.Add(parent);
    }

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

    public static ActEnemyRoster GetRoster(int act) => act switch
    {
        1 => _cfg.act1Roster,
        2 => _cfg.act2Roster,
        3 => _cfg.act3Roster,
        _ => _cfg.act1Roster
    };

    private static EnemyData GetBossEnemy(int act)
    {
        var roster = GetRoster(act);
        return roster != null ? roster.BossEnemy : null;
    }
}
