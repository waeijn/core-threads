using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ResponsiveUIFixer : EditorWindow
{
    [MenuItem("Tools/Make UI Responsive")]
    public static void FixResponsiveUI()
    {
        int modifiedCount = 0;
        
        // Find all CanvasScalers in the active scene
        CanvasScaler[] scalers = Resources.FindObjectsOfTypeAll<CanvasScaler>();
        
        foreach (CanvasScaler scaler in scalers)
        {
            // Skip prefabs that are not in the scene
            if (scaler.gameObject.scene.name == null) continue;

            Undo.RecordObject(scaler, "Make UI Responsive");
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); // Matched to your Editor Game View!
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            
            EditorUtility.SetDirty(scaler);
            modifiedCount++;
        }

        if (modifiedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"<color=green>Successfully updated {modifiedCount} Canvas(es) to be responsive using 1280x720!</color>");
        }
        else
        {
            Debug.Log("No Canvases found in the current scene to update.");
        }
    }
}
