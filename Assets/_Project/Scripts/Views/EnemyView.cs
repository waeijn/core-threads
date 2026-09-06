using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text intentText;
    [SerializeField] private SpriteRenderer intentIcon;
    [SerializeField] private TMP_Text secondaryIntentText;
    [SerializeField] private SpriteRenderer secondaryIntentIcon;
    [SerializeField] private Sprite[] intentSprites;

    public int AttackPower { get; set; }
    public int BlockPower { get; set; }
    public int BuffAmount { get; set; }
    public EnemyIntent CurrentIntent { get; private set; }

    private EnemyData enemyData;

    public void Setup(EnemyData data)
    {
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            var eUI = canvas.transform.Find("HealthUIContainer/EnemyHealthUI") ?? canvas.transform.Find("EnemyHealthUI");
            if (eUI != null)
            {
                var hpTr  = eUI.Find("EnemyHealthText");
                var blkTr = eUI.Find("EnemyBlockText");
                var strTr = eUI.Find("EnemyStrengthText");
                if (hpTr != null)  healthText = hpTr.GetComponent<TMP_Text>();
                if (blkTr != null) blockText  = blkTr.GetComponent<TMP_Text>();
                if (strTr != null) strengthText = strTr.GetComponent<TMP_Text>();
            }
        }

        if (healthText == null)
        {
            var hpObj = GameObject.Find("EnemyHealthText");
            if (hpObj != null) 
            {
                healthText = hpObj.GetComponent<TMP_Text>();
                if (blockText == null)
                {
                    var blkTr = hpObj.transform.parent.Find("EnemyBlockText");
                    if (blkTr != null) blockText = blkTr.GetComponent<TMP_Text>();
                }
                if (strengthText == null)
                {
                    var strTr = hpObj.transform.parent.Find("EnemyStrengthText");
                    if (strTr != null) strengthText = strTr.GetComponent<TMP_Text>();
                }
            }
        }

        enemyData = data;
        AttackPower = data.AttackPower;
        BlockPower = data.BlockPower;
        BuffAmount = data.BuffAmount;
        UpdateAttackText();
        SetupBase(data.Health, data.Image);

        // Assign the correct AnimatorController for this enemy type
        if (data.AnimatorController != null && Animator != null)
        {
            Animator.runtimeAnimatorController = data.AnimatorController;
        }

        // Apply custom scale for this enemy type
        if (SpriteTransform != null && data.Scale != Vector3.zero)
        {
            SpriteTransform.localScale = data.Scale;
        }

        RollNextIntent();
    }

    /// <summary>
    /// Picks the next intent using the health-threshold FSM.
    /// Evaluates FSM states top-to-bottom; first matching state's move pool is used.
    /// Falls back to the default MovePool if no FSM state matches.
    /// </summary>
    public void RollNextIntent()
    {
        List<EnemyMove> moves = GetCurrentMovePool();

        if (moves == null || moves.Count == 0)
        {
            CurrentIntent = EnemyIntent.Attack;
            UpdateIntentDisplay();
            return;
        }

        // Weighted random selection
        int totalWeight = 0;
        foreach (var move in moves)
            totalWeight += move.Weight;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var move in moves)
        {
            cumulative += move.Weight;
            if (roll < cumulative)
            {
                CurrentIntent = move.Intent;
                UpdateIntentDisplay();
                return;
            }
        }

        // Fallback
        CurrentIntent = EnemyIntent.Attack;
        UpdateIntentDisplay();
    }

    /// <summary>
    /// Returns the move pool for the current FSM state based on HP%.
    /// Falls back to the default MovePool if no FSM state matches.
    /// </summary>
    private List<EnemyMove> GetCurrentMovePool()
    {
        if (enemyData.FSMStates != null && enemyData.FSMStates.Count > 0)
        {
            float hpPercent = (MaxHealth > 0) ? (CurrentHealth * 100f / MaxHealth) : 0f;

            foreach (var state in enemyData.FSMStates)
            {
                if (hpPercent >= state.MinHPPercent && hpPercent <= state.MaxHPPercent)
                {
                    Debug.Log($"<color=orange>FSM:</color> {enemyData.name} in state '{state.StateName}' (HP: {hpPercent:F0}%)");
                    return state.Moves;
                }
            }
        }

        // No FSM match — use default move pool
        return enemyData.MovePool;
    }

    /// <summary>
    /// Returns the value associated with the current intent's move,
    /// or falls back to the enemy's default stat if no override is set.
    /// </summary>
    public int GetIntentValue()
    {
        List<EnemyMove> moves = GetCurrentMovePool();
        if (moves != null)
        {
            foreach (var move in moves)
            {
                if (move.Intent == CurrentIntent && move.OverrideValue > 0)
                    return move.OverrideValue;
            }
        }

        return CurrentIntent switch
        {
            EnemyIntent.Attack => AttackPower,
            EnemyIntent.Defend => BlockPower,
            EnemyIntent.Buff => BuffAmount,
            _ => AttackPower
        };
    }

    public string GetIntentTooltip(bool isSecondary)
    {
        if (CurrentIntent == EnemyIntent.AttackAndDefend)
        {
            if (isSecondary)
            {
                return $"<color=#4488FF>Defend</color>\nThis enemy intends to gain <color=#4488FF>{BlockPower}</color> Block.";
            }
            else
            {
                return $"<color=#FF4444>Attack</color>\nThis enemy intends to deal <color=#FF4444>{AttackPower}</color> damage.";
            }
        }
        else
        {
            switch (CurrentIntent)
            {
                case EnemyIntent.Attack:
                    return $"<color=#FF4444>Attack</color>\nThis enemy intends to deal <color=#FF4444>{AttackPower}</color> damage.";
                case EnemyIntent.Defend:
                    return $"<color=#4488FF>Defend</color>\nThis enemy intends to gain <color=#4488FF>{BlockPower}</color> Block.";
                case EnemyIntent.Buff:
                    return $"<color=#44FF44>Buff</color>\nThis enemy intends to apply a buff.";
                default:
                    return "Unknown Intent";
            }
        }
    }

    private void UpdateAttackText()
    {
        attackText.text = "ATK: " + AttackPower;
    }

    public void HideIntent()
    {
        if (intentIcon != null) intentIcon.gameObject.SetActive(false);
        if (intentText != null) intentText.gameObject.SetActive(false);
        if (secondaryIntentIcon != null) secondaryIntentIcon.gameObject.SetActive(false);
        if (secondaryIntentText != null) secondaryIntentText.gameObject.SetActive(false);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        HideIntent();
        
        if (healthText != null) healthText.gameObject.SetActive(false);
        if (blockText != null) blockText.gameObject.SetActive(false);
        if (strengthText != null) strengthText.gameObject.SetActive(false);
    }

    private void UpdateIntentDisplay()
    {
        HideIntent();

        if (intentIcon != null && intentSprites != null && intentSprites.Length >= 4)
        {
            intentIcon.gameObject.SetActive(true);
            if (intentText != null) intentText.gameObject.SetActive(true);

            float rightEdgeX = 2.0f; // Pushed further right
            float yPosPrimary = 7.5f;
            float yPosSecondary = 6.5f; // Stacked vertically below
            float textOffsetX = -0.45f;  // Value closer to the left of the icon

            switch (CurrentIntent)
            {
                case EnemyIntent.Attack:
                    intentIcon.sprite = intentSprites[0];
                    intentIcon.transform.position = new Vector3(rightEdgeX, yPosPrimary, 0f);
                    if (intentText != null) {
                        intentText.text = $"{AttackPower}";
                        intentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosPrimary, 0f);
                    }
                    break;
                case EnemyIntent.Defend:
                    intentIcon.sprite = intentSprites[1];
                    intentIcon.transform.position = new Vector3(rightEdgeX, yPosPrimary, 0f);
                    if (intentText != null) {
                        intentText.text = $"{BlockPower}";
                        intentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosPrimary, 0f);
                    }
                    break;
                case EnemyIntent.Buff:
                    intentIcon.sprite = intentSprites[2];
                    intentIcon.transform.position = new Vector3(rightEdgeX, yPosPrimary, 0f);
                    if (intentText != null) {
                        intentText.text = $"{BuffAmount}";
                        intentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosPrimary, 0f);
                    }
                    break;
                case EnemyIntent.AttackAndDefend:
                    intentIcon.sprite = intentSprites[0];
                    intentIcon.transform.position = new Vector3(rightEdgeX, yPosPrimary, 0f);
                    if (intentText != null) {
                        intentText.text = $"{AttackPower}";
                        intentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosPrimary, 0f);
                    }
                    
                    if (secondaryIntentIcon != null && secondaryIntentText != null)
                    {
                        secondaryIntentIcon.gameObject.SetActive(true);
                        secondaryIntentText.gameObject.SetActive(true);
                        secondaryIntentIcon.sprite = intentSprites[1];
                        secondaryIntentText.text = $"{BlockPower}";
                        
                        // Stack secondary intent vertically
                        secondaryIntentIcon.transform.position = new Vector3(rightEdgeX, yPosSecondary, 0f);
                        secondaryIntentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosSecondary, 0f);
                    }
                    break;
            }
        }
        else
        {
            // Fallback to text if icons aren't assigned
            if (intentText != null)
            {
                intentText.gameObject.SetActive(true);
                switch (CurrentIntent)
                {
                    case EnemyIntent.Attack:
                        intentText.text = $"<color=#FF4444>ATK {AttackPower}</color>";
                        break;
                    case EnemyIntent.Defend:
                        intentText.text = $"<color=#4488FF>DEF {BlockPower}</color>";
                        break;
                    case EnemyIntent.AttackAndDefend:
                        intentText.text = $"<color=#FFAA00>ATK {AttackPower} + DEF {BlockPower}</color>";
                        break;
                    case EnemyIntent.Buff:
                        intentText.text = $"<color=#44FF44>BUFF +{BuffAmount}</color>";
                        break;
                }
            }
        }
    }
}