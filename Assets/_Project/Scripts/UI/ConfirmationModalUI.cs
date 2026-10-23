using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Universal confirmation modal dialog for significant or destructive actions:
/// - Quitting application to desktop
/// - Quitting current run to Main Menu
/// - Overwriting an active run
/// - Permanently purging a card from deck
/// </summary>
public class ConfirmationModalUI : MonoBehaviour
{
    private static ConfirmationModalUI _instance;

    [Header("Runtime References")]
    private GameObject _modalBox;
    private TMP_Text _titleText;
    private TMP_Text _messageText;
    private Button _confirmButton;
    private TMP_Text _confirmButtonText;
    private Image _confirmButtonBg;
    private Outline _confirmButtonOutline;
    private Button _cancelButton;
    private TMP_Text _cancelButtonText;
    private Button _closeButton;

    private Action _onConfirm;
    private Action _onCancel;

    /// <summary>
    /// Displays a confirmation dialog modal on the active Canvas.
    /// </summary>
    public static void Show(
        string title,
        string message,
        string confirmText = "CONFIRM",
        string cancelText = "CANCEL",
        Action onConfirm = null,
        Action onCancel = null,
        bool isDanger = true)
    {
        EnsureInstance();
        if (_instance != null)
        {
            _instance.SetupAndOpen(title, message, confirmText, cancelText, onConfirm, onCancel, isDanger);
        }
    }

    private static void EnsureInstance()
    {
        // Unity overloaded null-check safely detects destroyed instances across scene unloads
        if (_instance != null) return;

        Canvas canvas = CanvasHelper.GetSceneCanvas();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ConfirmationCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject modalRoot = new GameObject("ConfirmationModalUI");
        modalRoot.transform.SetParent(canvas.transform, false);
        _instance = modalRoot.AddComponent<ConfirmationModalUI>();
        _instance.BuildUI(canvas);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void BuildUI(Canvas canvas)
    {
        RectTransform rootRT = gameObject.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        // 1. Separate Backdrop child (sits behind ModalBox so it NEVER intercepts clicks on ModalBox)
        GameObject backdropObj = new GameObject("Backdrop");
        backdropObj.transform.SetParent(transform, false);
        RectTransform backdropRT = backdropObj.AddComponent<RectTransform>();
        backdropRT.anchorMin = Vector2.zero;
        backdropRT.anchorMax = Vector2.one;
        backdropRT.offsetMin = Vector2.zero;
        backdropRT.offsetMax = Vector2.zero;

        Image overlayBg = backdropObj.AddComponent<Image>();
        overlayBg.color = new Color(0.01f, 0.02f, 0.05f, 0.88f);
        overlayBg.raycastTarget = true;

        Button backdropBtn = backdropObj.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.onClick.AddListener(OnCancelClicked);

        // Find font asset from scene if available
        TMP_FontAsset font = null;
        var existingTmp = canvas.GetComponentInChildren<TMP_Text>(true);
        if (existingTmp != null) font = existingTmp.font;

        // 2. Modal Box (Sibling of Backdrop, in front)
        _modalBox = new GameObject("ModalBox");
        _modalBox.transform.SetParent(transform, false);
        RectTransform modalRT = _modalBox.AddComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.5f, 0.5f);
        modalRT.anchorMax = new Vector2(0.5f, 0.5f);
        modalRT.pivot = new Vector2(0.5f, 0.5f);
        modalRT.sizeDelta = new Vector2(640f, 340f);

        Image modalBg = _modalBox.AddComponent<Image>();
        modalBg.color = new Color(0.05f, 0.07f, 0.12f, 0.98f);
        modalBg.raycastTarget = true;

        Outline modalOutline = _modalBox.AddComponent<Outline>();
        modalOutline.effectColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        modalOutline.effectDistance = new Vector2(2f, -2f);

        // Header Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(_modalBox.transform, false);
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -20f);
        titleRT.sizeDelta = new Vector2(-60f, 44f);

