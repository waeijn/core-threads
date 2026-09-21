using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AutoBuildSettings : Editor
{
    [MenuItem("Tools/Auto-Build Settings UI")]
    public static void BuildUI()
    {
        GameObject settingsPanel = null;
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave) continue;
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(obj.transform.root.gameObject)) continue;
#endif
            if (obj.name.Replace(" ", "").ToLower().Contains("settingspanel"))
            {
                settingsPanel = obj;
                break;
            }
        }
        
        if (settingsPanel == null)
        {
            Debug.LogError("[Auto-Build] Could not find a SettingsPanel.");
            return;
        }

        Undo.RecordObject(settingsPanel, "Auto Build Settings UI");

        // Style the Panel itself
        Image panelImage = settingsPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark elegant background
        }
        
        RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 450); // Force taller for 3 sliders
        }

        SettingsPanelUI settingsScript = settingsPanel.GetComponent<SettingsPanelUI>();
        if (settingsScript == null) settingsScript = Undo.AddComponent<SettingsPanelUI>(settingsPanel);

        // CLEAR OLD STUFF: Destroy any existing text/buttons so they don't overlap!
        for (int i = settingsPanel.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(settingsPanel.transform.GetChild(i).gameObject);
        }

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(settingsPanel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SETTINGS";
        titleText.fontSize = 32;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 150);
        titleRect.sizeDelta = new Vector2(300, 50);

        // Master Slider (Parent)
        Slider masterSlider = CreateLabeledSlider(settingsPanel.transform, "Master Volume", 110, 80, 0, 250, 20);
        
        // Child Sliders (Indented and slightly smaller)
        Slider musicSlider = CreateLabeledSlider(settingsPanel.transform, "Music Volume", 30, 0, 25, 200, 16);
        Slider sfxSlider = CreateLabeledSlider(settingsPanel.transform, "SFX Volume", -50, -80, 25, 200, 16);

        // Quit Button
        GameObject buttonObj = DefaultControls.CreateButton(GetStandardResources());
        buttonObj.name = "QuitGameButton";
        buttonObj.transform.SetParent(settingsPanel.transform, false);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(0, -150);
        buttonRect.sizeDelta = new Vector2(200, 50);
        
        // Style button
        buttonObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Object.DestroyImmediate(buttonObj.GetComponentInChildren<Text>().gameObject); // Remove ugly default text
        
        GameObject btnTextObj = new GameObject("Text (TMP)");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "QUIT GAME";
        btnText.color = Color.white;
        btnText.fontSize = 22;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Link script
        SerializedObject so = new SerializedObject(settingsScript);
        so.FindProperty("masterSlider").objectReferenceValue = masterSlider;
        so.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        so.FindProperty("quitButton").objectReferenceValue = buttonObj.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Wire into MenuButtonsUI if it exists in this scene
        MenuButtonsUI menuButtons = Object.FindAnyObjectByType<MenuButtonsUI>();
        if (menuButtons != null)
        {
            SerializedObject mso = new SerializedObject(menuButtons);
            mso.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            mso.ApplyModifiedProperties();
            Debug.Log("[Auto-Build] Found MenuButtonsUI and automatically linked the settings panel to it!");
        }

        Debug.Log("[Auto-Build] High-Quality UI Generated!");
    }

    private static DefaultControls.Resources GetStandardResources()
    {
        DefaultControls.Resources resources = new DefaultControls.Resources();
        resources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        resources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        resources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        return resources;
    }

    private static Slider CreateLabeledSlider(Transform parent, string labelText, float labelY, float sliderY, float xOffset, float width, int fontSize)
    {
        // Slider Label
        GameObject volumeLabelObj = new GameObject(labelText.Replace(" ", "") + "Label");
        volumeLabelObj.transform.SetParent(parent, false);
        TextMeshProUGUI volumeLabelText = volumeLabelObj.AddComponent<TextMeshProUGUI>();
        volumeLabelText.text = labelText;
        volumeLabelText.fontSize = fontSize;
        volumeLabelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        volumeLabelText.alignment = TextAlignmentOptions.Center;
        RectTransform volumeLabelRect = volumeLabelObj.GetComponent<RectTransform>();
        volumeLabelRect.anchoredPosition = new Vector2(xOffset, labelY);
        volumeLabelRect.sizeDelta = new Vector2(width, 30);

        // Slider
        GameObject sliderObj = DefaultControls.CreateSlider(GetStandardResources());
        sliderObj.name = labelText.Replace(" ", "") + "Slider";
        sliderObj.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(xOffset, sliderY);
        sliderRect.sizeDelta = new Vector2(width, 20);
        
        // Style slider
        sliderObj.transform.Find("Background").GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        sliderObj.transform.Find("Fill Area/Fill").GetComponent<Image>().color = new Color(0.2f, 0.7f, 1f, 1f);
        
        return sliderObj.GetComponent<Slider>();
    }
}
