using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable card selection overlay. Displays the player's deck cards as clickable entries.
/// Used by RewardSystem (GameScene), and NodeChoicePanelUI (MapScene) for Duplicate/Purge flows.
/// </summary>
public class CardSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cardListContainer;
    [SerializeField] private GameObject cardEntryPrefab;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    private Action<CardData> onCardSelected;
    private Action onClosed;
    private List<GameObject> spawnedEntries = new();

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); Hide(); });
            AddHoverSound(closeButton);
        }
        gameObject.SetActive(false);
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
        if (button.gameObject.GetComponent<UIHoverSound>() == null)
        {
            button.gameObject.AddComponent<UIHoverSound>();
        }
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            // Only allow closing if there's actually a close button or if it's explicitly allowed.
            if (closeButton == null || closeButton.gameObject.activeSelf)
            {
                AudioSystem.Instance?.PlayButtonClick();
                Hide();
            }
        }
    }

    /// <summary>
    /// Shows the card selection overlay with the given eligible cards.
    /// </summary>
    public void Show(List<CardData> eligibleCards, string title, Action<CardData> onSelect, Action onClose = null, bool showCounts = true)
    {
        onCardSelected = onSelect;
        onClosed = onClose;
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
                cardImage.color = Color.white;
            }

            // Set optional count text (ensure raycastTarget is false so clicks reach button)
            TMP_Text countText = entry.GetComponentInChildren<TMP_Text>();
            if (countText != null)
            {
                countText.raycastTarget = false;
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
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => 
                {
                    AudioSystem.Instance?.PlayButtonClick();
                    OnEntryClicked(capturedCard);
                });
                AddHoverSound(btn);
            }
        }

        gameObject.SetActive(true);
    }

    private void OnEntryClicked(CardData card)
    {
        onCardSelected?.Invoke(card);
        gameObject.SetActive(false); // Hide UI without triggering onClosed
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onCardSelected = null;
        onClosed?.Invoke();
        onClosed = null;
    }
}
