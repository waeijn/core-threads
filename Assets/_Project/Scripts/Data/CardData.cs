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
    [field: SerializeReference, SR] public List<Effect> Effects { get; set; }

    [Header("Audio")]
    [field: SerializeField] public AudioClip PlaySound { get; private set; }
}

