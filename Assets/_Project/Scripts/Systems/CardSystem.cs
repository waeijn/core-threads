using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsSystem : Singleton<CardsSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private Transform exhaustPilePoint;
    private const int MAX_HAND_SIZE = 10;
    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> exhaustPile = new();
    private readonly List<Card> hand = new();

    // --- Pile data exposed for UI ---
    public IReadOnlyList<Card> DrawPileCards => drawPile;
    public IReadOnlyList<Card> DiscardPileCards => discardPile;
    public IReadOnlyList<Card> ExhaustPileCards => exhaustPile;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int ExhaustPileCount => exhaustPile.Count;

    /// <summary>
    /// Fired whenever the draw, discard, or exhaust pile changes.
    /// PileCountUI subscribes to this to update the count badges.
    /// </summary>
    public event Action OnPilesChanged;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public void Setup(List<CardData> deckData)
    {
        foreach(var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
        drawPile.Shuffle();
        OnPilesChanged?.Invoke();
    }
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        Debug.Log($"<color=cyan>1. PERFORMER TRIGGERED:</color> The system asked to draw {drawCardsGA.Amount} cards.");
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;

        for (int i = 0; i < actualAmount; i++)
        {
            if (hand.Count >= MAX_HAND_SIZE) yield break;
            yield return DrawCards();
        }

        if (notDrawnAmount > 0)
        {
            RefillDeck();

            // FIX 2: Recalculate how many we can ACTUALLY draw after refilling, 
            // just in case both the deck and discard pile were completely empty!
            int amountAfterRefill = Mathf.Min(notDrawnAmount, drawPile.Count);

            for (int i = 0; i < amountAfterRefill; i++)
            {
                if (hand.Count >= MAX_HAND_SIZE) yield break;
                yield return DrawCards();
            }
        }
    }

    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach(var card in hand)
        {
            discardPile.Add(card);
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
        OnPilesChanged?.Invoke();
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        hand.Remove(playCardGA.Card);

        CardView cardView = handView.RemoveCard(playCardGA.Card);

        if (playCardGA.Card.IsExhaust)
        {
            exhaustPile.Add(playCardGA.Card);
            yield return ExhaustCard(cardView);
        }
        else
        {
            discardPile.Add(playCardGA.Card);
            yield return DiscardCard(cardView);
        }

        OnPilesChanged?.Invoke();

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);

        foreach (var effect in playCardGA.Card.Effects)
        {
            PerformEffectGA performEffectGA = new(effect);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
    }

    private IEnumerator ExhaustCard(CardView cardView)
    {
        // Fade, rotate, and fly toward exhaustPilePoint
        Vector3 targetPos = exhaustPilePoint != null ? exhaustPilePoint.position : (discardPilePoint != null ? discardPilePoint.position : Vector3.zero);
        cardView.transform.DOScale(Vector3.zero, 0.25f);
        cardView.transform.DORotate(new Vector3(0, 0, 180), 0.25f);
        Tween tween = cardView.transform.DOMove(targetPos, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }

    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        // Reset player block at start of player turn
        HeroSystem.Instance.HeroView.ResetBlock();
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }

    private IEnumerator DrawCards()
    {
        Debug.Log("<color=yellow>2. CARD SPAWNED:</color> Instantiating one visual card prefab.");
        Card card = drawPile.Draw();
        Debug.Log($"<color=green>SUCCESS:</color> The system just drew the card image named: {card.Image.name}");
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        OnPilesChanged?.Invoke();
        yield return handView.AddCard(cardView);
    }

    public void HideHand()
    {
        if (handView != null)
        {
            handView.gameObject.SetActive(false);
        }
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        drawPile.Shuffle();
        OnPilesChanged?.Invoke();
    }

    private IEnumerator DiscardCard(CardView cardView)
    {
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
}
