using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the DrawPileUI or DiscardPileUI button.
/// Shows a card count badge and opens the PileViewerUI on click.
/// </summary>
public class PileCountUI : MonoBehaviour
{
    public enum PileType { Draw, Discard, Exhaust }

    [SerializeField] private PileType pileType;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private PileViewerUI pileViewerUI;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);

        // Subscribe to pile changes
        if (CardsSystem.Instance != null)
            CardsSystem.Instance.OnPilesChanged += UpdateCount;
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);

        if (CardsSystem.Instance != null)
            CardsSystem.Instance.OnPilesChanged -= UpdateCount;
    }

    private void Start()
    {
        // Subscribe again in Start in case CardsSystem wasn't ready in OnEnable
        if (CardsSystem.Instance != null)
        {
            CardsSystem.Instance.OnPilesChanged -= UpdateCount; // prevent double-sub
            CardsSystem.Instance.OnPilesChanged += UpdateCount;
        }
        UpdateCount();
    }

    private void UpdateCount()
    {
        if (countText == null) return;

        int count = pileType switch
        {
            PileType.Draw    => CardsSystem.Instance.DrawPileCount,
            PileType.Discard => CardsSystem.Instance.DiscardPileCount,
            PileType.Exhaust => CardsSystem.Instance.ExhaustPileCount,
            _ => 0
        };

        countText.text = count.ToString();
    }

    private void OnClick()
    {
        if (pileViewerUI == null) return;

        // Don't allow opening the viewer during animations
        if (ActionSystem.Instance.IsPerforming) return;

        // If the viewer is already open, check if it's showing THIS pile
        if (pileViewerUI.IsOpen)
        {
            // Match the enums correctly (they are defined in two different places)
            bool isSamePile = false;
            if (pileType == PileType.Draw && pileViewerUI.CurrentPile == PileViewerUI.PileType.Draw) isSamePile = true;
            if (pileType == PileType.Discard && pileViewerUI.CurrentPile == PileViewerUI.PileType.Discard) isSamePile = true;
            if (pileType == PileType.Exhaust && pileViewerUI.CurrentPile == PileViewerUI.PileType.Exhaust) isSamePile = true;

            // If it is, close it
            if (isSamePile)
            {
                pileViewerUI.Hide();
                return;
            }
            // Otherwise, it will just overwrite and show the new pile (which is what we want!)
        }

        if (pileType == PileType.Draw)
        {
            pileViewerUI.ShowDrawPile();
        }
        else if (pileType == PileType.Discard)
        {
            pileViewerUI.ShowDiscardPile();
        }
        else if (pileType == PileType.Exhaust)
        {
            pileViewerUI.ShowExhaustPile();
        }
    }
}
