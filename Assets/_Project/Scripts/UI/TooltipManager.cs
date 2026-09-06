using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    private static TooltipManager instance;
    private GameObject tooltipObj;
    private TMP_Text tooltipText;
    private RectTransform rectTransform;
    private RectTransform canvasRect;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;

        var go = new GameObject("TooltipManager");
        Object.DontDestroyOnLoad(go);
        instance = go.AddComponent<TooltipManager>();
        instance.Setup();
    }

    private void Setup()
    {
        var canvasObj = new GameObject("TooltipCanvas");
        canvasObj.transform.SetParent(transform);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>().enabled = false;

        canvasRect = canvasObj.GetComponent<RectTransform>();

        tooltipObj = new GameObject("TooltipPanel");
        tooltipObj.transform.SetParent(canvasObj.transform, false);
        rectTransform = tooltipObj.AddComponent<RectTransform>();
        // Anchor to center so anchoredPosition matches ScreenPointToLocalPointInRectangle output
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 1f); // Top-left pivot: tooltip hangs below-right of cursor

        var bg = tooltipObj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        bg.raycastTarget = false;

        var outline = tooltipObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.8f, 1f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);

        // Add layout groups to auto-size
        var layout = tooltipObj.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        var fitter = tooltipObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(tooltipObj.transform, false);
        var textRt = textObj.AddComponent<RectTransform>();

        tooltipText = textObj.AddComponent<TextMeshProUGUI>();
        tooltipText.fontSize = 22;
        tooltipText.color = Color.white;
        tooltipText.alignment = TextAlignmentOptions.TopLeft;
        tooltipText.enableWordWrapping = false; // Disable wrapping to let it grow horizontally, or use a max width layout element
        tooltipText.raycastTarget = false;
        
        // Add LayoutElement to limit max width if desired, but for short intents, false is fine.
        var le = textObj.AddComponent<LayoutElement>();
        le.preferredWidth = -1; // Let it be natural

        tooltipObj.SetActive(false);
    }

    private void Update()
    {
        if (Camera.main == null) return;
        
        // Don't show world tooltips if the player is dragging or if the battle is over (reward screen)
        if (Interactions.Instance != null && !Interactions.Instance.PlayerCanHover())
        {
            if (tooltipObj.activeSelf) HideInternal();
            return;
        }

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);

        bool hoveringIntent = false;

        foreach (var hit in hits)
        {
            var intentTooltip = hit.GetComponent<IntentTooltip>();
            if (intentTooltip != null && intentTooltip.Enemy != null)
            {
                hoveringIntent = true;
                string text = intentTooltip.Enemy.GetIntentTooltip(intentTooltip.IsSecondary);
                if (!string.IsNullOrEmpty(text) && (!tooltipObj.activeSelf || tooltipText.text != text))
                {
                    ShowInternal(text);
                }
                break;
            }
        }

        if (!hoveringIntent && tooltipObj.activeSelf)
        {
            HideInternal();
        }

        if (tooltipObj.activeSelf)
        {
            PositionTooltip();
        }
    }

    private void PositionTooltip()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, Input.mousePosition, null, out localPoint);

        // Offset: show tooltip to the right and slightly below cursor
        Vector2 pos = localPoint + new Vector2(20f, -10f);

        // Clamp: keep tooltip on screen
        Vector2 halfCanvas = canvasRect.sizeDelta * 0.5f;
        Vector2 tooltipSize = rectTransform.sizeDelta;

        // Right edge check — flip to left of cursor
        if (pos.x + tooltipSize.x > halfCanvas.x)
            pos.x = localPoint.x - tooltipSize.x - 10f;

        // Bottom edge check — flip above cursor
        if (pos.y - tooltipSize.y < -halfCanvas.y)
            pos.y = localPoint.y + 10f;

        rectTransform.anchoredPosition = pos;
    }

    private void ShowInternal(string content)
    {
        tooltipText.text = content;
        // Layout components handle the sizing automatically now
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        tooltipObj.SetActive(true);
    }

    private void HideInternal()
    {
        tooltipObj.SetActive(false);
    }

    public static void Show(string content)
    {
        if (instance != null) instance.ShowInternal(content);
    }

    public static void Hide()
    {
        if (instance != null) instance.HideInternal();
    }
}
