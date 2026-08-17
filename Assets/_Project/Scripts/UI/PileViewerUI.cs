using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen overlay that shows the contents of a card pile in a scrollable grid.
/// Slay the Spire style: Draw pile shows cards in shuffled order, Discard shows newest first.
/// </summary>
public class PileViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text cardCountText;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundOverlay;

    [Header("Card Thumbnail")]
    [SerializeField] private CardThumbnailUI cardThumbnailPrefab;

    private readonly List<CardThumbnailUI> spawnedThumbnails = new();

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        if (backgroundOverlay != null)
            backgroundOverlay.onClick.AddListener(Hide);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);
        if (backgroundOverlay != null)
            backgroundOverlay.onClick.RemoveListener(Hide);
    }

    /// <summary>
    /// Shows the draw pile. Per Slay the Spire rules, cards are displayed 
    /// in random order so the player can see what's remaining but not the exact draw order.
    /// </summary>
    public void ShowDrawPile()
    {
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
        IReadOnlyList<Card> cards = CardsSystem.Instance.ExhaustPileCards;
        List<Card> displayList = new(cards);
        displayList.Reverse();

        Show("Exhaust Pile", displayList);
    }

    private void Show(string title, List<Card> cards)
    {
        // Clear old thumbnails
        ClearThumbnails();

        // Set title and count
        if (titleText != null)
            titleText.text = title;
        if (cardCountText != null)
            cardCountText.text = $"{cards.Count} Cards";

        // Spawn card thumbnails
        foreach (var card in cards)
        {
            if (cardThumbnailPrefab != null && cardGrid != null)
            {
                CardThumbnailUI thumbnail = Instantiate(cardThumbnailPrefab, cardGrid);
                thumbnail.Setup(card);
                spawnedThumbnails.Add(thumbnail);
            }
        }

        // Show the panel
        if (panel != null)
            panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);

        ClearThumbnails();
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
}
