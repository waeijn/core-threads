using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class FindEnemies 
{
    static FindEnemies()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyData");
        foreach(var guid in guids) {
            Debug.Log("Antigravity Found Enemy: " + AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
