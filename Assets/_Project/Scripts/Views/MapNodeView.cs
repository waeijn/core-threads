using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI representation of a single map node. Displays the correct icon sprite,
/// handles Slay the Spire style locked/unlocked/visited visual states and animations.
/// </summary>
public class MapNodeView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    [SerializeField] private GameObject visitedOverlay;   // Checkmark or faded overlay
    [SerializeField] private GameObject lockedOverlay;    // Dark overlay
    [SerializeField] private GameObject encircledRing;    // Visited highlight ring

    public MapNodeRuntime RuntimeNode { get; private set; }

    private MapConfig _config;
    private Action<MapNodeView> _onClicked;
    private Tween _pulseTween;
    private Vector3 _baseScale = Vector3.one;

    private void OnDestroy()
    {
        _pulseTween?.Kill();
    }

    // ── Setup ──────────────────────────────────────────────────────────────

    public void Setup(MapNodeRuntime node, MapConfig config, Action<MapNodeView> onClicked)
    {
        RuntimeNode = node;
        _config     = config;
        _onClicked  = onClicked;

        // Base scale (Boss nodes are 1.5x)
        _baseScale = (node.Type == NodeType.Boss) ? Vector3.one * config.bossScale : Vector3.one;
        transform.localScale = _baseScale;

        // Assign sprite
        if (config.nodeSprites != null && node.SpriteIndex < config.nodeSprites.Length)
            iconImage.sprite = config.nodeSprites[node.SpriteIndex];

        // Wire button
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => 
        {
            AudioSystem.Instance?.PlayButtonClick();
            _onClicked?.Invoke(this);
        });

        AddHoverSound(button);

        RefreshVisuals();
    }

    private void AddHoverSound(Button btn)
    {
        if (btn == null) return;
        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter);
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { if (btn.interactable) AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    // ── State ──────────────────────────────────────────────────────────────

    /// <summary>Call whenever node state may have changed (after returning from combat).</summary>
    public void RefreshVisuals()
    {
        if (RuntimeNode == null) return;

        bool visited  = RuntimeNode.IsVisited;
        bool unlocked = RuntimeNode.IsUnlocked && !RuntimeNode.IsLocked && !visited;
        bool locked   = RuntimeNode.IsLocked || (!RuntimeNode.IsUnlocked && !visited);

        // Button interactability
        button.interactable = unlocked;

        // Stop any active pulse tween first
        _pulseTween?.Kill();
        _pulseTween = null;

        // Visual states: Slay the Spire style
        if (visited)
        {
            // Visited: Natural color, encircled ring active, slightly enlarged
            iconImage.color = _config.visitedColor;
            transform.localScale = _baseScale * 1.15f;

            if (encircledRing != null)   encircledRing.SetActive(true);
            if (visitedOverlay != null)  visitedOverlay.SetActive(false);
            if (lockedOverlay != null)   lockedOverlay.SetActive(false);
        }
        else if (unlocked)
        {
            // Unlocked (Next available choices): Full natural color, pulsing bigger and smaller in loop
            iconImage.color = _config.unlockedColor;
            transform.localScale = _baseScale;

            if (encircledRing != null)   encircledRing.SetActive(false);
            if (visitedOverlay != null)  visitedOverlay.SetActive(false);
            if (lockedOverlay != null)   lockedOverlay.SetActive(false);

            // Looping pulse animation (Yoyo 1.0x ↔ 1.18x)
            _pulseTween = transform.DOScale(_baseScale * 1.18f, 0.55f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            // Locked / Unvisited: Bright visible color (85% opacity), static base scale
            iconImage.color = _config.lockedColor;
            transform.localScale = _baseScale;

            if (encircledRing != null)   encircledRing.SetActive(false);
            if (visitedOverlay != null)  visitedOverlay.SetActive(false);
            if (lockedOverlay != null)   lockedOverlay.SetActive(false);
        }
    }
}

