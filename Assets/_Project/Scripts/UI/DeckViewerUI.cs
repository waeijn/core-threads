using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "View Deck" overlay for the Map scene. Shows the player's full current deck
/// as a scrollable grid of card thumbnails. Includes animated open/close.
/// </summary>
public class DeckViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   titleText;
    [SerializeField] private TMP_Text   cardCountText;
    [SerializeField] private Transform  cardGrid;
    [SerializeField] private Button     backgroundOverlay;

    [Header("Prefab")]
    [SerializeField] private CardThumbnailUI cardThumbnailPrefab;

    [Header("Animation")]
    [SerializeField] private float openDuration  = 0.20f;
    [SerializeField] private float closeDuration = 0.15f;
    [SerializeField] private float cardStaggerDelay = 0.04f;

    private CanvasGroup       _cg;
    private RectTransform     _panelRT;
    private List<GameObject>  _spawned = new();
    private System.Collections.IEnumerator _anim;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (panel != null)
        {
            _cg = panel.GetComponent<CanvasGroup>();
            if (_cg == null) _cg = panel.AddComponent<CanvasGroup>();
            _panelRT = panel.GetComponent<RectTransform>();
        }
        gameObject.SetActive(false); // Hide the root on start
    }

    private void OnEnable()
    {
        if (backgroundOverlay != null) backgroundOverlay.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); Hide(); });
    }

    private void OnDisable()
    {
        if (backgroundOverlay != null) backgroundOverlay.onClick.RemoveListener(Hide);
    }

    public void Show()
    {
        IsOpen = true;
        gameObject.SetActive(true); // Ensure the script's own GameObject is active before coroutines!
        transform.SetAsLastSibling(); // Ensure it renders OVER the TopBar and everything else
        if (panel != null) panel.SetActive(true);
        
        if (CardsSystem.Instance != null) CardsSystem.Instance.HideHand();

        ClearCards();

        List<CardData> deck = GameState.GetFullDeck();
        int count = deck.Count;

        if (titleText    != null) titleText.text    = "YOUR DECK";
        if (cardCountText!= null) cardCountText.text = $"{count} Cards";

        foreach (var cardData in deck)
        {
            if (cardThumbnailPrefab == null || cardGrid == null) continue;

            // Build a temporary Card wrapper to satisfy CardThumbnailUI.Setup
            Card card = new(cardData);
            CardThumbnailUI thumb = Instantiate(cardThumbnailPrefab, cardGrid);
            thumb.Setup(card);
            
            // Start hidden and small for the bounce-in
            var rt = thumb.GetComponent<RectTransform>();
            if (rt != null) rt.localScale = Vector3.zero;
            
            _spawned.Add(thumb.gameObject);
        }

        StartAnim(AnimOpen());
    }

    public void Hide()
    {
        IsOpen = false;
        if (CardsSystem.Instance != null) CardsSystem.Instance.ShowHand();
        StartAnim(AnimClose());
    }

    private void Update()
    {
        if (IsOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            AudioSystem.Instance?.PlayButtonClick();
            Hide();
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void ClearCards()
    {
        foreach (var go in _spawned)
            if (go != null) Destroy(go);
        _spawned.Clear();
    }

    private void StartAnim(System.Collections.IEnumerator routine)
    {
        if (!gameObject.activeInHierarchy) return; // Prevent coroutine errors if unexpectedly inactive
        if (_anim != null) StopCoroutine(_anim);
        _anim = routine;
        StartCoroutine(_anim);
    }

    // ── Animations ─────────────────────────────────────────────────────────

    private System.Collections.IEnumerator AnimOpen()
    {
        if (_cg == null || _panelRT == null) yield break;
        _panelRT.localScale = Vector3.one * 0.85f;
        _cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            _panelRT.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);
            _cg.alpha = t;
            yield return null;
        }
        _panelRT.localScale = Vector3.one;
        _cg.alpha = 1f;
        
        // Stagger card thumbnails in with a bounce
        StartCoroutine(StaggerCards());
    }

    private System.Collections.IEnumerator AnimClose()
    {
        if (_cg == null || _panelRT == null)
        {
            gameObject.SetActive(false);
            ClearCards();
            yield break;
        }

        float elapsed = 0f;
        Vector3 startScale = _panelRT.localScale;
        float startAlpha   = _cg.alpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            _panelRT.localScale = Vector3.Lerp(startScale, Vector3.one * 0.85f, t);
            _cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        gameObject.SetActive(false);
        ClearCards();
    }
    
    private System.Collections.IEnumerator StaggerCards()
    {
        // Overshoot bounce target scale
        Vector3 overshoot = Vector3.one * 1.10f;
        float   popIn     = 0.12f;
        float   settleOut = 0.06f;

        foreach (var go in _spawned)
        {
            if (go == null) continue;
            StartCoroutine(BounceCard(go.GetComponent<RectTransform>(), popIn, settleOut, overshoot));
            yield return new WaitForSecondsRealtime(cardStaggerDelay);
        }
    }

    private System.Collections.IEnumerator BounceCard(RectTransform rt, float popIn, float settleOut, Vector3 overshoot)
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
