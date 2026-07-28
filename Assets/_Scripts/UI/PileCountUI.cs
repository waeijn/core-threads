using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the DrawPileUI or DiscardPileUI button.
/// Shows a card count badge and opens the PileViewerUI on click.
/// </summary>
public class PileCountUI : MonoBehaviour
{
    public enum PileType { Draw, Discard }

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

        int count = pileType == PileType.Draw
            ? CardsSystem.Instance.DrawPileCount
            : CardsSystem.Instance.DiscardPileCount;

        countText.text = count.ToString();
    }

    private void OnClick()
    {
        if (pileViewerUI == null) return;

        // Don't allow opening the viewer during animations
        if (ActionSystem.Instance.IsPerforming) return;

        if (pileType == PileType.Draw)
        {
            pileViewerUI.ShowDrawPile();
        }
        else
        {
            pileViewerUI.ShowDiscardPile();
        }
    }
}
