public class CleanupCardGA : GameAction
{
    public Card Card { get; }
    public CardView CardView { get; }
    public CleanupCardGA(Card card, CardView cardView)
    {
        Card = card;
        CardView = cardView;
    }
}
