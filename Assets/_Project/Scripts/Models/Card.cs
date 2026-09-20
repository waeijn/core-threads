using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public string Title => string.IsNullOrEmpty(data.Title) ? data.name : data.Title;
    public string Category => data.Category;
    public Color CardColor => data.CardColor;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public bool IsExhaust => data.IsExhaust;

    public List<Effect> Effects => data.Effects ?? new List<Effect>();
    public int Mana { get; private set; }
    private readonly CardData data;

    public Card(CardData cardData)
    {
        data = cardData;
        Mana = cardData.Mana;
    }
}

