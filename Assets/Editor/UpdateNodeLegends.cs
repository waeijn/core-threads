using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class UpdateNodeLegends : Editor
{
    [MenuItem("Tools/Update Node Legends")]
    public static void UpdateText()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "MapScene")
        {
            Debug.LogError("Please open MapScene first!");
            return;
        }

        TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (var txt in texts)
        {
            if (txt.text.Contains("[TERMINAL]"))
            {
                txt.text = "<b>[TERMINAL]  Rest</b>\nA secure shell prompt.\nAllows the player to Garbage Collect (purge a card) or Cooling (restore System Health).";
                EditorUtility.SetDirty(txt);
                count++;
            }
            else if (txt.text.Contains("[BUG]"))
            {
                txt.text = "<b>[BUG]  Basic Enemy</b>\nA localized software defect.\nCombat here is straightforward but necessary to expand your deck (Draft New Card).";
                EditorUtility.SetDirty(txt);
                count++;
            }
            else if (txt.text.Contains("[DATABASE]"))
            {
                txt.text = "<b>[DATABASE]  Treasure</b>\nAn unsecured data store.\nAllows the player to perform Targeted Duplication (copy a card) or Draft a Reward Card.";
                EditorUtility.SetDirty(txt);
                count++;
            }
        }
        
        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"Updated {count} Node Legend text components!");
        }
        else
        {
            Debug.LogWarning("Could not find the Node Legend text containing 'upgrade cards'.");
        }
    }
}
