using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button archiveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private GameObject archivePanel;

    [Header("Mode Selection Panel")]
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private Button modeBackdropButton;
    [SerializeField] private Button modeCloseButton;
    [SerializeField] private Button continueRunButton;
    [SerializeField] private Button normalModeButton;
    [SerializeField] private Button hardcoreModeButton;

    // Cached references for dynamic status updates
    private Image _continueRunBg;
    private Outline _continueRunOutline;
    private TMP_Text _continueTitleText;
    private TMP_Text _continueDescText;

    private Image _hardcoreModeBg;
    private Outline _hardcoreModeOutline;
    private TMP_Text _hardcoreTitleText;
    private TMP_Text _hardcoreDescText;
    private TMP_FontAsset _menuFont;

    private void Awake()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (archivePanel != null) archivePanel.SetActive(false);
        if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
            var tmp = playButton.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) _menuFont = tmp.font;
        }
        if (archiveButton != null)
        {
            archiveButton.onClick.RemoveAllListeners();
            archiveButton.onClick.AddListener(OnArchiveClicked);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.RemoveAllListeners();
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        }

        AddHoverSound(playButton);
        AddHoverSound(archiveButton);
        AddHoverSound(settingsButton);
        AddHoverSound(quitButton);
        AddHoverSound(settingsBackButton);

        BindModeSelectPanelListeners();
    }

    private void BindModeSelectPanelListeners()
    {
        if (modeSelectPanel == null) return;

        if (modeBackdropButton != null)
        {
            modeBackdropButton.onClick.RemoveAllListeners();
            modeBackdropButton.onClick.AddListener(CloseModeSelectionPopup);
        }

        if (modeCloseButton != null)
        {
            modeCloseButton.onClick.RemoveAllListeners();
            modeCloseButton.onClick.AddListener(CloseModeSelectionPopup);
            AddHoverSound(modeCloseButton);
        }

        if (continueRunButton != null)
        {
            continueRunButton.onClick.RemoveAllListeners();
            continueRunButton.onClick.AddListener(ContinueExistingRun);
            AddHoverSound(continueRunButton);
            _continueRunBg = continueRunButton.GetComponent<Image>();
            _continueRunOutline = continueRunButton.GetComponent<Outline>();
            var titleTr = continueRunButton.transform.Find("Title");
            var descTr = continueRunButton.transform.Find("Description");
            if (titleTr != null) _continueTitleText = titleTr.GetComponent<TMP_Text>();
            if (descTr != null) _continueDescText = descTr.GetComponent<TMP_Text>();
        }

        if (normalModeButton != null)
        {
            normalModeButton.onClick.RemoveAllListeners();
            normalModeButton.onClick.AddListener(OnNormalModeClicked);
            AddHoverSound(normalModeButton);
        }

        if (hardcoreModeButton != null)
        {
            hardcoreModeButton.onClick.RemoveAllListeners();
            hardcoreModeButton.onClick.AddListener(OnHardcoreModeClicked);
            AddHoverSound(hardcoreModeButton);
            _hardcoreModeBg = hardcoreModeButton.GetComponent<Image>();
            _hardcoreModeOutline = hardcoreModeButton.GetComponent<Outline>();
            var titleTr = hardcoreModeButton.transform.Find("Title");
            var descTr = hardcoreModeButton.transform.Find("Description");
            if (titleTr != null) _hardcoreTitleText = titleTr.GetComponent<TMP_Text>();
            if (descTr != null) _hardcoreDescText = descTr.GetComponent<TMP_Text>();
        }
    }

    public void CloseModeSelectionPopup()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (modeSelectPanel != null) modeSelectPanel.SetActive(false);
    }

    public void OnNormalModeClicked()
    {
        StartNewGameInMode(false);
    }

    public void OnHardcoreModeClicked()
    {
        if (GameState.IsHardcoreUnlocked())
            StartNewGameInMode(true);
    }

    private void Update()
    {
        // F12 debug shortcut to toggle Hardcore Mode unlock for testing/demo
        if (Input.GetKeyDown(KeyCode.F12))
        {
            GameState.ToggleHardcoreUnlock();
            AudioSystem.Instance?.PlayButtonClick();
            RefreshModePopupButtons();
        }

        if (modeSelectPanel != null && modeSelectPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                CloseModeSelectionPopup();
            }
        }
    }

    [ContextMenu("Toggle Hardcore Mode Unlock")]
    public void DebugToggleHardcoreUnlock()
    {
        GameState.ToggleHardcoreUnlock();
        RefreshModePopupButtons();
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.Clear();
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnPlayClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        ShowModeSelectionPopup();
    }

    private void ShowModeSelectionPopup()
    {
        if (modeSelectPanel == null)
        {
            BuildModeSelectionPopup();
        }

        RefreshModePopupButtons();
        modeSelectPanel.SetActive(true);
        modeSelectPanel.transform.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
    }

    private void ContinueExistingRun()
    {
        if (!GameState.HasActiveRun()) return;
        AudioSystem.Instance?.PlayButtonClick();
        Debug.Log($"[MainMenu] Continuing active run -> Act {GameState.CurrentAct}, Mode: {(GameState.IsHardcoreMode ? "HARDCORE" : "NORMAL")} -> Loading MapScene");
        SceneManager.LoadScene("MapScene");
    }

    private void StartNewGameInMode(bool hardcore)
    {
        AudioSystem.Instance?.PlayButtonClick();
        GameState.ResetRun();
        GameState.IsHardcoreMode = hardcore;
        Debug.Log($"[MainMenu] Starting NEW run (fresh map layout) -> Mode: {(hardcore ? "HARDCORE (No Intents)" : "NORMAL")} -> Loading MapScene");
        SceneManager.LoadScene("MapScene");
    }

    private void BuildModeSelectionPopup()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            StartNewGameInMode(false);
            return;
        }

        // Fullscreen dark backdrop
        modeSelectPanel = new GameObject("ModeSelectPanel");
        modeSelectPanel.transform.SetParent(canvas.transform, false);
        RectTransform overlayRT = modeSelectPanel.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        Image overlayBg = modeSelectPanel.AddComponent<Image>();
        overlayBg.color = new Color(0.01f, 0.02f, 0.05f, 0.88f);

        modeBackdropButton = modeSelectPanel.AddComponent<Button>();
        modeBackdropButton.transition = Selectable.Transition.None;
        modeBackdropButton.onClick.AddListener(CloseModeSelectionPopup);

        // Modal dialog box
        GameObject modal = new GameObject("ModalBox");
        modal.transform.SetParent(modeSelectPanel.transform, false);
        RectTransform modalRT = modal.AddComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.5f, 0.5f);
        modalRT.anchorMax = new Vector2(0.5f, 0.5f);
        modalRT.pivot = new Vector2(0.5f, 0.5f);
        modalRT.sizeDelta = new Vector2(740f, 590f);

        Image modalBg = modal.AddComponent<Image>();
        modalBg.color = new Color(0.06f, 0.08f, 0.14f, 0.98f);

        Outline modalOutline = modal.AddComponent<Outline>();
        modalOutline.effectColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        modalOutline.effectDistance = new Vector2(2f, -2f);

        // Header Title
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(modal.transform, false);
        RectTransform headerRT = headerObj.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 1f);
        headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = new Vector2(0f, -22f);
        headerRT.sizeDelta = new Vector2(-60f, 50f);

        TMP_Text headerTxt = CreateTMPText(headerObj, "// SELECT OPERATION MODE", 30, new Color(0.2f, 0.85f, 1f), TextAlignmentOptions.Center);
        headerTxt.fontStyle = FontStyles.Bold;

        // Close [X] Button
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(modal.transform, false);
        RectTransform closeRT = closeObj.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(1f, 1f);
        closeRT.anchoredPosition = new Vector2(-16f, -16f);
        closeRT.sizeDelta = new Vector2(44f, 44f);

        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.2f, 0.08f, 0.1f, 0.9f);
        modeCloseButton = closeObj.AddComponent<Button>();
        modeCloseButton.onClick.AddListener(CloseModeSelectionPopup);
        AddHoverSound(modeCloseButton);

        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeObj.transform, false);
        StretchRect(closeTxtObj.AddComponent<RectTransform>());
        CreateTMPText(closeTxtObj, "X", 24, new Color(1f, 0.4f, 0.4f), TextAlignmentOptions.Center);

        // 1. Continue Run Button (Top)
        CreateModeButton(
            modal.transform,
            "ContinueRunButton",
            new Vector2(0f, 130f),
            "[ CONTINUE RUN ]",
            "Resume your current session and map progress.",
            new Color(0.07f, 0.24f, 0.15f, 0.95f),
            new Color(0.25f, 0.95f, 0.55f, 0.9f),
            new Color(0.3f, 1f, 0.6f),
            ContinueExistingRun,
            out continueRunButton,
            out _continueRunBg,
            out _continueRunOutline,
            out _continueTitleText,
            out _continueDescText
        );

        // 2. New Run — Normal Mode Button (Middle)
        CreateModeButton(
            modal.transform,
            "NormalModeButton",
            new Vector2(0f, -5f),
            "[ NEW RUN — NORMAL MODE ]",
            "Generates a new map layout — Enemy intents & values are visible.",
            new Color(0.08f, 0.18f, 0.28f, 0.95f),
            new Color(0.2f, 0.85f, 1f, 0.9f),
            new Color(0.2f, 0.9f, 1f),
            () => StartNewGameInMode(false),
            out normalModeButton, out _, out _, out _, out _
        );

        // 3. New Run — Hardcore Mode Button (Bottom)
        CreateModeButton(
            modal.transform,
            "HardcoreModeButton",
            new Vector2(0f, -140f),
            "[ NEW RUN — HARDCORE MODE ]",
            "Generates a new map layout — All enemy intents & tooltips are hidden.",
            new Color(0.26f, 0.07f, 0.09f, 0.95f),
            new Color(1f, 0.3f, 0.3f, 0.9f),
            new Color(1f, 0.35f, 0.35f),
            () =>
            {
                if (GameState.IsHardcoreUnlocked())
                    StartNewGameInMode(true);
            },
            out hardcoreModeButton,
            out _hardcoreModeBg,
            out _hardcoreModeOutline,
            out _hardcoreTitleText,
            out _hardcoreDescText
        );

        // Footer hint
        GameObject footerObj = new GameObject("FooterHint");
        footerObj.transform.SetParent(modal.transform, false);
        RectTransform footerRT = footerObj.AddComponent<RectTransform>();
        footerRT.anchorMin = new Vector2(0f, 0f);
        footerRT.anchorMax = new Vector2(1f, 0f);
        footerRT.pivot = new Vector2(0.5f, 0f);
        footerRT.anchoredPosition = new Vector2(0f, 14f);
        footerRT.sizeDelta = new Vector2(-40f, 30f);
        CreateTMPText(footerObj, "ESC / Right-Click to Cancel   |   F12: Toggle Hardcore Unlock (Demo)", 15, new Color(0.55f, 0.6f, 0.68f), TextAlignmentOptions.Center);
    }

    private void CreateModeButton(
        Transform parent,
        string objName,
        Vector2 anchoredPos,
        string title,
        string subtitle,
        Color bgColor,
        Color borderColor,
        Color titleColor,
        UnityEngine.Events.UnityAction onClick,
        out Button btn,
        out Image bgImg,
        out Outline outline,
        out TMP_Text titleTmp,
        out TMP_Text descTmp)
    {
        GameObject btnObj = new GameObject(objName);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(640f, 114f);

        bgImg = btnObj.AddComponent<Image>();
        bgImg.color = bgColor;

        outline = btnObj.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(2f, -2f);

        btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = bgImg;
        btn.onClick.AddListener(onClick);
        AddHoverSound(btn);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(btnObj.transform, false);
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.5f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = new Vector2(20f, 2f);
        titleRT.offsetMax = new Vector2(-20f, -8f);
        titleTmp = CreateTMPText(titleObj, title, 26, titleColor, TextAlignmentOptions.Center);
        titleTmp.fontStyle = FontStyles.Bold;

        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(btnObj.transform, false);
        RectTransform descRT = descObj.AddComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0f, 0f);
        descRT.anchorMax = new Vector2(1f, 0.5f);
        descRT.offsetMin = new Vector2(20f, 8f);
        descRT.offsetMax = new Vector2(-20f, -2f);
        descTmp = CreateTMPText(descObj, subtitle, 17, new Color(0.82f, 0.86f, 0.92f), TextAlignmentOptions.Center);
    }

    private void RefreshModePopupButtons()
    {
        // 1. Refresh Continue Button
        if (continueRunButton != null)
        {
            bool hasRun = GameState.HasActiveRun();
            continueRunButton.interactable = hasRun;

            if (hasRun)
            {
                string modeLabel = GameState.IsHardcoreMode ? "HARDCORE" : "NORMAL";
                int curHp = GameState.PlayerCurrentHP > 0 ? GameState.PlayerCurrentHP : GameState.PlayerMaxHP;
                int deckCount = GameState.PlayerDeck != null ? GameState.PlayerDeck.Count : 0;

                if (_continueRunBg != null) _continueRunBg.color = new Color(0.07f, 0.24f, 0.15f, 0.95f);
                if (_continueRunOutline != null) _continueRunOutline.effectColor = new Color(0.25f, 0.95f, 0.55f, 0.9f);
                if (_continueTitleText != null)
                {
                    _continueTitleText.text = $"[ CONTINUE RUN - ACT {GameState.CurrentAct} ]";
                    _continueTitleText.color = new Color(0.3f, 1f, 0.6f);
                }
                if (_continueDescText != null)
                {
                    _continueDescText.text = $"Mode: {modeLabel}   |   HP: {curHp}/{GameState.PlayerMaxHP}   |   Deck: {deckCount} Cards";
                    _continueDescText.color = new Color(0.82f, 0.95f, 0.86f);
                }
            }
            else
            {
                if (_continueRunBg != null) _continueRunBg.color = new Color(0.11f, 0.12f, 0.15f, 0.85f);
                if (_continueRunOutline != null) _continueRunOutline.effectColor = new Color(0.3f, 0.32f, 0.36f, 0.5f);
                if (_continueTitleText != null)
                {
                    _continueTitleText.text = "[ CONTINUE RUN - NO ACTIVE SESSION ]";
                    _continueTitleText.color = new Color(0.48f, 0.5f, 0.55f);
                }
                if (_continueDescText != null)
                {
                    _continueDescText.text = "Start a New Run below to generate a new procedural map.";
                    _continueDescText.color = new Color(0.45f, 0.47f, 0.52f);
                }
            }
        }

        // 2. Refresh Hardcore Mode Button
        if (hardcoreModeButton == null) return;

        bool unlocked = GameState.IsHardcoreUnlocked();
        hardcoreModeButton.interactable = unlocked;

        if (unlocked)
        {
            if (_hardcoreModeBg != null) _hardcoreModeBg.color = new Color(0.26f, 0.07f, 0.09f, 0.95f);
            if (_hardcoreModeOutline != null) _hardcoreModeOutline.effectColor = new Color(1f, 0.3f, 0.3f, 0.9f);
            if (_hardcoreTitleText != null)
            {
                _hardcoreTitleText.text = "[ NEW RUN - HARDCORE MODE ]";
                _hardcoreTitleText.color = new Color(1f, 0.35f, 0.35f);
            }
            if (_hardcoreDescText != null)
            {
                _hardcoreDescText.text = "Generates a new map layout - All enemy intents & tooltips are hidden.";
                _hardcoreDescText.color = new Color(0.9f, 0.8f, 0.8f);
            }
        }
        else
        {
            if (_hardcoreModeBg != null) _hardcoreModeBg.color = new Color(0.12f, 0.12f, 0.15f, 0.9f);
            if (_hardcoreModeOutline != null) _hardcoreModeOutline.effectColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
            if (_hardcoreTitleText != null)
            {
                _hardcoreTitleText.text = "[ HARDCORE MODE - LOCKED ]";
                _hardcoreTitleText.color = new Color(0.55f, 0.55f, 0.6f);
            }
            if (_hardcoreDescText != null)
            {
                _hardcoreDescText.text = "Clear Normal Mode (Defeat Act 3 Boss) to unlock.";
                _hardcoreDescText.color = new Color(0.5f, 0.5f, 0.55f);
            }
        }
    }

    private TMP_Text CreateTMPText(GameObject go, string content, float fontSize, Color color, TextAlignmentOptions align)
    {
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (_menuFont != null) tmp.font = _menuFont;
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return tmp;
    }

    private void StretchRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void OnArchiveClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (archivePanel != null)
        {
            archivePanel.SetActive(true);
            var archiveUI = archivePanel.GetComponent<ArchiveUI>();
            if (archiveUI != null)
                archiveUI.Show();
        }
    }

    private void OnSettingsClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    private void OnSettingsBackClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnQuitClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        Debug.Log("[MainMenu] Quit Clicked");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
