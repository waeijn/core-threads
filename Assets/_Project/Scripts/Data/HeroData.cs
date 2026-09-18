using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero")]
public class HeroData : ScriptableObject
{
    [field: SerializeField] public string HeroName { get; private set; } = "THREAD-04";
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public List<CardData> Deck { get; private set; }
    
    [Header("Audio")]
    [field: SerializeField] public AudioClip AttackSound { get; private set; }
    [field: SerializeField] public AudioClip DamageSound { get; private set; }
    [field: SerializeField] public AudioClip HealSound { get; private set; }
    [field: SerializeField] public AudioClip BuffSound { get; private set; }
    [field: SerializeField] public AudioClip DebuffSound { get; private set; }
    [field: SerializeField] public AudioClip CardDrawSound { get; private set; }
    [field: SerializeField] public AudioClip CardDiscardSound { get; private set; }
}
