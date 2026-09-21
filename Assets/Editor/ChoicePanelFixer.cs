using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ChoicePanelFixer : Editor
{
    [MenuItem("Tools/Fix Choice Panel UI")]
    public static void FixChoicePanel()
    {
        ChoicePanelUI[] panels = Resources.FindObjectsOfTypeAll<ChoicePanelUI>();
        int count = 0;
        foreach (var panel in panels)
        {
            if (panel.gameObject.scene.name == null) continue; // Skip prefabs if any
            
            // Find the container holding the two options
            // Usually optionA and optionB share a parent.
            Button btnA = (Button)typeof(ChoicePanelUI).GetField("optionAButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            Button btnB = (Button)typeof(ChoicePanelUI).GetField("optionBButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            
            if (btnA != null && btnB != null && btnA.transform.parent == btnB.transform.parent)
            {
                Transform container = btnA.transform.parent;
                HorizontalLayoutGroup hlg = container.GetComponent<HorizontalLayoutGroup>();
                if (hlg == null)
                {
                    hlg = container.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childAlignment = TextAnchor.MiddleCenter;
                    hlg.childControlHeight = false;
                    hlg.childControlWidth = false;
                    hlg.childForceExpandHeight = false;
                    hlg.childForceExpandWidth = false;
                    hlg.spacing = 50;
                    Debug.Log($"[FixChoicePanel] Added HorizontalLayoutGroup to {container.name} in {panel.gameObject.scene.name}");
                    EditorUtility.SetDirty(container.gameObject);
                    count++;
                }
            }
        }
        
        Debug.Log($"[FixChoicePanel] Processed {count} choice panels.");
    }
}
