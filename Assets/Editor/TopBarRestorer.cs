using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class TopBarRestorer : Editor
{
    [MenuItem("Tools/Restore TopBar from GameScene")]
    public static void RestoreTopBar()
    {
        // 1. Find MapScene Canvas
        Canvas mapCanvas = Object.FindAnyObjectByType<Canvas>();
        if (mapCanvas == null)
        {
            Debug.LogError("No Canvas in current scene!");
            return;
        }

        // 2. Delete my ugly TopBar
        Transform uglyTopBar = mapCanvas.transform.Find("TopBar");
        if (uglyTopBar != null)
        {
            DestroyImmediate(uglyTopBar.gameObject);
        }

        // 3. Open GameScene additively in the background to steal its TopBar
        Scene currentScene = EditorSceneManager.GetActiveScene();
        string gameScenePath = "Assets/_Project/Scenes/GameScene.unity";
        Scene gameScene = EditorSceneManager.OpenScene(gameScenePath, OpenSceneMode.Additive);

        // 4. Find the real TopBar in GameScene
        GameObject realTopBar = null;
        GameObject[] rootObjects = gameScene.GetRootGameObjects();
        foreach (var go in rootObjects)
        {
            if (go.name == "---UI---" || go.GetComponentInChildren<Canvas>() != null)
            {
                Canvas gsCanvas = go.GetComponentInChildren<Canvas>();
                if (gsCanvas != null)
                {
                    Transform tb = gsCanvas.transform.Find("TopBar");
                    if (tb != null)
                    {
                        realTopBar = tb.gameObject;
                        break;
                    }
                }
            }
        }

        if (realTopBar != null)
        {
            // 5. Duplicate the perfect TopBar and move it to MapScene
            GameObject restoredTopBar = Instantiate(realTopBar, mapCanvas.transform);
            restoredTopBar.name = "TopBar";
            
            // Ensure Map Btn is visible (acts as toggle to close map)
            Transform navPanel = restoredTopBar.transform.Find("Right_Navigation_Panel");
            if (navPanel != null)
            {
                Transform mapBtn = navPanel.Find("MapBtn");
                if (mapBtn != null) mapBtn.gameObject.SetActive(true);

                // Wire up the MenuButtonsUI to existing viewers in MapScene
                MenuButtonsUI menuUI = navPanel.GetComponent<MenuButtonsUI>();
                if (menuUI != null)
                {
                    Transform deckViewerPanel = mapCanvas.transform.Find("DeckViewerPanel");
                    if (deckViewerPanel != null) menuUI.GetType().GetField("deckViewer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(menuUI, deckViewerPanel.GetComponent<DeckViewerUI>());

                    Transform settingsPanel = mapCanvas.transform.Find("SettingsPanel");
                    if (settingsPanel != null) menuUI.GetType().GetField("settingsPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(menuUI, settingsPanel.gameObject);
                }
            }
            
            // Force Unity to realize the scene has changed so it asks you to save!
            EditorSceneManager.MarkSceneDirty(mapCanvas.gameObject.scene);

            Debug.Log("Successfully restored the beautiful TopBar from GameScene!");
        }
        else
        {
            Debug.LogError("Could not find TopBar in GameScene to copy!");
        }

        // 6. Close GameScene
        EditorSceneManager.CloseScene(gameScene, true);
        EditorSceneManager.SetActiveScene(currentScene);
    }
}
