using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages post-combat rewards in GameScene.
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

        choicePanel.Show(
            "COMBAT COMPLETE",
            "DRAFT NEW CARD",
            "",
            OnDraftSelected,
            null,
            OnSkipSelected,
            optionBEnabled: false
        );
    }

    public void ShowBossReward()
    {
        if (choicePanel == null || cardSelection == null)
        {
            Debug.LogWarning("[RewardSystem] UI not assigned — skipping reward.");
            ReturnToMap();
            return;
        }

        // --- PHASE 1: SIGNATURE CARD DRAFT ---
        // Guaranteed high-tier cards (Act + 1). Use currentAct before advancing it.
        List<CardData> draftPool = CardRewardManager.GetDraftChoices(RewardContext.BossVictory, GameState.CurrentAct);
        
        cardSelection.Show(
            draftPool, 
            "BOSS REWARD — SIGNATURE DRAFT", 
            onSelect: (card) =>
            {
                GameState.PlayerDeck.Add(card);
                StartPhase2HardwareExpansion();
            },
            onClose: () => 
            {
                // Player skipped the draft
                StartPhase2HardwareExpansion();
            },
            showCounts: false
        );
    }

    private void StartPhase2HardwareExpansion()
    {
        if (choicePanel == null)
        {
            StartPhase3SystemRestore();
            return;
        }

        choicePanel.Show(
            "CRITICAL SYSTEM FORK",
            "OVERCLOCK CORE\n<size=60%>(+1 Max Energy)</size>",
            "EXPANDED SECTOR\n<size=60%>(+25 Max HP & Purge 1)</size>",
            onA: () =>
            {
                // Option A: +1 Energy (capped at 4)
                GameState.PlayerMaxMana = Mathf.Min(GameState.PlayerMaxMana + 1, 4);
                StartPhase3SystemRestore();
            },
            onB: () =>
            {
                // Option B: +25 HP and Purge
                GameState.PlayerMaxHP += 25;
                
                // Show purge UI
                if (GameState.CanPurge())
                {
                    List<CardData> purgeCards = GameState.GetDistinctCards();
                    cardSelection.Show(
                        purgeCards, 
                        "PURGE CORRUPTED DATA", 
                        onSelect: (cardToPurge) =>
                        {
                            GameState.PurgeCard(cardToPurge);
                            StartPhase3SystemRestore();
                        },
                        onClose: () =>
                        {
                            StartPhase3SystemRestore();
                        },
                        showCounts: false
                    );
                }
                else
                {
                    StartPhase3SystemRestore();
                }
            },
            onSkipAction: null, // Force them to choose an upgrade!
            optionBEnabled: true
        );
    }

    private void StartPhase3SystemRestore()
    {
        // --- PHASE 3: SYSTEM RESTORE & ACT TRANSITION ---
        // 1. Full System Restore (100% Heal, including any new Max HP)
        GameState.PlayerCurrentHP = GameState.PlayerMaxHP;
        Debug.Log($"[RewardSystem] Boss sequence complete! Full heal applied. HP: {GameState.PlayerCurrentHP}/{GameState.PlayerMaxHP}. Mana: {GameState.PlayerMaxMana}");

        // 2. Advance Act state so when they return to map, it generates the next act
        if (GameState.CurrentAct >= 3)
        {
            // End of the game, transition to credits!
            SceneManager.LoadScene("CreditsScene");
        }
        else
        {
            GameState.CurrentAct++;
            GameState.GeneratedMap = null;
            GameState.SelectedNode = null;
            ReturnToMap();
        }
    }

    private void OnDraftSelected()
    {
        if (cardSelection == null)
        {
            ReturnToMap();
            return;
        }

        List<CardData> cards = CardRewardManager.GetDraftChoices(RewardContext.BasicEnemy, GameState.CurrentAct);
        cardSelection.Show(
            cards, 
            "DRAFT — SELECT A NEW CARD", 
            onSelect: (card) =>
            {
                GameState.PlayerDeck.Add(card);
                ReturnToMap();
            }, 
            onClose: () => { ReturnToMap(); },
            showCounts: false
        );
    }

    /// <summary>
    /// Returns a random subset of distinct cards from the player's deck.
    /// </summary>
    private List<CardData> GetRandomCardOffers()
    {
        List<CardData> allDistinct = GameState.GetDistinctCards();
        if (allDistinct.Count <= 3) return allDistinct;

        List<CardData> shuffled = allDistinct.OrderBy(_ => Random.value).ToList();
        return shuffled.Take(3).ToList();
    }

    private void OnPurgeSelected()
    {
        if (cardSelection == null)
        {
            ReturnToMap();
            return;
        }

        List<CardData> cards = GetRandomCardOffers();
        cardSelection.Show(
            cards, 
            "PURGE — RANDOM SELECTION", 
            onSelect: (card) =>
            {
                GameState.PurgeCard(card);
                ReturnToMap();
            }, 
            onClose: () => { ReturnToMap(); },
            showCounts: false
        );
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
