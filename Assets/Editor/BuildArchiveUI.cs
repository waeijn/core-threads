using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BuildArchiveUI : Editor
{
    [MenuItem("Tools/Build Archive UI")]
    public static void Build()
    {
        // Open MainMenu scene
        Scene scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity");
        
        // Find Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[BuildArchiveUI] No Canvas found in MainMenu scene!");
            return;
        }

        // Find MainMenuController
        MainMenuController controller = Object.FindAnyObjectByType<MainMenuController>();
        if (controller == null)
        {
            Debug.LogError("[BuildArchiveUI] No MainMenuController found!");
            return;
        }

        // Find MainMenuButtons layout group
        Transform buttonsParent = null;
        foreach (Transform child in canvas.transform)
        {
            if (child.name == "MainMenuButtons")
            {
                buttonsParent = child;
                break;
            }
        }

        if (buttonsParent == null)
        {
            Debug.LogError("[BuildArchiveUI] MainMenuButtons not found in Canvas!");
            return;
        }

        // Find existing font from PlayButton
        TMP_FontAsset font = null;
        var existingTexts = buttonsParent.GetComponentsInChildren<TMP_Text>();
        if (existingTexts.Length > 0) font = existingTexts[0].font;

        // ═══════════════════════════════════════════════════════════════════
        // 1. CREATE ARCHIVE BUTTON (between Play and Settings — index 1)
        // ═══════════════════════════════════════════════════════════════════
        
        // Check if already exists
        Transform existingArchiveBtn = buttonsParent.Find("ArchiveButton");
        if (existingArchiveBtn != null)
        {
            DestroyImmediate(existingArchiveBtn.gameObject);
        }

        GameObject archiveBtnObj = new GameObject("ArchiveButton");
        archiveBtnObj.transform.SetParent(buttonsParent, false);
        archiveBtnObj.transform.SetSiblingIndex(1); // After PlayButton (index 0)

        Image archiveBtnImg = archiveBtnObj.AddComponent<Image>();
        archiveBtnImg.color = new Color(0, 0, 0, 0.7f);

        Button archiveBtn = archiveBtnObj.AddComponent<Button>();
        var colors = archiveBtn.colors;
        colors.highlightedColor = new Color(0.2f, 0.8f, 1f, 0.3f);
        archiveBtn.colors = colors;

        LayoutElement archiveBtnLe = archiveBtnObj.AddComponent<LayoutElement>();
        archiveBtnLe.minHeight = 80;

        // Button Text
        GameObject archiveBtnTextObj = new GameObject("Text");
        archiveBtnTextObj.transform.SetParent(archiveBtnObj.transform, false);
        TextMeshProUGUI archiveBtnText = archiveBtnTextObj.AddComponent<TextMeshProUGUI>();
        archiveBtnText.text = "ARCHIVE";
        archiveBtnText.fontSize = 40;
        archiveBtnText.color = Color.white;
        archiveBtnText.fontStyle = FontStyles.Bold; // Make it match PLAY and SETTINGS
        archiveBtnText.alignment = TextAlignmentOptions.Left; // Match other buttons
        if (font != null) archiveBtnText.font = font;
        RectTransform archiveBtnTextRT = archiveBtnTextObj.GetComponent<RectTransform>();
        archiveBtnTextRT.anchorMin = Vector2.zero;
        archiveBtnTextRT.anchorMax = Vector2.one;
        archiveBtnTextRT.sizeDelta = Vector2.zero;

        // ═══════════════════════════════════════════════════════════════════
        // 2. CREATE ARCHIVE PANEL (full screen overlay)
        // ═══════════════════════════════════════════════════════════════════
        
        // Remove existing if present
        Transform existingArchive = canvas.transform.Find("ArchivePanel");
        if (existingArchive != null) DestroyImmediate(existingArchive.gameObject);

        GameObject archiveRoot = new GameObject("ArchivePanel");
        archiveRoot.transform.SetParent(canvas.transform, false);
        
        // CRITICAL: Make archiveRoot fill the entire canvas
        RectTransform archiveRootRT = archiveRoot.AddComponent<RectTransform>();
        archiveRootRT.anchorMin = Vector2.zero;
        archiveRootRT.anchorMax = Vector2.one;
        archiveRootRT.sizeDelta = Vector2.zero;
        archiveRootRT.offsetMin = Vector2.zero;
        archiveRootRT.offsetMax = Vector2.zero;
        
        ArchiveUI archiveUI = archiveRoot.AddComponent<ArchiveUI>();

        // Main Panel (dark background — full screen)
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(archiveRoot.transform, false);
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.97f);
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Background Overlay (click to close)
        Button bgOverlay = panelObj.AddComponent<Button>();
        bgOverlay.transition = Selectable.Transition.None;

        // ── Title Bar ──────────────────────────────────────────────────────
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(panelObj.transform, false);
        RectTransform titleBarRT = titleBar.AddComponent<RectTransform>();
        titleBarRT.anchorMin = new Vector2(0, 1);
        titleBarRT.anchorMax = new Vector2(1, 1);
        titleBarRT.pivot = new Vector2(0.5f, 1);
        titleBarRT.anchoredPosition = new Vector2(0, -10);
        titleBarRT.sizeDelta = new Vector2(-120, 70); // Leave room for X button

        HorizontalLayoutGroup titleHlg = titleBar.AddComponent<HorizontalLayoutGroup>();
        titleHlg.spacing = 50;
        titleHlg.childAlignment = TextAnchor.MiddleCenter;
        titleHlg.childControlWidth = false;
        titleHlg.childControlHeight = false;

        // SCRIPTS tab button
        GameObject scriptsTabObj = CreateTabButton(titleBar.transform, "ScriptsTab", "[ SCRIPTS ]", font);
        Button scriptsTabBtn = scriptsTabObj.GetComponent<Button>();

        // THREATS tab button
        GameObject threatsTabObj = CreateTabButton(titleBar.transform, "ThreatsTab", "[ THREATS ]", font);
        Button threatsTabBtn = threatsTabObj.GetComponent<Button>();

        // Close (X) button
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panelObj.transform, false);
        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.8f, 0.1f, 0.1f, 0.8f);
        Button closeBtn = closeObj.AddComponent<Button>();
        RectTransform closeRT = closeObj.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 1);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.pivot = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-10, -10);
        closeRT.sizeDelta = new Vector2(50, 50);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        TextMeshProUGUI closeTmp = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "X";
        closeTmp.fontSize = 30;
        closeTmp.color = Color.white;
        closeTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) closeTmp.font = font;
        RectTransform closeTextRT = closeTextObj.GetComponent<RectTransform>();
        closeTextRT.anchorMin = Vector2.zero;
        closeTextRT.anchorMax = Vector2.one;
        closeTextRT.sizeDelta = Vector2.zero;

        // ── Content Area ───────────────────────────────────────────────────

        // SCRIPTS Content (ScrollView)
        GameObject scriptsContent = CreateScrollContent(panelObj.transform, "ScriptsContent");

        // THREATS Content (ScrollView)
        GameObject threatsContent = CreateScrollContent(panelObj.transform, "ThreatsContent");

        // ── Card Detail Popup ──────────────────────────────────────────────
        GameObject cardDetailPanel = CreateCardDetailPopup(panelObj.transform, font);

        // ── Enemy Detail Popup ─────────────────────────────────────────────
        GameObject enemyDetailPanel = CreateEnemyDetailPopup(panelObj.transform, font);

        // ═══════════════════════════════════════════════════════════════════
        // 3. WIRE ALL SERIALIZED REFERENCES
        // ═══════════════════════════════════════════════════════════════════

        SerializedObject archiveSO = new SerializedObject(archiveUI);
        archiveSO.FindProperty("panel").objectReferenceValue = panelObj;
        archiveSO.FindProperty("backgroundOverlay").objectReferenceValue = bgOverlay;
        archiveSO.FindProperty("mainCloseButton").objectReferenceValue = closeBtn; // Wire mainCloseButton
        archiveSO.FindProperty("scriptsTabButton").objectReferenceValue = scriptsTabBtn;
        archiveSO.FindProperty("threatsTabButton").objectReferenceValue = threatsTabBtn;
        archiveSO.FindProperty("scriptsContent").objectReferenceValue = scriptsContent.transform.GetChild(0).GetChild(0).gameObject; // Viewport > Content
        archiveSO.FindProperty("threatsContent").objectReferenceValue = threatsContent.transform.GetChild(0).GetChild(0).gameObject;

        // Card Detail
        archiveSO.FindProperty("cardDetailPanel").objectReferenceValue = cardDetailPanel;
        archiveSO.FindProperty("cardDetailImage").objectReferenceValue = cardDetailPanel.transform.Find("CardImage").GetComponent<Image>();
        archiveSO.FindProperty("cardDetailTitle").objectReferenceValue = cardDetailPanel.transform.Find("CardTitle").GetComponent<TMP_Text>();
        archiveSO.FindProperty("cardDetailDescription").objectReferenceValue = cardDetailPanel.transform.Find("CardDescription").GetComponent<TMP_Text>();
        archiveSO.FindProperty("cardDetailStats").objectReferenceValue = cardDetailPanel.transform.Find("CardStats").GetComponent<TMP_Text>();
        archiveSO.FindProperty("cardDetailCloseButton").objectReferenceValue = cardDetailPanel.transform.Find("CloseButton").GetComponent<Button>();

        // Enemy Detail
        archiveSO.FindProperty("enemyDetailPanel").objectReferenceValue = enemyDetailPanel;
        archiveSO.FindProperty("enemyDetailImage").objectReferenceValue = enemyDetailPanel.transform.Find("EnemyImage").GetComponent<Image>();
        archiveSO.FindProperty("enemyDetailName").objectReferenceValue = enemyDetailPanel.transform.Find("EnemyName").GetComponent<TMP_Text>();
        archiveSO.FindProperty("enemyDetailStats").objectReferenceValue = enemyDetailPanel.transform.Find("EnemyStats").GetComponent<TMP_Text>();
        archiveSO.FindProperty("enemyDetailMoves").objectReferenceValue = enemyDetailPanel.transform.Find("EnemyMoves").GetComponent<TMP_Text>();
        archiveSO.FindProperty("enemyDetailCloseButton").objectReferenceValue = enemyDetailPanel.transform.Find("CloseButton").GetComponent<Button>();

        // Rosters
        ActEnemyRoster roster1 = AssetDatabase.LoadAssetAtPath<ActEnemyRoster>("Assets/_Project/Data/Rosters/Act1Roster.asset");
        ActEnemyRoster roster2 = AssetDatabase.LoadAssetAtPath<ActEnemyRoster>("Assets/_Project/Data/Rosters/Act2Roster.asset");
        ActEnemyRoster roster3 = AssetDatabase.LoadAssetAtPath<ActEnemyRoster>("Assets/_Project/Data/Rosters/Act3Roster.asset");
        archiveSO.FindProperty("act1Roster").objectReferenceValue = roster1;
        archiveSO.FindProperty("act2Roster").objectReferenceValue = roster2;
        archiveSO.FindProperty("act3Roster").objectReferenceValue = roster3;

        archiveSO.FindProperty("mainFont").objectReferenceValue = font;

        archiveSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(archiveUI);

        // Wire MainMenuController
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("archiveButton").objectReferenceValue = archiveBtn;
        controllerSO.FindProperty("archivePanel").objectReferenceValue = archiveRoot;
        controllerSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[BuildArchiveUI] Archive UI built successfully!");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════

    private static GameObject CreateTabButton(Transform parent, string name, string label, TMP_FontAsset font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0);

        Button btn = obj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.2f, 0.8f, 1f, 0.2f);
        btn.colors = colors;

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minWidth = 300;
        le.preferredWidth = 300;
        le.minHeight = 50;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        if (font != null) tmp.font = font;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        return obj;
    }

    private static GameObject CreateScrollContent(Transform parent, string name)
    {
        GameObject scrollObj = new GameObject(name);
        scrollObj.transform.SetParent(parent, false);
        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(20, 20);
        scrollRT.offsetMax = new Vector2(-20, -90); // Leave room for title bar + tabs

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30;

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0); // Transparent

        Mask mask = scrollObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRT = viewport.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = Vector2.zero;

        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0);
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRT;
        scroll.content = contentRT;

        return scrollObj;
    }

    private static GameObject CreateCardDetailPopup(Transform parent, TMP_FontAsset font)
    {
        GameObject popup = new GameObject("CardDetailPanel");
        popup.transform.SetParent(parent, false);
        popup.SetActive(false);

        Image bg = popup.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0.1f);
        rt.anchorMax = new Vector2(0.8f, 0.9f);
        rt.sizeDelta = Vector2.zero;

        // Card Image (left side)
        GameObject cardImgObj = new GameObject("CardImage");
        cardImgObj.transform.SetParent(popup.transform, false);
        Image cardImg = cardImgObj.AddComponent<Image>();
        cardImg.preserveAspect = true;
        RectTransform cardImgRT = cardImgObj.GetComponent<RectTransform>();
        cardImgRT.anchorMin = new Vector2(0.05f, 0.2f);
        cardImgRT.anchorMax = new Vector2(0.4f, 0.9f);
        cardImgRT.sizeDelta = Vector2.zero;

        // Card Title
        GameObject titleObj = new GameObject("CardTitle");
        titleObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.fontSize = 36;
        titleTmp.color = new Color(0.2f, 0.8f, 1f);
        titleTmp.alignment = TextAlignmentOptions.TopLeft;
        if (font != null) titleTmp.font = font;
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.45f, 0.75f);
        titleRT.anchorMax = new Vector2(0.95f, 0.9f);
        titleRT.sizeDelta = Vector2.zero;

        // Card Stats
        GameObject statsObj = new GameObject("CardStats");
        statsObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI statsTmp = statsObj.AddComponent<TextMeshProUGUI>();
        statsTmp.fontSize = 22;
        statsTmp.color = Color.white;
        statsTmp.alignment = TextAlignmentOptions.TopLeft;
        if (font != null) statsTmp.font = font;
        RectTransform statsRT = statsObj.GetComponent<RectTransform>();
        statsRT.anchorMin = new Vector2(0.45f, 0.6f);
        statsRT.anchorMax = new Vector2(0.95f, 0.75f);
        statsRT.sizeDelta = Vector2.zero;

        // Card Description
        GameObject descObj = new GameObject("CardDescription");
        descObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
        descTmp.fontSize = 28;
        descTmp.color = new Color(0.85f, 0.85f, 0.85f);
        descTmp.alignment = TextAlignmentOptions.TopLeft;
        descTmp.enableWordWrapping = true;
        if (font != null) descTmp.font = font;
        RectTransform descRT = descObj.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0.45f, 0.2f);
        descRT.anchorMax = new Vector2(0.95f, 0.6f);
        descRT.sizeDelta = Vector2.zero;

        // Close Button
        CreatePopupCloseButton(popup.transform, font);

        return popup;
    }

    private static GameObject CreateEnemyDetailPopup(Transform parent, TMP_FontAsset font)
    {
        GameObject popup = new GameObject("EnemyDetailPanel");
        popup.transform.SetParent(parent, false);
        popup.SetActive(false);

        Image bg = popup.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.15f, 0.05f);
        rt.anchorMax = new Vector2(0.85f, 0.95f);
        rt.sizeDelta = Vector2.zero;

        // Enemy Image (left side)
        GameObject imgObj = new GameObject("EnemyImage");
        imgObj.transform.SetParent(popup.transform, false);
        Image img = imgObj.AddComponent<Image>();
        img.preserveAspect = true;
        RectTransform imgRT = imgObj.GetComponent<RectTransform>();
        imgRT.anchorMin = new Vector2(0.05f, 0.4f);
        imgRT.anchorMax = new Vector2(0.35f, 0.9f);
        imgRT.sizeDelta = Vector2.zero;

        // Enemy Name
        GameObject nameObj = new GameObject("EnemyName");
        nameObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.fontSize = 36;
        nameTmp.color = new Color(1f, 0.3f, 0.3f);
        nameTmp.alignment = TextAlignmentOptions.TopLeft;
        if (font != null) nameTmp.font = font;
        RectTransform nameRT = nameObj.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.4f, 0.82f);
        nameRT.anchorMax = new Vector2(0.95f, 0.92f);
        nameRT.sizeDelta = Vector2.zero;

        // Enemy Stats
        GameObject statsObj = new GameObject("EnemyStats");
        statsObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI statsTmp = statsObj.AddComponent<TextMeshProUGUI>();
        statsTmp.fontSize = 24;
        statsTmp.color = Color.white;
        statsTmp.alignment = TextAlignmentOptions.TopLeft;
        if (font != null) statsTmp.font = font;
        RectTransform statsRT = statsObj.GetComponent<RectTransform>();
        statsRT.anchorMin = new Vector2(0.4f, 0.6f);
        statsRT.anchorMax = new Vector2(0.95f, 0.82f);
        statsRT.sizeDelta = Vector2.zero;

        // Enemy Moves
        GameObject movesObj = new GameObject("EnemyMoves");
        movesObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI movesTmp = movesObj.AddComponent<TextMeshProUGUI>();
        movesTmp.fontSize = 20;
        movesTmp.color = new Color(0.8f, 0.8f, 0.8f);
        movesTmp.alignment = TextAlignmentOptions.TopLeft;
        movesTmp.enableWordWrapping = true;
        if (font != null) movesTmp.font = font;
        RectTransform movesRT = movesObj.GetComponent<RectTransform>();
        movesRT.anchorMin = new Vector2(0.05f, 0.05f);
        movesRT.anchorMax = new Vector2(0.95f, 0.55f);
        movesRT.sizeDelta = Vector2.zero;

        // Close Button
        CreatePopupCloseButton(popup.transform, font);

        return popup;
    }

    private static void CreatePopupCloseButton(Transform parent, TMP_FontAsset font)
    {
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(parent, false);
        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.8f, 0.1f, 0.1f, 0.8f);
        closeObj.AddComponent<Button>();
        RectTransform closeRT = closeObj.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 1);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.pivot = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-10, -10);
        closeRT.sizeDelta = new Vector2(40, 40);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(closeObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "X";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
    }
}
