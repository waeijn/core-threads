using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable card selection overlay. Displays the player's deck cards as clickable entries.
/// Used by RewardSystem (BattleScene), and NodeChoicePanelUI (MapScene) for Duplicate/Purge flows.
/// </summary>
public class CardSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cardListContainer;
    [SerializeField] private GameObject cardEntryPrefab;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    private Action<CardData> onCardSelected;
    private List<GameObject> spawnedEntries = new();

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the card selection overlay with the given eligible cards.
    /// </summary>
    /// <param name="eligibleCards">Distinct cards the player can pick from.</param>
    /// <param name="title">Header text (e.g. "SELECT A CARD TO DUPLICATE").</param>
    /// <param name="onSelect">Callback invoked with the selected CardData.</param>
    /// <param name="showCounts">If true, shows how many copies of each card are in the deck.</param>
    public void Show(List<CardData> eligibleCards, string title, Action<CardData> onSelect, bool showCounts = true)
    {
        onCardSelected = onSelect;
        if (titleText != null) titleText.text = title;

        // Clear previous entries
        foreach (var entry in spawnedEntries)
            Destroy(entry);
        spawnedEntries.Clear();

        // Create card entries
        foreach (var card in eligibleCards)
        {
            if (cardEntryPrefab == null || cardListContainer == null) continue;

            GameObject entry = Instantiate(cardEntryPrefab, cardListContainer);
            spawnedEntries.Add(entry);

            // Set card sprite (the actual image from the card data)
            Image cardImage = entry.GetComponent<Image>();
            if (cardImage != null && card.Image != null)
            {
                cardImage.sprite = card.Image;
                cardImage.color = Color.white; // Ensure it's not tinted if the prefab had a tint
            }

            // Set optional count text (assume there's a child TMP_Text for the count badge)
            TMP_Text countText = entry.GetComponentInChildren<TMP_Text>();
            if (countText != null)
            {
                if (showCounts)
                {
                    countText.text = $"x{GameState.GetCardCount(card)}";
                    countText.gameObject.SetActive(true);
                }
                else
                {
                    countText.gameObject.SetActive(false);
                }
            }

            // Set click handler
            Button btn = entry.GetComponent<Button>();
            if (btn != null)
            {
                CardData capturedCard = card;
                btn.onClick.AddListener(() => OnEntryClicked(capturedCard));
            }
        }

        gameObject.SetActive(true);
    }

    private void OnEntryClicked(CardData card)
    {
        onCardSelected?.Invoke(card);
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onCardSelected = null;
    }
}