        _titleText = CreateTMPText(titleObj, "CONFIRM ACTION", 26, new Color(0.2f, 0.85f, 1f), TextAlignmentOptions.Center, font);
        _titleText.fontStyle = FontStyles.Bold;

        // Close [X] Button
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(_modalBox.transform, false);
        RectTransform closeRT = closeObj.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(1f, 1f);
        closeRT.anchoredPosition = new Vector2(-12f, -12f);
        closeRT.sizeDelta = new Vector2(36f, 36f);

        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.25f, 0.1f, 0.12f, 0.9f);
        closeBg.raycastTarget = true;
        _closeButton = closeObj.AddComponent<Button>();
        _closeButton.onClick.AddListener(OnCancelClicked);
        closeObj.AddComponent<UIHoverSound>();

        GameObject closeTxtObj = new GameObject("XText");
        closeTxtObj.transform.SetParent(closeObj.transform, false);
        RectTransform closeTxtRT = closeTxtObj.AddComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero;
        closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.offsetMin = Vector2.zero;
        closeTxtRT.offsetMax = Vector2.zero;
        CreateTMPText(closeTxtObj, "X", 20, new Color(1f, 0.4f, 0.4f), TextAlignmentOptions.Center, font);

        // Message Body
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(_modalBox.transform, false);
        RectTransform msgRT = msgObj.AddComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0f, 0.35f);
        msgRT.anchorMax = new Vector2(1f, 0.82f);
        msgRT.offsetMin = new Vector2(40f, 0f);
        msgRT.offsetMax = new Vector2(-40f, 0f);

        _messageText = CreateTMPText(msgObj, "Are you sure you want to proceed?", 18, new Color(0.85f, 0.9f, 0.96f), TextAlignmentOptions.Center, font);
        _messageText.enableWordWrapping = true;

        // Button Container
        GameObject btnContainer = new GameObject("ButtonsContainer");
        btnContainer.transform.SetParent(_modalBox.transform, false);
        RectTransform btnContainerRT = btnContainer.AddComponent<RectTransform>();
        btnContainerRT.anchorMin = new Vector2(0f, 0f);
        btnContainerRT.anchorMax = new Vector2(1f, 0.32f);
        btnContainerRT.offsetMin = new Vector2(40f, 16f);
        btnContainerRT.offsetMax = new Vector2(-40f, -4f);

        // Confirm Button
        GameObject confirmObj = new GameObject("ConfirmButton");
        confirmObj.transform.SetParent(btnContainer.transform, false);
        RectTransform confirmRT = confirmObj.AddComponent<RectTransform>();
        confirmRT.anchorMin = new Vector2(0.05f, 0.1f);
        confirmRT.anchorMax = new Vector2(0.48f, 0.9f);
        confirmRT.offsetMin = Vector2.zero;
        confirmRT.offsetMax = Vector2.zero;

        _confirmButtonBg = confirmObj.AddComponent<Image>();
        _confirmButtonBg.raycastTarget = true;
        _confirmButtonOutline = confirmObj.AddComponent<Outline>();
        _confirmButtonOutline.effectDistance = new Vector2(2f, -2f);
        _confirmButton = confirmObj.AddComponent<Button>();
        _confirmButton.onClick.AddListener(OnConfirmClicked);
        confirmObj.AddComponent<UIHoverSound>();

        GameObject confirmTxtObj = new GameObject("Text");
        confirmTxtObj.transform.SetParent(confirmObj.transform, false);
        RectTransform confirmTxtRT = confirmTxtObj.AddComponent<RectTransform>();
        confirmTxtRT.anchorMin = Vector2.zero;
        confirmTxtRT.anchorMax = Vector2.one;
        confirmTxtRT.offsetMin = Vector2.zero;
        confirmTxtRT.offsetMax = Vector2.zero;
        _confirmButtonText = CreateTMPText(confirmTxtObj, "CONFIRM", 20, Color.white, TextAlignmentOptions.Center, font);
        _confirmButtonText.fontStyle = FontStyles.Bold;

        // Cancel Button
        GameObject cancelObj = new GameObject("CancelButton");
        cancelObj.transform.SetParent(btnContainer.transform, false);
        RectTransform cancelRT = cancelObj.AddComponent<RectTransform>();
        cancelRT.anchorMin = new Vector2(0.52f, 0.1f);
        cancelRT.anchorMax = new Vector2(0.95f, 0.9f);
        cancelRT.offsetMin = Vector2.zero;
        cancelRT.offsetMax = Vector2.zero;

        Image cancelBg = cancelObj.AddComponent<Image>();
        cancelBg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
        cancelBg.raycastTarget = true;
        Outline cancelOutline = cancelObj.AddComponent<Outline>();
        cancelOutline.effectColor = new Color(0.35f, 0.45f, 0.58f, 0.8f);
        cancelOutline.effectDistance = new Vector2(1.5f, -1.5f);
        _cancelButton = cancelObj.AddComponent<Button>();
        _cancelButton.onClick.AddListener(OnCancelClicked);
        cancelObj.AddComponent<UIHoverSound>();

        GameObject cancelTxtObj = new GameObject("Text");
        cancelTxtObj.transform.SetParent(cancelObj.transform, false);
        RectTransform cancelTxtRT = cancelTxtObj.AddComponent<RectTransform>();
        cancelTxtRT.anchorMin = Vector2.zero;
        cancelTxtRT.anchorMax = Vector2.one;
        cancelTxtRT.offsetMin = Vector2.zero;
        cancelTxtRT.offsetMax = Vector2.zero;
        _cancelButtonText = CreateTMPText(cancelTxtObj, "CANCEL", 20, new Color(0.8f, 0.86f, 0.94f), TextAlignmentOptions.Center, font);
        _cancelButtonText.fontStyle = FontStyles.Bold;

        gameObject.SetActive(false);
    }

    private void SetupAndOpen(
        string title,
        string message,
        string confirmText,
        string cancelText,
        Action onConfirm,
        Action onCancel,
        bool isDanger)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (_titleText != null)
        {
            _titleText.text = title;
            _titleText.color = isDanger ? new Color(1f, 0.38f, 0.38f) : new Color(0.2f, 0.85f, 1f);
        }

        if (_messageText != null)
        {
            _messageText.text = message;
        }

        if (_confirmButtonText != null)
        {
            _confirmButtonText.text = confirmText;
        }

        if (_cancelButtonText != null)
        {
            _cancelButtonText.text = cancelText;
        }

        // Danger vs Normal coloring for Confirm Button
        if (_confirmButtonBg != null)
        {
            _confirmButtonBg.color = isDanger
                ? new Color(0.55f, 0.12f, 0.14f, 0.96f)
                : new Color(0.08f, 0.34f, 0.46f, 0.96f);
        }

        if (_confirmButtonOutline != null)
        {
            _confirmButtonOutline.effectColor = isDanger
                ? new Color(1f, 0.32f, 0.32f, 0.9f)
                : new Color(0.2f, 0.9f, 1f, 0.9f);
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void OnConfirmClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
        var action = _onConfirm;
        _onConfirm = null;
        _onCancel = null;
        action?.Invoke();
    }

    private void OnCancelClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
        var action = _onCancel;
        _onConfirm = null;
        _onCancel = null;
        action?.Invoke();
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                OnCancelClicked();
            }
        }
    }

    private TMP_Text CreateTMPText(GameObject go, string text, float size, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        return tmp;
    }
}

/// <summary>
/// Lightweight hover component that does NOT implement IPointerClickHandler,
/// ensuring Button.onClick is NEVER intercepted or suppressed by pointer event routing.
/// </summary>
public class UIHoverSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        var btn = GetComponent<Button>();
        if (btn == null || btn.interactable)
        {
            AudioSystem.Instance?.PlayButtonHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AudioSystem.Instance?.StopHoverSFX();
    }
}
