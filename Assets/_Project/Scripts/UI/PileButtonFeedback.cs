using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to any pile button (Draw, Discard, Exhaust) to give it
/// a satisfying scale-pop on hover and on click.
/// No DOTween dependency — pure coroutines.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PileButtonFeedback : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale    = 1.12f;
    [SerializeField] private float hoverDuration = 0.10f;

    [Header("Click")]
    [SerializeField] private float clickScale    = 0.90f;
    [SerializeField] private float clickDuration = 0.07f;

    private RectTransform _rt;
    private Vector3       _originalScale;
    private Coroutine     _scaleRoutine;
    private bool          _isHovered;

    private void Awake()
    {
        _rt            = GetComponent<RectTransform>();
        _originalScale = _rt.localScale;
    }

    // ── Hover ──────────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _)
    {
        _isHovered = true;
        ScaleTo(_originalScale * hoverScale, hoverDuration);
    }

    public void OnPointerExit(PointerEventData _)
    {
        _isHovered = false;
        ScaleTo(_originalScale, hoverDuration);
    }

    // ── Click ──────────────────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData _)
    {
        ScaleTo(_originalScale * clickScale, clickDuration);
    }

    public void OnPointerUp(PointerEventData _)
    {
        // Return to hover scale if still hovering, otherwise back to normal
        Vector3 target = _isHovered ? _originalScale * hoverScale : _originalScale;
        ScaleTo(target, clickDuration);
    }

    // ── Scale Coroutine ────────────────────────────────────────────────────

    private void ScaleTo(Vector3 target, float duration)
    {
        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);
        _scaleRoutine = StartCoroutine(ScaleRoutine(_rt.localScale, target, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _rt.localScale = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }
        _rt.localScale = to;
    }
}
