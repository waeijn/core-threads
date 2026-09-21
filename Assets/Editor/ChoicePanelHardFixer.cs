using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ChoicePanelHardFixer : Editor
{
    [MenuItem("Tools/Hard Fix Choice Panel UI")]
    public static void HardFixChoicePanel()
    {
        ChoicePanelUI[] panels = Resources.FindObjectsOfTypeAll<ChoicePanelUI>();
        foreach (var panel in panels)
        {
            if (panel.gameObject.scene.name == null) continue;

            // Fix Title Text
            TMP_Text title = (TMP_Text)typeof(ChoicePanelUI).GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (title != null)
            {
                RectTransform rt = title.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0, -100);
                rt.sizeDelta = new Vector2(1000, 100);
                EditorUtility.SetDirty(title);
            }

            // Fix Option A
            Button btnA = (Button)typeof(ChoicePanelUI).GetField("optionAButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (btnA != null)
            {
                RectTransform rt = btnA.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-275, 0);
                rt.sizeDelta = new Vector2(500, 150);
                
                // Fix Text inside Option A
                TMP_Text txt = btnA.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 130);
                }
                EditorUtility.SetDirty(btnA);
            }

            // Fix Option B
            Button btnB = (Button)typeof(ChoicePanelUI).GetField("optionBButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (btnB != null)
            {
                RectTransform rt = btnB.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(275, 0);
                rt.sizeDelta = new Vector2(500, 150);
                
                TMP_Text txt = btnB.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 130);
                }
                EditorUtility.SetDirty(btnB);
            }

            // Fix Skip Button
            Button skipBtn = (Button)typeof(ChoicePanelUI).GetField("skipButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (skipBtn != null)
            {
                RectTransform rt = skipBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0, 150);
                rt.sizeDelta = new Vector2(300, 80);
                
                TMP_Text txt = skipBtn.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 60);
                }
                EditorUtility.SetDirty(skipBtn);
            }
            
            Debug.Log($"[HardFix] Reset RectTransform sizes for {panel.gameObject.name} in {panel.gameObject.scene.name}");
            EditorUtility.SetDirty(panel.gameObject);
        }
    }
}
