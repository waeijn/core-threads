using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentBlock { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

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
        if (animator != null)
        {
            animator.SetTrigger("Shield");
        }
        UpdateHealthText();
    }

    /// <summary>
    /// Slay the Spire mechanic: Block resets to 0 at the start of each turn.
    /// </summary>
    public void ResetBlock()
    {
        CurrentBlock = 0;
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

        // Play take damage animation
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Shake only the sprite, not the UI text elements
        SpriteTransform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }

    /// <summary>
    /// Triggers the attack animation on this combatant's animator.
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
}