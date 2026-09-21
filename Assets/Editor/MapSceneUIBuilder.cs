using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class MapSceneUIBuilder : Editor
{
    [MenuItem("Tools/Wire Existing Map Scene TopBar")]
    public static void WireExistingTopBar()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform topBar = canvas.transform.Find("TopBar");
        if (topBar == null)
        {
            Debug.LogError("Could not find TopBar in Canvas!");
            return;
        }

        Transform navPanel = topBar.Find("Right_Navigation_Panel");
        if (navPanel == null)
        {
            Debug.LogError("Could not find Right_Navigation_Panel under TopBar!");
            return;
        }

        Button deckBtn = navPanel.Find("DeckBtn")?.GetComponent<Button>();
        Button settingsBtn = navPanel.Find("SettingsBtn")?.GetComponent<Button>();

        if (deckBtn == null || settingsBtn == null)
        {
            Debug.LogError("Could not find DeckBtn or SettingsBtn!");
            return;
        }

        // Make sure we have DeckViewerPanel and SettingsPanel
        Transform deckViewerPanel = canvas.transform.Find("DeckViewerPanel");
        DeckViewerUI deckViewerUI = null;
        if (deckViewerPanel != null)
        {
            deckViewerUI = deckViewerPanel.GetComponent<DeckViewerUI>();
            if (deckViewerUI == null) deckViewerUI = Undo.AddComponent<DeckViewerUI>(deckViewerPanel.gameObject);
            
            // Wire DeckViewerUI
            SerializedObject dvSo = new SerializedObject(deckViewerUI);
            dvSo.FindProperty("panel").objectReferenceValue = deckViewerPanel.gameObject;
            dvSo.FindProperty("titleText").objectReferenceValue = deckViewerPanel.Find("TitleText")?.GetComponent<TMP_Text>();
            dvSo.FindProperty("cardCountText").objectReferenceValue = deckViewerPanel.Find("CardCountText")?.GetComponent<TMP_Text>();
            dvSo.FindProperty("cardGrid").objectReferenceValue = deckViewerPanel.Find("ScrollArea/Viewport/CardGrid");
            dvSo.FindProperty("backgroundOverlay").objectReferenceValue = deckViewerPanel.Find("BackgroundOverlay")?.GetComponent<Button>();
            
            string[] guids = AssetDatabase.FindAssets("CardThumbnailUI t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                dvSo.FindProperty("cardThumbnailPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CardThumbnailUI>(path);
            }
            dvSo.ApplyModifiedProperties();
        }

        Transform settingsPanel = canvas.transform.Find("SettingsPanel");
        if (settingsPanel != null)
        {
            // Just ensure it has SettingsPanelUI via AutoBuildSettings later if needed,
            // or the user already ran AutoBuildSettings on it.
        }

        MenuButtonsUI menuBtns = navPanel.GetComponent<MenuButtonsUI>();
        if (menuBtns == null) menuBtns = Undo.AddComponent<MenuButtonsUI>(navPanel.gameObject);

        SerializedObject mbSo = new SerializedObject(menuBtns);
        mbSo.FindProperty("deckButton").objectReferenceValue = deckBtn;
        mbSo.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
        if (deckViewerUI != null) mbSo.FindProperty("deckViewer").objectReferenceValue = deckViewerUI;
        if (settingsPanel != null) mbSo.FindProperty("settingsPanel").objectReferenceValue = settingsPanel.gameObject;
        mbSo.ApplyModifiedProperties();

        Debug.Log("Successfully wired the existing TopBar!");
    }
}
