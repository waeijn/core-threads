using UnityEngine;
using UnityEditor;

public class EnemyHPDebugTool : Editor
{
    [MenuItem("Tools/Debug: Set All Enemy HP to 1")]
    public static void SetHPToOne()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (data != null)
            {
                SerializedObject so = new SerializedObject(data);
                SerializedProperty hpProp = so.FindProperty("<Health>k__BackingField");
                if (hpProp != null)
                {
                    hpProp.intValue = 1;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(data);
                    count++;
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Debug] Set {count} enemies to 1 HP for testing.");
    }

    [MenuItem("Tools/Debug: Restore Placeholder Enemy HP")]
    public static void RestoreHP()
    {
        // We can just wipe their HP to 0 and let EnemyAnimationSetupUtility restore them!
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (data != null)
            {
                SerializedObject so = new SerializedObject(data);
                SerializedProperty hpProp = so.FindProperty("<Health>k__BackingField");
                if (hpProp != null)
                {
                    hpProp.intValue = 0; // Set to 0 so SetupEnemyAnimations replaces it
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(data);
                }
            }
        }
        AssetDatabase.SaveAssets();
        
        // Call the setup utility to re-apply the dictionary stats
        EnemyAnimationSetupUtility.SetupAllEnemyAnimations();
        Debug.Log("[Debug] Restored placeholder HP to all enemies.");
    }
}
