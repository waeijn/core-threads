using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RewardContext
{
    BasicEnemy,
    DatabaseTreasure,
    BossVictory
}

public static class CardRewardManager
{
    // Global cache to avoid constant Resources.LoadAll
    private static List<CardData> _allCardsCache;

    public static List<CardData> GetAllCards()
    {
        if (_allCardsCache == null || _allCardsCache.Count == 0)
        {
            _allCardsCache = Resources.LoadAll<CardData>("Cards").ToList();
        }
        return _allCardsCache;
    }

    public static List<CardData> GetDraftChoices(RewardContext context, int currentAct, int count = 3)
    {
        var allCards = GetAllCards();
        if (allCards.Count == 0) return new List<CardData>();

        List<CardData> choices = new List<CardData>();
        
        // Failsafe: if we don't have enough cards to provide unique choices, just return what we can
        if (allCards.Count <= count) return allCards.ToList();

        int attempts = 0;
        while (choices.Count < count && attempts < 100)
        {
            attempts++;
            
            CardActTier targetTier = RollTier(context, currentAct);
            CardRole targetRole = RollRole(targetTier);

            // Filter pool
            var validCards = allCards.Where(c => c.Tier == targetTier && c.Role == targetRole && !choices.Contains(c)).ToList();

            // Fallback 1: Ignore Role if none found
            if (validCards.Count == 0)
            {
                validCards = allCards.Where(c => c.Tier == targetTier && !choices.Contains(c)).ToList();
            }

            // Fallback 2: Ignore Tier if still none found
            if (validCards.Count == 0)
            {
                validCards = allCards.Where(c => !choices.Contains(c)).ToList();
            }

            if (validCards.Count > 0)
            {
                CardData selected = validCards[Random.Range(0, validCards.Count)];
                choices.Add(selected);
            }
        }

        return choices;
    }

    private static CardActTier RollTier(RewardContext context, int currentAct)
    {
        float roll = Random.value; // 0.0 to 1.0

        if (context == RewardContext.BossVictory)
        {
            // Boss drops guaranteed signature/rare (Higher tier, capped at Act 3)
            int tierInt = Mathf.Clamp(currentAct + 1, 1, 3);
            return (CardActTier)tierInt;
        }
        
        if (context == RewardContext.DatabaseTreasure)
        {
            // Database Draft guarantees higher tier (Act + 1)
            int tierInt = Mathf.Clamp(currentAct + 1, 1, 3);
            return (CardActTier)tierInt;
        }

        // Basic Enemy
        if (currentAct == 1)
        {
            return CardActTier.Act1; // 100% Act 1
        }
        else if (currentAct == 2)
        {
            // 70% Act 2, 30% Act 1
            return roll < 0.70f ? CardActTier.Act2 : CardActTier.Act1;
        }
        else // Act 3
        {
            // 60% Act 3, 30% Act 2, 10% Act 1
            if (roll < 0.60f) return CardActTier.Act3;
            if (roll < 0.90f) return CardActTier.Act2;
            return CardActTier.Act1;
        }
    }

    private static CardRole RollRole(CardActTier tier)
    {
        while (true)
        {
            float roll = Random.value * 100f; // 0 to 100
            CardRole selectedRole;

            if (roll < 40f) selectedRole = CardRole.Attack;          // 40%
            else if (roll < 75f) selectedRole = CardRole.Skill;      // 35%
            else if (roll < 90f) selectedRole = CardRole.Utility;    // 15%
            else if (roll < 97f) selectedRole = CardRole.BuffDebuff; // 7%
            else selectedRole = CardRole.Repair;                     // 3%

            // Restrict Buffs from Act 1
            if (tier == CardActTier.Act1 && selectedRole == CardRole.BuffDebuff)
            {
                continue; // Reroll
            }

            return selectedRole;
        }
    }
}
