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
    }
}
