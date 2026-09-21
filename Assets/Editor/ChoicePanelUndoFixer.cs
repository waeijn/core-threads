using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ChoicePanelUndoFixer : Editor
{
    [MenuItem("Tools/Undo Choice Panel Fix")]
    public static void UndoFix()
    {
        ChoicePanelUI[] panels = Resources.FindObjectsOfTypeAll<ChoicePanelUI>();
        foreach (var panel in panels)
        {
            if (panel.gameObject.scene.name == null) continue;
            
            Button btnA = (Button)typeof(ChoicePanelUI).GetField("optionAButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            
            if (btnA != null && btnA.transform.parent != null)
            {
                HorizontalLayoutGroup hlg = btnA.transform.parent.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    DestroyImmediate(hlg);
                    Debug.Log($"[Undo] Removed HorizontalLayoutGroup from {btnA.transform.parent.name}");
                    EditorUtility.SetDirty(btnA.transform.parent.gameObject);
                }
            }
        }
    }
}
