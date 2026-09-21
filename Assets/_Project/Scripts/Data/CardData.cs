using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Title { get; private set; }
    [field: SerializeField] public string Category { get; private set; }
    [field: SerializeField] public Color CardColor { get; private set; } = Color.white;
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Mana { get; private set; }
    [field: SerializeField] public bool IsExhaust { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public AudioClip PlaySound { get; private set; }
    [field: SerializeReference, SR] public List<Effect> Effects { get; set; }

    // ── Taxonomy & Drafting ───────────────────────────────────────────────
    [field: SerializeField] public CardActTier Tier { get; private set; } = CardActTier.Act1;
    [field: SerializeField] public CardRole Role { get; private set; } = CardRole.Attack;

    public string GetExtension()
    {
        return Tier switch
        {
            CardActTier.Act1 => Role switch
            {
                CardRole.Attack => ".exe",
                CardRole.Skill => ".sys",
                CardRole.Utility => ".bat",
                CardRole.Repair => ".patch",
                CardRole.BuffDebuff => ".log", // Restricted in Gen, but mapped just in case
                _ => ".dat"
            },
            CardActTier.Act2 => Role switch
            {
                CardRole.Attack => ".sh",
                CardRole.Skill => ".dll",
                CardRole.Utility => ".cmd",
                CardRole.Repair => ".cfg",
                CardRole.BuffDebuff => ".log",
                _ => ".dat"
            },
            CardActTier.Act3 => Role switch
            {
                CardRole.Attack => ".bin",
                CardRole.Skill => ".iso",
                CardRole.Utility => ".py",
                CardRole.Repair => ".db",
                CardRole.BuffDebuff => ".env",
                _ => ".dat"
            },
            _ => ".dat"
        };
    }

    public string GetFormattedTitle()
    {
        return $"{Title.ToUpper()}{GetExtension().ToUpper()}";
    }
}

public enum CardActTier
{
    Act1 = 1,
    Act2 = 2,
    Act3 = 3
}

public enum CardRole
{
    Attack,
    Skill,
    Utility,
    Repair,
    BuffDebuff
}

