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
    public int Vulnerable { get; private set; }
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
        Vulnerable = 0;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        healthText.text = $"{CurrentHealth}/{MaxHealth}";
        
        bool layoutChanged = false;

        if (blockText != null)
        {
            if (CurrentBlock > 0)
            {
                if (!blockText.gameObject.activeSelf) layoutChanged = true;
                blockText.gameObject.SetActive(true);
                blockText.text = $"BLK: {CurrentBlock}";
            }
            else
            {
                if (blockText.gameObject.activeSelf) layoutChanged = true;
                blockText.gameObject.SetActive(false);
            }
        }

        if (strengthText != null)
        {
            if (Strength > 0)
            {
                if (!strengthText.gameObject.activeSelf) layoutChanged = true;
                strengthText.gameObject.SetActive(true);
                strengthText.text = $"STR: {Strength}";
            }
            else
            {
                if (strengthText.gameObject.activeSelf) layoutChanged = true;
                strengthText.gameObject.SetActive(false);
            }
        }

        if (layoutChanged && healthText.transform.parent != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(healthText.transform.parent.GetComponent<RectTransform>());
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
        // Vulnerable: incoming damage is multiplied by 1.5x
        if (Vulnerable > 0)
        {
            damageAmount = Mathf.RoundToInt(damageAmount * 1.5f);
        }

        // Block absorbs damage first
        if (CurrentBlock > 0)
        {
            int blockedDamage = Mathf.Min(CurrentBlock, damageAmount);
            CurrentBlock -= blockedDamage;
            damageAmount -= blockedDamage;
        }

        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath();
        }

        // Shake only the sprite, not the UI text elements (hit visual effect)
        SpriteTransform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }

    protected virtual void OnDeath()
    {
        // Override in subclasses for death behavior
    }

    /// <summary>
    /// Applies Vulnerable stacks. Vulnerable targets take 50% more damage.
    /// Stacks decrease by 1 each turn.
    /// </summary>
    public void ApplyVulnerable(int stacks)
    {
        Vulnerable += stacks;
        UpdateHealthText();
    }

    /// <summary>
    /// Decrements Vulnerable by 1 at the start of each turn.
    /// </summary>
    public void TickVulnerable()
    {
        if (Vulnerable > 0)
        {
            Vulnerable--;
            UpdateHealthText();
        }
    }

    /// <summary>
    /// Removes all debuffs (Vulnerable, etc.) from this combatant.
    /// </summary>
    public void Cleanse()
    {
        Vulnerable = 0;
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