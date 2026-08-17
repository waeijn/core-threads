using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    public TMP_Text healthText;
    public TMP_Text blockText;
    public TMP_Text strengthText;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    /// <summary>Exposes the Animator so subclasses (EnemyView) can swap controllers at runtime.</summary>
    protected Animator Animator => animator;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentBlock { get; private set; }
    public int Strength { get; private set; }
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
        Strength = 0;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        healthText.text = $"{CurrentHealth}/{MaxHealth}";
        
        if (blockText != null)
        {
            if (CurrentBlock > 0)
            {
                blockText.gameObject.SetActive(true);
                blockText.text = $"BLK: {CurrentBlock}";
            }
            else
            {
                blockText.gameObject.SetActive(false);
            }
        }

        if (strengthText != null)
        {
            if (Strength > 0)
            {
                strengthText.gameObject.SetActive(true);
                strengthText.text = $"STR: {Strength}";
            }
            else
            {
                strengthText.gameObject.SetActive(false);
            }
        }
    }

    public void GainStrength(int amount)
    {
        Strength += amount;
        UpdateHealthText();
    }

    public void GainBlock(int amount)
    {
        CurrentBlock += amount;
        if (animator != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == "Shield")
                {
                    animator.SetTrigger("Shield");
                    break;
                }
            }
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

    /// <summary>
    /// Sets current HP directly (used to restore HP from GameState between fights).
    /// </summary>
    public void SetCurrentHP(int hp)
    {
        CurrentHealth = Mathf.Clamp(hp, 0, MaxHealth);
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

        // Shake only the sprite, not the UI text elements (hit visual effect)
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
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == "Attack")
                {
                    animator.SetTrigger("Attack");
                    break;
                }
            }
        }
    }
}