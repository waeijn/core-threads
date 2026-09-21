using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class MapSceneCardScaler : Editor
{
    [MenuItem("Tools/Fix MapScene Card Scale")]
    public static void FixScale()
    {
        // Must be in MapScene
        if (SceneManager.GetActiveScene().name != "MapScene")
        {
            Debug.LogWarning("Please open MapScene first!");
            return;
        }

        GameObject container = GameObject.Find("CardListContainer");
        if (container != null)
        {
            GridLayoutGroup glg = container.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                // GameScene uses 200x280 on a 720p canvas.
                // MapScene uses a 1080p canvas, so we scale it by 1.5x (1080/720) to maintain visual size.
                glg.cellSize = new Vector2(300, 420);
                glg.spacing = new Vector2(30, 30); // scale spacing too (from 20,20)
                
                EditorUtility.SetDirty(glg);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("CardListContainer GridLayoutGroup scaled to 300x420 to match GameScene proportions!");
            }
            else
            {
                Debug.LogError("No GridLayoutGroup found on CardListContainer.");
            }
        }
        else
        {
            Debug.LogError("CardListContainer not found in MapScene.");
        }
    }
}
