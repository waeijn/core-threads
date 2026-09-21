using UnityEngine;
using UnityEditor;

public class RosterSetupUtility : Editor
{
    [MenuItem("Tools/Setup Enemy Rosters")]
    public static void SetupRosters()
    {
        string rosterFolder = "Assets/_Project/Data/Rosters";
        if (!AssetDatabase.IsValidFolder(rosterFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Data", "Rosters");
        }

        // 1. Act 1 Roster
        ActEnemyRoster act1 = GetOrCreateRoster(rosterFolder, "Act1Roster");
        act1.ActNumber = 1;
        act1.StandardEnemies = new[] {
            LoadEnemy("Coupler"),
            LoadEnemy("Corrupted")
        };
        act1.BossEnemy = LoadEnemy("Collector");
        EditorUtility.SetDirty(act1);

        // 2. Act 2 Roster
        ActEnemyRoster act2 = GetOrCreateRoster(rosterFolder, "Act2Roster");
        act2.ActNumber = 2;
        act2.StandardEnemies = new[] {
            LoadEnemy("Act 2/Leech"),
            LoadEnemy("Act 2/Sniffer"),
            LoadEnemy("Act 2/BadSector")
        };
        act2.BossEnemy = LoadEnemy("Act 2/Inject");
        EditorUtility.SetDirty(act2);

        // 3. Act 3 Roster
        ActEnemyRoster act3 = GetOrCreateRoster(rosterFolder, "Act3Roster");
        act3.ActNumber = 3;
        act3.StandardEnemies = new[] {
            LoadEnemy("Act 3/Bloat"),
            LoadEnemy("Act 3/Overclock"),
            LoadEnemy("Act 3/Spooler"),
            LoadEnemy("Act 3/Surge")
        };
        act3.BossEnemy = LoadEnemy("Act 3/Kernel");
        EditorUtility.SetDirty(act3);

        // 4. Update MapConfig
        string[] configGuids = AssetDatabase.FindAssets("t:MapConfig");
        if (configGuids.Length > 0)
        {
            MapConfig config = AssetDatabase.LoadAssetAtPath<MapConfig>(AssetDatabase.GUIDToAssetPath(configGuids[0]));
            if (config != null)
            {
                config.act1Roster = act1;
                config.act2Roster = act2;
                config.act3Roster = act3;
                EditorUtility.SetDirty(config);
                Debug.Log($"[RosterSetup] Updated MapConfig '{config.name}' with rosters.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RosterSetup] Rosters created and linked successfully!");
    }

    private static ActEnemyRoster GetOrCreateRoster(string folder, string name)
    {
        string path = $"{folder}/{name}.asset";
        ActEnemyRoster roster = AssetDatabase.LoadAssetAtPath<ActEnemyRoster>(path);
        if (roster == null)
        {
            roster = ScriptableObject.CreateInstance<ActEnemyRoster>();
            AssetDatabase.CreateAsset(roster, path);
        }
        return roster;
    }

    private static EnemyData LoadEnemy(string relativePath)
    {
        string path = $"Assets/_Project/Data/Enemies/{relativePath}.asset";
        EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (data == null) Debug.LogError($"[RosterSetup] Missing EnemyData at {path}");
        return data;
    }
}
