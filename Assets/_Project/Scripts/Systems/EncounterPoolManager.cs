using System.Collections.Generic;
using UnityEngine;

public static class EncounterPoolManager
{
    /// <summary>
    /// Distributes enemies across all combat nodes in the map according to layering rules.
    /// </summary>
    public static void AssignEncounters(List<List<MapNodeRuntime>> map, int act, ActEnemyRoster roster, ActEnemyRoster prevRoster)
    {
        if (roster == null || roster.StandardEnemies == null || roster.StandardEnemies.Length == 0)
        {
            Debug.LogError($"[EncounterPoolManager] Missing or empty roster for Act {act}!");
            return;
        }

        // 1. Gather all combat nodes (boss is handled separately by MapGenerator)
        List<MapNodeRuntime> combatNodes = new List<MapNodeRuntime>();
        foreach (var layer in map)
        {
            foreach (var node in layer)
            {
                if (node.Type == NodeType.Combat)
                {
                    combatNodes.Add(node);
                }
            }
        }

        // Sort by layer so we process earliest nodes first
        combatNodes.Sort((a, b) => a.Layer.CompareTo(b.Layer));

        // 2. Build our pools
        List<EnemyData> nativePool = new List<EnemyData>(roster.StandardEnemies);
        List<EnemyData> crossoverPool = new List<EnemyData>();
        if (prevRoster != null && prevRoster.StandardEnemies != null)
        {
            crossoverPool.AddRange(prevRoster.StandardEnemies);
        }

        // 3. To ensure every native enemy is seen at least once, we keep a queue
        Queue<EnemyData> requiredNativeEnemies = new Queue<EnemyData>(nativePool);

        // 4. Assign an enemy to each combat node
        foreach (var node in combatNodes)
        {
            node.Enemies = new EnemyData[1];

            // Layer pacing logic:
            // Layer 0-1 (early) has a chance to spawn crossover enemies from the previous act
            if (node.Layer <= 1 && crossoverPool.Count > 0 && Random.value < 0.4f)
            {
                node.Enemies[0] = crossoverPool[Random.Range(0, crossoverPool.Count)];
                continue;
            }

            // Assign from the required native queue first to guarantee coverage
            if (requiredNativeEnemies.Count > 0)
            {
                node.Enemies[0] = requiredNativeEnemies.Dequeue();
            }
            else
            {
                // Once we've shown all required native enemies, pick a random native enemy
                // (Could add weighted FSM rules here later)
                node.Enemies[0] = nativePool[Random.Range(0, nativePool.Count)];
            }
        }
    }
}
