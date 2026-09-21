using System.Collections;
using UnityEngine;

public class CardView : MonoBehaviour
{
    // We only need one SpriteRenderer now for the whole card!
    [SerializeField] private SpriteRenderer cardGraphic;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;

    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private Vector3 dragOffset;

    public Card Card { get; private set; }

    public void Setup(Card card)
    {
        Card = card;

        // Simply apply the full 'Encrypt.sys.png' image to the sprite renderer
        if (cardGraphic != null)
        {
            cardGraphic.sprite = card.Image;
        }
    }

    void OnMouseEnter()
    {
        if (Card == null)
        {
            return;
        }

        if (!Interactions.Instance.PlayerCanHover()) return;
        
        AudioSystem.Instance?.PlayCardHover();
        
        wrapper.SetActive(false);
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + 4.5f, -1f);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        AudioSystem.Instance?.StopHoverSFX();
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        
        AudioSystem.Instance?.StopHoverSFX();
        
        Interactions.Instance.PlayerIsDragging = true;
        wrapper.SetActive(true);
        CardViewHoverSystem.Instance.Hide();

        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Calculate the exact distance between the mouse and the card's center
        Vector3 mousePos = MouseUtil.GetMousePositionInWorldSpace(-1);
        dragOffset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;

        // Add the offset to the position while dragging!
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1) + dragOffset;
    }

    void OnMouseUp()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;

        // 1. Pull the starting point 5 units back toward the camera 
        Vector3 safeRayOrigin = transform.position + new Vector3(0, 0, -5f);

        // 2. Combine our custom Raycast with the tutorial's Mana check!
        if (ManaSystem.Instance.HasEnoughMana(Card.Mana) &&
            Physics.Raycast(safeRayOrigin, Vector3.forward, out RaycastHit hit, 20f, dropLayer))
        {
            // Play the pop animation, then perform the card action
            StartCoroutine(PlayCardAnimation());
        }
        else
        {
            AudioSystem.Instance?.PlayError();
            transform.position = dragStartPosition;
            transform.rotation = dragStartRotation;
        }
        Interactions.Instance.PlayerIsDragging = false;
    }

    private IEnumerator PlayCardAnimation()
    {
        // Disable further interaction while animating
        Interactions.Instance.PlayerIsDragging = true;

        if (Card.PlaySound != null)
        {
            AudioSystem.Instance?.PlaySFX(Card.PlaySound);
        }

        Vector3 originalScale = transform.localScale;

        // Quick scale-up pop
        float elapsed = 0f;
        float popDuration = 0.08f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.25f, t);
            yield return null;
        }

        // Shrink to nothing
        elapsed = 0f;
        float shrinkDuration = 0.10f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(originalScale * 1.25f, Vector3.zero, t);
            yield return null;
        }

        // Perform the card action
        PlayCardGA playCardGA = new(Card);
        ActionSystem.Instance.Perform(playCardGA);

        Interactions.Instance.PlayerIsDragging = false;
    }
}