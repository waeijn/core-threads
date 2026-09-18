using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen overlay that shows the contents of a card pile in a scrollable grid.
/// Slay the Spire style: Draw pile shows cards in shuffled order, Discard shows newest first.
/// Includes animated open/close and staggered card bounce-in.
/// </summary>
public class PileViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text cardCountText;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private Button backgroundOverlay;

    [Header("Card Thumbnail")]
    [SerializeField] private CardThumbnailUI cardThumbnailPrefab;

    [Header("Animation")]
    [SerializeField] private float openDuration  = 0.20f;
    [SerializeField] private float closeDuration = 0.15f;
    [SerializeField] private float cardStaggerDelay = 0.04f;

    private readonly List<CardThumbnailUI> spawnedThumbnails = new();
    private CanvasGroup _canvasGroup;
    private RectTransform _panelRT;
    private Coroutine _animRoutine;
    
    public enum PileType { None, Draw, Discard, Exhaust }
    public PileType CurrentPile { get; private set; }

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (panel != null)
        {
            _canvasGroup = panel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = panel.AddComponent<CanvasGroup>();

            _panelRT = panel.GetComponent<RectTransform>();
        }
        gameObject.SetActive(false); // Hide the root on start
    }

    private void OnEnable()
    {
        if (backgroundOverlay != null)
            backgroundOverlay.onClick.AddListener(Hide);
    }

    private void OnDisable()
    {
        if (backgroundOverlay != null)
            backgroundOverlay.onClick.RemoveListener(Hide);
    }

    /// <summary>
    /// Shows the draw pile. Per Slay the Spire rules, cards are displayed 
    /// in random order so the player can see what's remaining but not the exact draw order.
    /// </summary>
    public void ShowDrawPile()
    {
        CurrentPile = PileType.Draw;
        IReadOnlyList<Card> cards = CardsSystem.Instance.DrawPileCards;

        // Create a shuffled copy for display (don't reveal draw order)
        List<Card> displayList = new(cards);
        displayList.Shuffle();

        Show("Draw Pile", displayList);
    }

    /// <summary>
    /// Shows the discard pile. Cards are displayed in order (most recently discarded first).
    /// </summary>
    public void ShowDiscardPile()
    {
        CurrentPile = PileType.Discard;
        IReadOnlyList<Card> cards = CardsSystem.Instance.DiscardPileCards;

        // Show newest first (reverse order)
        List<Card> displayList = new(cards);
        displayList.Reverse();

        Show("Discard Pile", displayList);
    }

    /// <summary>
    /// Shows the exhaust pile. Cards that were exhausted during combat.
    /// </summary>
    public void ShowExhaustPile()
    {
        CurrentPile = PileType.Exhaust;
        IReadOnlyList<Card> cards = CardsSystem.Instance.ExhaustPileCards;
        List<Card> displayList = new(cards);
        displayList.Reverse();

        Show("Exhaust Pile", displayList);
    }

    private void Show(string title, List<Card> cards)
    {
        IsOpen = true;
        gameObject.SetActive(true); // Ensure the script's own GameObject is active before coroutines!
        if (panel != null) panel.SetActive(true);

        if (CardsSystem.Instance != null) CardsSystem.Instance.HideHand();

        // Clear old thumbnails
        ClearThumbnails();

        // Set title and count
        if (titleText != null)
            titleText.text = title;
        if (cardCountText != null)
            cardCountText.text = $"{cards.Count} Cards";

        // Spawn card thumbnails (hidden initially for stagger animation)
        foreach (var card in cards)
        {
            if (cardThumbnailPrefab != null && cardGrid != null)
            {
                CardThumbnailUI thumbnail = Instantiate(cardThumbnailPrefab, cardGrid);
                thumbnail.Setup(card);
                // Start hidden and small for the bounce-in
                var rt = thumbnail.GetComponent<RectTransform>();
                if (rt != null) rt.localScale = Vector3.zero;
                spawnedThumbnails.Add(thumbnail);
            }
        }

        // Show the panel then animate open
        if (_animRoutine != null) StopCoroutine(_animRoutine);
        if (gameObject.activeInHierarchy) _animRoutine = StartCoroutine(AnimateOpen());
    }

    public void Hide()
    {
        IsOpen = false;
        CurrentPile = PileType.None;
        if (CardsSystem.Instance != null) CardsSystem.Instance.ShowHand();
        if (_animRoutine != null) StopCoroutine(_animRoutine);
        if (gameObject.activeInHierarchy) _animRoutine = StartCoroutine(AnimateClose());
    }

    private void ClearThumbnails()
    {
        foreach (var thumbnail in spawnedThumbnails)
        {
            if (thumbnail != null)
                Destroy(thumbnail.gameObject);
        }
        spawnedThumbnails.Clear();
    }

    // ── Animations ─────────────────────────────────────────────────────────

    private IEnumerator AnimateOpen()
    {
        if (_canvasGroup == null || _panelRT == null) yield break;

        // Start: small and transparent
        _panelRT.localScale = Vector3.one * 0.85f;
        _canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            _panelRT.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);
            _canvasGroup.alpha  = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        _panelRT.localScale = Vector3.one;
        _canvasGroup.alpha  = 1f;

        // Stagger card thumbnails in with a bounce
        StartCoroutine(StaggerCards());
    }

    private IEnumerator AnimateClose()
    {
        if (_canvasGroup == null || _panelRT == null)
        {
            gameObject.SetActive(false);
            ClearThumbnails();
            yield break;
        }

        float elapsed = 0f;
        Vector3 startScale = _panelRT.localScale;
        float   startAlpha = _canvasGroup.alpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            _panelRT.localScale = Vector3.Lerp(startScale, Vector3.one * 0.85f, t);
            _canvasGroup.alpha  = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        gameObject.SetActive(false);
        ClearThumbnails();
    }

    private IEnumerator StaggerCards()
    {
        // Overshoot bounce target scale
        Vector3 overshoot = Vector3.one * 1.10f;
        float   popIn     = 0.12f;
        float   settleOut = 0.06f;

        foreach (var thumb in spawnedThumbnails)
        {
            if (thumb == null) continue;
            StartCoroutine(BounceCard(thumb.GetComponent<RectTransform>(), popIn, settleOut, overshoot));
            yield return new WaitForSecondsRealtime(cardStaggerDelay);
        }
    }

    private IEnumerator BounceCard(RectTransform rt, float popIn, float settleOut, Vector3 overshoot)
    {
        if (rt == null) yield break;

        // Pop in to overshoot
        float elapsed = 0f;
        while (elapsed < popIn)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / popIn);
            rt.localScale = Vector3.Lerp(Vector3.zero, overshoot, t);
            yield return null;
        }

        // Settle back to normal
        elapsed = 0f;
        while (elapsed < settleOut)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / settleOut);
            rt.localScale = Vector3.Lerp(overshoot, Vector3.one, t);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }
}
