using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ChoicePanelFinalFixer : Editor
{
    [MenuItem("Tools/Final Fix Choice Panel")]
    public static void FinalFixChoicePanel()
    {
        ChoicePanelUI[] panels = Resources.FindObjectsOfTypeAll<ChoicePanelUI>();
        foreach (var panel in panels)
        {
            if (panel.gameObject.scene.name == null) continue;

            // Fix Option A Text Rect
            Button btnA = (Button)typeof(ChoicePanelUI).GetField("optionAButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (btnA != null)
            {
                TMP_Text txt = btnA.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    RectTransform rt = txt.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero; // This is the secret sauce for STRETCH!
                    EditorUtility.SetDirty(txt);
                }
            }

            // Fix Option B Text Rect
            Button btnB = (Button)typeof(ChoicePanelUI).GetField("optionBButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (btnB != null)
            {
                TMP_Text txt = btnB.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    RectTransform rt = txt.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero;
                    EditorUtility.SetDirty(txt);
                }
            }

            // Fix Skip Button Text Rect
            Button skipBtn = (Button)typeof(ChoicePanelUI).GetField("skipButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (skipBtn != null)
            {
                TMP_Text txt = skipBtn.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    RectTransform rt = txt.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero;
                    EditorUtility.SetDirty(txt);
                }
            }

            // Let's also make sure Title Text aligns Center
            TMP_Text title = (TMP_Text)typeof(ChoicePanelUI).GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (title != null)
            {
                title.alignment = TextAlignmentOptions.Center;
                EditorUtility.SetDirty(title);
            }
            
            Debug.Log($"[FinalFix] Reset Text RectTransforms to Stretch inside buttons for {panel.gameObject.name}");
            EditorUtility.SetDirty(panel.gameObject);
        }
    }
}
