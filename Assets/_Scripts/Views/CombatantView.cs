using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentBlock { get; private set; }

    /// <summary>
    /// The transform of the sprite child object. Use this for movement/shake
    /// animations so that UI elements (HP, ATK text) stay in place.
    /// </summary>
    public Transform SpriteTransform => spriteRenderer.transform;

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        CurrentBlock = 0;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (CurrentBlock > 0)
        {
            healthText.text = $"{CurrentHealth}/{MaxHealth} | BLK: {CurrentBlock}";
        }
        else
        {
            healthText.text = $"{CurrentHealth}/{MaxHealth}";
        }
    }

    public void GainBlock(int amount)
    {
        CurrentBlock += amount;
        UpdateHealthText();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        UpdateHealthText();
    }

    public void Damage(int damageAmount)
    {
        // Block absorbs damage first
        if (CurrentBlock > 0)
        {
            int blockedDamage = Mathf.Min(CurrentBlock, damageAmount);
            CurrentBlock -= blockedDamage;
            damageAmount -= blockedDamage;
        }

        CurrentHealth -= damageAmount;
        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
        }

        // Shake only the sprite, not the UI text elements
        SpriteTransform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }
}