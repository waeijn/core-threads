using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages post-combat rewards in BattleScene.
/// On non-boss victory, presents the player with Duplicate / Purge / Skip choices.
/// Unlike Treasure nodes (which show the full deck for targeted selection),
/// combat rewards show a random subset of 3 cards.
/// </summary>
public class RewardSystem : Singleton<RewardSystem>
{
    [SerializeField] private ChoicePanelUI choicePanel;
    [SerializeField] private CardSelectionUI cardSelection;

    private const int RANDOM_OFFER_COUNT = 2;

    /// <summary>
    /// Called by GameOverSystem after a non-boss enemy victory.
    /// Shows the reward choice panel instead of immediately returning to map.
    /// </summary>
    public void ShowPostCombatReward()
    {
        if (choicePanel == null)
        {
            Debug.LogWarning("[RewardSystem] ChoicePanelUI not assigned — skipping reward.");
            ReturnToMap();
            return;
        }

        bool canPurge = GameState.CanPurge();

        choicePanel.Show(
            "COMBAT COMPLETE",
            "DUPLICATE CARD",
            canPurge ? "PURGE CARD" : "PURGE CARD (MIN DECK)",
            OnDuplicateSelected,
            OnPurgeSelected,
            OnSkipSelected,
            optionBEnabled: canPurge
        );
    }

    /// <summary>
    /// Returns a random subset of distinct cards from the player's deck.
    /// </summary>
    private List<CardData> GetRandomCardOffers()
    {
        List<CardData> allDistinct = GameState.GetDistinctCards();
        if (allDistinct.Count <= RANDOM_OFFER_COUNT) return allDistinct;

        // Shuffle and take N
        List<CardData> shuffled = allDistinct.OrderBy(_ => Random.value).ToList();
        return shuffled.Take(RANDOM_OFFER_COUNT).ToList();
    }

    private void OnDuplicateSelected()
    {
        if (cardSelection == null)
        {
            ReturnToMap();
            return;
        }

        List<CardData> cards = GetRandomCardOffers();
        cardSelection.Show(cards, "DUPLICATE — RANDOM SELECTION", (card) =>
        {
            GameState.DuplicateCard(card);
            ReturnToMap();
        }, showCounts: false);
    }

    private void OnPurgeSelected()
    {
        if (cardSelection == null)
        {
            ReturnToMap();
            return;
        }

        List<CardData> cards = GetRandomCardOffers();
        cardSelection.Show(cards, "PURGE — RANDOM SELECTION", (card) =>
        {
            GameState.PurgeCard(card);
            ReturnToMap();
        }, showCounts: false);
    }

    private void OnSkipSelected()
    {
        ReturnToMap();
    }

    private void ReturnToMap()
    {
        SceneManager.LoadScene("MapScene");
    }
}
