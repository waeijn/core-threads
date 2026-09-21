using UnityEngine;
using UnityEditor;

public class ChoicePanelReverter : Editor
{
    [MenuItem("Tools/Revert Choice Panel Prefab")]
    public static void RevertChoicePanel()
    {
        ChoicePanelUI[] panels = Resources.FindObjectsOfTypeAll<ChoicePanelUI>();
        int count = 0;
        foreach (var panel in panels)
        {
            if (panel.gameObject.scene.name == null) continue;

            if (PrefabUtility.IsPartOfPrefabInstance(panel.gameObject))
            {
                PrefabUtility.RevertPrefabInstance(panel.gameObject, InteractionMode.AutomatedAction);
                Debug.Log($"[Revert] Reverted prefab instance for {panel.gameObject.name} in {panel.gameObject.scene.name}");
                count++;
            }
            else
            {
                Debug.LogWarning($"[Revert] {panel.gameObject.name} in {panel.gameObject.scene.name} is NOT a prefab instance!");
            }
        }
        
        Debug.Log($"[Revert] Processed {count} prefab instances.");
    }
}
