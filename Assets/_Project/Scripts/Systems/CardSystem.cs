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
    private bool retainHand = false;

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
        ActionSystem.AttachPerformer<CleanupCardGA>(CleanupCardPerformer);
        ActionSystem.AttachPerformer<RetainHandGA>(RetainHandPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.DetachPerformer<CleanupCardGA>();
        ActionSystem.DetachPerformer<RetainHandGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public void Setup(List<CardData> deckData)
    {
        retainHand = false;
        foreach(var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
        drawPile.Shuffle();
        AudioSystem.Instance?.PlayShuffleDeck();
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
            yield return RefillDeck();

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

        // Instead of adding to discardPile immediately, we wait until effects finish.
        // The card is now floating in "limbo".
        
        // Add Mana Cost (Pre Reaction)
        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);

        // Add Effects (Post Reaction)
        if (playCardGA.Card.Effects != null)
        {
            foreach (var effect in playCardGA.Card.Effects)
            {
                PerformEffectGA performEffectGA = new(effect);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
        }

        // Add Cleanup Action (Post Reaction - added last so it runs after effects)
        CleanupCardGA cleanupCardGA = new(playCardGA.Card, cardView);
        ActionSystem.Instance.AddReaction(cleanupCardGA);

        yield return null;
    }

    private IEnumerator CleanupCardPerformer(CleanupCardGA cleanupCardGA)
    {
        if (cleanupCardGA.Card.IsExhaust)
        {
            exhaustPile.Add(cleanupCardGA.Card);
            yield return ExhaustCard(cleanupCardGA.CardView);
        }
        else
        {
            discardPile.Add(cleanupCardGA.Card);
            yield return DiscardCard(cleanupCardGA.CardView);
        }

        OnPilesChanged?.Invoke();
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

    private IEnumerator RetainHandPerformer(RetainHandGA retainHandGA)
    {
        retainHand = true;
        yield return null;
    }

    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        if (retainHand)
        {
            retainHand = false;
        }
        else
        {
            DiscardAllCardsGA discardAllCardsGA = new();
            ActionSystem.Instance.AddReaction(discardAllCardsGA);
        }
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        // Reset player block at start of player turn
        HeroSystem.Instance.HeroView.ResetBlock();
        AudioSystem.Instance?.PlayStartTurn();
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }

    private IEnumerator DrawCards()
    {
        Card card = drawPile.Draw();
        hand.Add(card);

        // Spawn at draw pile position, invisible (scale 0)
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        cardView.transform.localScale = Vector3.zero;

        // Scale up from 0 → full size with a slight overshoot bounce
        cardView.transform.DOScale(Vector3.one * 1.1f, 0.10f)
            .OnComplete(() => cardView.transform.DOScale(Vector3.one, 0.06f));

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

    public void ShowHand()
    {
        if (handView != null)
        {
            handView.gameObject.SetActive(true);
        }
    }

    private IEnumerator RefillDeck()
    {
        if (discardPile.Count == 0) yield break;

        // Visual animation: spawn actual CardViews for up to 3 cards in the discard pile
        if (discardPilePoint != null && drawPilePoint != null)
        {
            int cardsToSpawn = Mathf.Min(3, discardPile.Count);
            for (int i = 0; i < cardsToSpawn; i++)
            {
                // Grab actual cards from the top of the discard pile
                Card cardToShow = discardPile[discardPile.Count - 1 - i];
                
                // Use CardViewCreator but override its scale and disable collider so it's purely visual
                CardView cardView = CardViewCreator.Instance.CreateCardView(cardToShow, discardPilePoint.position, discardPilePoint.rotation);
                
                // Immediately set scale to 0.8 so we override the creator's scale animation
                cardView.transform.DOKill(); 
                cardView.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                
                // Offset slightly to look like a stack
                cardView.transform.position += new Vector3(0, 0, -1f - i * 0.1f);
                
                // Disable interaction
                Collider2D col = cardView.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                // Rotate it
                cardView.transform.DORotate(new Vector3(0, 0, 180f), 0.3f, RotateMode.FastBeyond360);
                
                // Fly it
                Tween moveTween = cardView.transform.DOMove(drawPilePoint.position, 0.3f).SetEase(Ease.OutQuad);
                
                // Destroy after tween
                moveTween.OnComplete(() => Destroy(cardView.gameObject));

                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitForSeconds(0.1f); // Wait a bit for the last one to finish
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        drawPile.Shuffle();
        AudioSystem.Instance?.PlayShuffleDeck();
        OnPilesChanged?.Invoke();
    }

    private IEnumerator DiscardCard(CardView cardView)
    {
        // Fly to discard pile with a gentle arc rotation
        cardView.transform.DORotate(new Vector3(0, 0, -20f), 0.18f);
        cardView.transform.DOScale(Vector3.zero, 0.18f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.18f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
}
