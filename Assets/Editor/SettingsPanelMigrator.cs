using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SettingsPanelMigrator : Editor
{
    [MenuItem("Tools/Migrate Settings Panel to MapScene")]
    public static void MigrateSettings()
    {
        // 1. Open GameScene to grab the SettingsPanel
        Scene mapScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MapScene.unity", OpenSceneMode.Single);
        Scene gameScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/GameScene.unity", OpenSceneMode.Additive);
        
        GameObject settingsPanel = null;
        foreach (var root in gameScene.GetRootGameObjects())
        {
            var found = root.GetComponentInChildren<SettingsPanelUI>(true);
            if (found != null)
            {
                settingsPanel = found.gameObject;
                break;
            }
        }

        if (settingsPanel == null)
        {
            Debug.LogError("Could not find SettingsPanel in GameScene!");
            EditorSceneManager.CloseScene(gameScene, true);
            return;
        }

        // 2. Clone it and move it to MapScene
        GameObject clone = Instantiate(settingsPanel);
        clone.name = "SettingsPanel";
        SceneManager.MoveGameObjectToScene(clone, mapScene);

        // 3. Find Canvas in MapScene
        Canvas mapCanvas = null;
        foreach (var root in mapScene.GetRootGameObjects())
        {
            mapCanvas = root.GetComponentInChildren<Canvas>(true);
            if (mapCanvas != null) break;
        }

        if (mapCanvas != null)
        {
            clone.transform.SetParent(mapCanvas.transform, false);
            clone.transform.SetAsLastSibling(); // Ensure it's on top
            clone.SetActive(false); // Default to off
            Debug.Log("Successfully migrated SettingsPanel to MapScene Canvas!");
            EditorSceneManager.MarkSceneDirty(mapScene);
            EditorSceneManager.SaveScene(mapScene);
        }
        else
        {
            Debug.LogError("Could not find Canvas in MapScene!");
            DestroyImmediate(clone);
        }

        // Close GameScene (additive)
        EditorSceneManager.CloseScene(gameScene, true);
    }
}
