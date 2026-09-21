using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single card thumbnail in the pile viewer grid.
/// Shows the card image as a UI Image.
/// </summary>
public class CardThumbnailUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    public void Setup(Card card)
    {
        if (cardImage != null && card.Image != null)
        {
            cardImage.sprite = card.Image;
        }

        AddHoverSound(gameObject);
    }

    private void AddHoverSound(GameObject obj)
    {
        var trigger = obj.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? obj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter || e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }
}
