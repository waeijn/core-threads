using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ChoicePanelFontFixer : Editor
{
    [MenuItem("Tools/Upgrade Choice Panel Fonts")]
    public static void UpgradeFonts()
    {
        ChoicePanelUI[] panels = Resources.FindObjectsOfTypeAll<ChoicePanelUI>();
        foreach (var panel in panels)
        {
            if (panel.gameObject.scene.name == null) continue;

            // 1. Upgrade Title Text
            TMP_Text title = (TMP_Text)typeof(ChoicePanelUI).GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (title != null)
            {
                title.enableAutoSizing = true;
                title.fontSizeMin = 30;
                title.fontSizeMax = 60;
                title.fontStyle = FontStyles.Bold;
                title.alignment = TextAlignmentOptions.Center;
                EditorUtility.SetDirty(title);
            }

            // 2. Upgrade Option A Text
            Button btnA = (Button)typeof(ChoicePanelUI).GetField("optionAButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (btnA != null)
            {
                TMP_Text txt = btnA.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.enableAutoSizing = true;
                    txt.fontSizeMin = 20;
                    txt.fontSizeMax = 45;
                    txt.fontStyle = FontStyles.Bold;
                    txt.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(txt);
                }
            }

            // 3. Upgrade Option B Text
            Button btnB = (Button)typeof(ChoicePanelUI).GetField("optionBButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (btnB != null)
            {
                TMP_Text txt = btnB.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.enableAutoSizing = true;
                    txt.fontSizeMin = 20;
                    txt.fontSizeMax = 45;
                    txt.fontStyle = FontStyles.Bold;
                    txt.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(txt);
                }
            }

            // 4. Upgrade Skip Text
            Button skipBtn = (Button)typeof(ChoicePanelUI).GetField("skipButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(panel);
            if (skipBtn != null)
            {
                TMP_Text txt = skipBtn.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.enableAutoSizing = true;
                    txt.fontSizeMin = 18;
                    txt.fontSizeMax = 32;
                    txt.fontStyle = FontStyles.Bold;
                    txt.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(txt);
                }
            }
            
            Debug.Log($"[FontUpgrade] Upgraded fonts for {panel.gameObject.name} in {panel.gameObject.scene.name}");
            EditorUtility.SetDirty(panel.gameObject);
        }
    }
}
