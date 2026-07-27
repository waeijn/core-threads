using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;

    public List<Effect> Effects => data.Effects;
    public int Mana {  get; private set; }
    private readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
        Mana = cardData.Mana;
    }
}
