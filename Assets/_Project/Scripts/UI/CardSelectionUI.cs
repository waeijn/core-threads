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
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter || e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            // Only allow closing if there's actually a close button or if it's explicitly allowed.
            // Assuming if closeButton is assigned and active, we can close. 
            // If closeButton is null, we can still close since many drafts allow skipping.
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
    /// <param name="eligibleCards">Distinct cards the player can pick from.</param>
    /// <param name="title">Header text (e.g. "SELECT A CARD TO DUPLICATE").</param>
    /// <param name="onSelect">Callback invoked with the selected CardData.</param>
    /// <param name="onClose">Callback invoked when the window is closed/skipped.</param>
    /// <param name="showCounts">If true, shows how many copies of each card are in the deck.</param>
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

            // Use the prefab's native size instead of squashing it to 160x224 for drafts
            RectTransform entryRT = entry.GetComponent<RectTransform>();
            if (entryRT != null)
            {
                // We no longer force 160x224 here because the user wants large cards for drafting
                // If it needs to be small in certain modes, we should pass a parameter.
                // For now, let it keep the prefab's default size which is larger and readable.
            }

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
