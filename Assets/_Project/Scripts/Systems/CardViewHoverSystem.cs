using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

    public void Show(Card card, Vector3 position)
    {
        // TRIPWIRE 1: Check if the Inspector reference is truly missing
        if (cardViewHover == null)
        {
            Debug.LogError("TRIPWIRE 1: cardViewHover is null! This ghost script is hiding on an object named: " + gameObject.name);
            return;
        }

        // TRIPWIRE 2: Check if the card you hovered over is blank
        if (card == null)
        {
            Debug.LogError("TRIPWIRE 2: The card data is null! Your mouse touched a card before it finished setting up.");
            return;
        }

        // Your original code
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position;
    }

    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }
}
