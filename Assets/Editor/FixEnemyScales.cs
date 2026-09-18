using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public class FixEnemyScales
{
    static FixEnemyScales()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        var scales = new Dictionary<string, float>
        {
            { "BadSector", 2.5f },
            { "Inject", 2.5f },
            { "Leech", 3.0f },
            { "Spooler", 3.0f },
            { "Surge", 3.5f },
            { "Bloat", 3.5f },
            { "Kernel", 4.5f },     // 64x64 native -> 288x288
            { "Sniffer", 1.5f }     // 218x222 native -> 327x333
        };

        foreach (var kvp in scales)
        {
            string path = $"Assets/_Project/Data/Enemies/{kvp.Key}.asset";
            EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (ed != null)
            {
                SerializedObject so = new SerializedObject(ed);
                so.FindProperty("<Scale>k__BackingField").vector3Value = new Vector3(kvp.Value, kvp.Value, 1f);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(ed);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Antigravity: Fixed Enemy Scales!");
    }
}
