using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float maxSpread = 1f;
    [SerializeField] private float maxCardSpacing = 1f / 6f;

    private readonly List<CardView> cards = new();
    private Coroutine layoutRoutine;

    public Coroutine AddCard(CardView cardView)
    {
        // Parent strictly to the Spline so they share the exact same coordinate system
        cardView.transform.SetParent(splineContainer.transform, true);

        cards.Add(cardView);

        if (layoutRoutine != null)
        {
            StopCoroutine(layoutRoutine);
        }

        // We start the animation and save the Coroutine to our variable
        layoutRoutine = StartCoroutine(UpdateCardPositions(0.15f));

        // THIS IS THE NEW LINE: We hand that Coroutine back to whoever called AddCard!
        return layoutRoutine;
    }

    public CardView RemoveCard(Card card)
    {
        CardView cardView = GetCardView(card);
        if (cardView == null) return null;
        cards.Remove(cardView);
        StartCoroutine(UpdateCardPositions(0.15f));
        return cardView;
    }

    private CardView GetCardView(Card card)
    {
        return cards.Where(cardView => cardView.Card == card).FirstOrDefault();
    }

    private IEnumerator UpdateCardPositions(float duration)
    {
        if (cards.Count == 0) yield break;

        float cardSpacing = cards.Count > 1
            ? Mathf.Min(maxCardSpacing, maxSpread / (cards.Count - 1))
            : 0f;

        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;

        Spline spline = splineContainer.Spline;
        float totalLength = spline.GetLength();

        for (int i = 0; i < cards.Count; i++)
        {
            float normalizedIndex = Mathf.Clamp01(firstCardPosition + i * cardSpacing);
            float targetDistance = normalizedIndex * totalLength;

            // Convert distance -> correct parameter t, evenly spaced by actual arc length
            SplineUtility.GetPointAtLinearDistance(spline, 0f, targetDistance, out float t);

            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 localTangent = spline.EvaluateTangent(t);
            float zRot = Mathf.Atan2(localTangent.y, localTangent.x) * Mathf.Rad2Deg;

            Debug.Log($"Card {i}: normalizedIndex={normalizedIndex}, t={t}, localPos={localPos}");

            cards[i].transform.DOKill(true);

            // Animate strictly in Local Space
            cards[i].transform.DOLocalMove(localPos + new Vector3(0, 0, -0.01f * i), duration);
            cards[i].transform.DOLocalRotate(new Vector3(0, 0, zRot), duration);
        }

        yield return new WaitForSeconds(duration);
    }
}