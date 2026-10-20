using System.Collections.Generic;
using System.Text;
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

    // ── Enemy AI: Internal Energy & Skill Deck State ────────────────────────
    public int MaxEnergy { get; private set; } = 3;
    public int CurrentEnergy { get; private set; } = 3;
    public int SkillDrawCount { get; private set; } = 3;

    private readonly List<EnemyMove> _skillDrawPile = new();
    private readonly List<EnemyMove> _skillHand = new();
    private readonly List<EnemyMove> _skillDiscardPile = new();
    private List<EnemyMove> _activeSourcePool;
    private EnemyIntent? _lastPrimaryIntent;

    /// <summary>
    /// The sequence of skills selected by the Enemy AI from its drawn hand for this turn.
    /// </summary>
    public List<EnemyMove> PlannedMoves { get; private set; } = new();

    private EnemyData enemyData;

    public void Setup(EnemyData data)
    {
        Transform eUI = null;
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            eUI = c.transform.Find("HealthUIContainer/EnemyHealthUI") ?? c.transform.Find("EnemyHealthUI");
            if (eUI != null) break;
        }

        if (eUI != null)
        {
            var hpTr  = eUI.Find("EnemyHealthText");
            var blkTr = eUI.Find("EnemyBlockText");
            var strTr = eUI.Find("EnemyStrengthText");
            if (hpTr != null)  healthText = hpTr.GetComponent<TMP_Text>();
            if (blkTr != null) blockText  = blkTr.GetComponent<TMP_Text>();
            if (strTr != null) strengthText = strTr.GetComponent<TMP_Text>();
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
        MaxEnergy = data.GetEffectiveMaxEnergy();
        SkillDrawCount = data.GetEffectiveSkillDrawCount();
        CurrentEnergy = MaxEnergy;

        _skillDrawPile.Clear();
        _skillHand.Clear();
        _skillDiscardPile.Clear();
        _activeSourcePool = null;
        _lastPrimaryIntent = null;

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
    /// Replenishes internal energy, draws a hand of skills from the enemy's internal skill deck,
    /// and uses Utility AI self-decision scoring to select the optimal skill(s) within energy budget.
    /// </summary>
    public void RollNextIntent()
    {
        PlannedMoves.Clear();
        MaxEnergy = enemyData != null ? enemyData.GetEffectiveMaxEnergy() : 3;
        SkillDrawCount = enemyData != null ? enemyData.GetEffectiveSkillDrawCount() : 3;
        CurrentEnergy = MaxEnergy;

        List<EnemyMove> activePool = GetCurrentMovePool();

        // Fallback if no moves configured on asset
        if (activePool == null || activePool.Count == 0)
        {
            var fallbackMove = new EnemyMove
            {
                SkillName = "Strike Protocol",
                Intent = EnemyIntent.Attack,
                Weight = 1,
                EnergyCost = 2,
                OverrideValue = 0
            };
            PlannedMoves.Add(fallbackMove);
            CurrentEnergy = Mathf.Max(0, CurrentEnergy - fallbackMove.GetEffectiveEnergyCost());
            CurrentIntent = EnemyIntent.Attack;
            UpdateIntentDisplay();
            return;
        }

        // If FSM state changed (or first turn), rebuild internal skill deck for the active state
        if (!ReferenceEquals(_activeSourcePool, activePool))
        {
            _activeSourcePool = activePool;
            RebuildInternalSkillDeck(activePool);
        }

        // 1. Discard leftover hand from previous turn & draw a new internal hand of skills
        DrawInternalSkills(SkillDrawCount);

        // 2. Self-Decision Utility AI: evaluate drawn skills and pick optimal move(s) within CurrentEnergy
        SelectOptimalSkillsFromHand();

        // 3. Synchronize primary CurrentIntent and update visual display (hidden in Hardcore Mode)
        CurrentIntent = PlannedMoves.Count > 0 ? PlannedMoves[0].Intent : EnemyIntent.Attack;
        _lastPrimaryIntent = CurrentIntent;
        UpdateIntentDisplay();
    }

    /// <summary>
    /// Builds and shuffles the enemy's internal skill draw pile based on the active move pool weights.
    /// Ensures a minimum deck size so drawing and reshuffling behave like a true card deck.
    /// </summary>
    private void RebuildInternalSkillDeck(List<EnemyMove> pool)
    {
        _skillDrawPile.Clear();
        _skillHand.Clear();
        _skillDiscardPile.Clear();

        foreach (var move in pool)
        {
            int copies = Mathf.Clamp(move.Weight, 1, 4);
            for (int i = 0; i < copies; i++)
            {
                _skillDrawPile.Add(move);
            }
        }

        // Ensure at least 6 skill cards in the internal deck so multi-card hands & reshuffles work smoothly
        int safety = 0;
        while (_skillDrawPile.Count < 6 && pool.Count > 0 && safety < 12)
        {
            foreach (var move in pool)
            {
                _skillDrawPile.Add(move);
                if (_skillDrawPile.Count >= 6) break;
            }
            safety++;
        }

        ShuffleSkillList(_skillDrawPile);
    }

    /// <summary>
    /// Discards any remaining skills in hand and draws <paramref name="count"/> skills from the internal draw pile,
    /// reshuffling the internal discard pile when the draw pile runs out.
    /// </summary>
    private void DrawInternalSkills(int count)
    {
        if (_skillHand.Count > 0)
        {
            _skillDiscardPile.AddRange(_skillHand);
            _skillHand.Clear();
        }

        for (int i = 0; i < count; i++)
        {
            if (_skillDrawPile.Count == 0)
            {
                if (_skillDiscardPile.Count == 0) break;
                _skillDrawPile.AddRange(_skillDiscardPile);
                _skillDiscardPile.Clear();
                ShuffleSkillList(_skillDrawPile);
            }

            int lastIdx = _skillDrawPile.Count - 1;
            EnemyMove drawn = _skillDrawPile[lastIdx];
            _skillDrawPile.RemoveAt(lastIdx);
            _skillHand.Add(drawn);
        }
    }

    /// <summary>
    /// Evaluates the utility score of each drawn skill in <see cref="_skillHand"/> based on combat state
    /// and selects up to 2 skills (primary + secondary intent) within <see cref="CurrentEnergy"/>.
    /// </summary>
    private void SelectOptimalSkillsFromHand()
    {
        if (_skillHand.Count == 0) return;

        StringBuilder logBuilder = new StringBuilder();
        string enemyLabel = enemyData != null ? enemyData.name : gameObject.name;
        logBuilder.Append($"<color=#00E5FF>[Enemy AI]</color> <b>{enemyLabel}</b> | Energy: {CurrentEnergy}/{MaxEnergy} EN | Hand Drawn ({_skillHand.Count}): ");

        // Score all affordable skills for the primary action
        int bestPrimaryIndex = -1;
        float bestPrimaryScore = float.MinValue;

        for (int i = 0; i < _skillHand.Count; i++)
        {
            EnemyMove candidate = _skillHand[i];
            int cost = candidate.GetEffectiveEnergyCost();
            float score = EvaluateSkillUtility(candidate, simulatedBlockGain: 0, simulatedStrengthGain: 0, isSecondAction: false);

            logBuilder.Append($"[{candidate.GetDisplayName()} ({cost}EN): {score:F0}pts] ");

            if (cost <= CurrentEnergy && score > bestPrimaryScore)
            {
                bestPrimaryScore = score;
                bestPrimaryIndex = i;
            }
        }

        // If every skill in hand somehow costs more than MaxEnergy, pick the highest-scored skill anyway
        if (bestPrimaryIndex == -1)
        {
            bestPrimaryIndex = 0;
        }

        EnemyMove primarySkill = _skillHand[bestPrimaryIndex];
        _skillHand.RemoveAt(bestPrimaryIndex);
        _skillDiscardPile.Add(primarySkill);

        int primaryCost = Mathf.Min(CurrentEnergy, primarySkill.GetEffectiveEnergyCost());
        CurrentEnergy -= primaryCost;
        PlannedMoves.Add(primarySkill);

        // Check if the AI has enough remaining energy to chain a second skill from its hand
        // (Only when the primary skill isn't AttackAndDefend, which already occupies both primary & secondary intent slots)
        if (CurrentEnergy > 0 && primarySkill.Intent != EnemyIntent.AttackAndDefend && _skillHand.Count > 0)
        {
            int simBlock = ResolveMoveBlock(primarySkill);
            int simStr = (primarySkill.Intent == EnemyIntent.Buff ? ResolveMoveValue(primarySkill) : 0) + primarySkill.BonusStrength;

            int bestSecondaryIndex = -1;
            float bestSecondaryScore = 15f; // Minimum threshold so AI only chains worthwhile combos

            for (int i = 0; i < _skillHand.Count; i++)
            {
                EnemyMove candidate = _skillHand[i];
                int cost = candidate.GetEffectiveEnergyCost();
                if (cost > CurrentEnergy) continue;
                // Avoid chaining AttackAndDefend as a secondary slot since UI has 2 intent icons total
                if (candidate.Intent == EnemyIntent.AttackAndDefend) continue;

                float score = EvaluateSkillUtility(candidate, simBlock, simStr, isSecondAction: true);
                if (score > bestSecondaryScore)
                {
                    bestSecondaryScore = score;
                    bestSecondaryIndex = i;
                }
            }

            if (bestSecondaryIndex >= 0)
            {
                EnemyMove secondarySkill = _skillHand[bestSecondaryIndex];
                _skillHand.RemoveAt(bestSecondaryIndex);
                _skillDiscardPile.Add(secondarySkill);

                CurrentEnergy -= secondarySkill.GetEffectiveEnergyCost();
                PlannedMoves.Add(secondarySkill);
            }
        }

        // Log final decision
        if (PlannedMoves.Count == 1)
        {
            logBuilder.Append($"=> <b>Selected:</b> {PlannedMoves[0].GetDisplayName()} ({PlannedMoves[0].GetEffectiveEnergyCost()} EN) | Remaining EN: {CurrentEnergy}/{MaxEnergy}");
        }
        else if (PlannedMoves.Count >= 2)
        {
            logBuilder.Append($"=> <b>Combo Selected:</b> {PlannedMoves[0].GetDisplayName()} + {PlannedMoves[1].GetDisplayName()} | Remaining EN: {CurrentEnergy}/{MaxEnergy}");
        }
        Debug.Log(logBuilder.ToString());
    }

    /// <summary>
    /// Utility AI heuristic function that scores a candidate skill based on Enemy & Hero combat state.
    /// </summary>
    private float EvaluateSkillUtility(EnemyMove move, int simulatedBlockGain, int simulatedStrengthGain, bool isSecondAction)
    {
        float score = 50f;

        float enemyHpRatio = MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 1f;
        int effectiveStrength = Strength + simulatedStrengthGain;
        int effectiveBlock = CurrentBlock + simulatedBlockGain;

        HeroView hero = HeroSystem.Instance != null ? HeroSystem.Instance.HeroView : null;
        int heroHp = hero != null ? hero.CurrentHealth : 50;
        int heroBlock = hero != null ? hero.CurrentBlock : 0;
        int heroEffectiveHp = heroHp + heroBlock;
        bool heroVulnerable = hero != null && hero.Vulnerable > 0;

        int rawValue = ResolveMoveValue(move);
        int hits = move.GetEffectiveHits();
        float missingHpRatio = 1f - enemyHpRatio;

        // Expected attack damage across all hits (Strength applies per hit)
        int perHit = rawValue + effectiveStrength;
        if (heroVulnerable) perHit = Mathf.RoundToInt(perHit * 1.5f);
        int totalAttackDmg = move.IsAttack ? perHit * hits : 0;

        switch (move.Intent)
        {
            case EnemyIntent.Attack:
            {
                // Lethal check: highest priority if this attack can defeat the player
                if (totalAttackDmg >= heroEffectiveHp)
                    score += 120f;

                // Scale with expected damage output
                score += totalAttackDmg * 2.2f;

                // Multi-hit attacks are better at stripping player Block and scale harder with Strength
                if (hits > 1)
                    score += (hits - 1) * (4f + effectiveStrength * 1.5f);

                // Capitalize on Vulnerable debuff on player
                if (heroVulnerable)
                    score += 25f;

                // Bonus when player has no Block up
                if (heroBlock == 0)
                    score += 10f;

                break;
            }

            case EnemyIntent.Defend:
            {
                // Higher urgency when enemy HP is low
                score += missingHpRatio * 48f;

                if (enemyHpRatio <= 0.45f)
                    score += 22f;

                // Defending is very valuable when the enemy itself is Vulnerable
                if (Vulnerable > 0)
                    score += 24f;

                // Avoid defending at full HP when aggression is better
                if (enemyHpRatio >= 0.95f)
                    score -= 18f;

                // Diminishing returns if we already queued a block move this turn
                if (effectiveBlock > 0)
                    score -= 22f;

                score += (rawValue + move.BonusBlock) * 1.4f;
                break;
            }

            case EnemyIntent.Buff:
            {
                // Buffing Strength is best early/healthy and worse when near death
                if (enemyHpRatio >= 0.65f)
                    score += 28f;
                else if (enemyHpRatio < 0.30f)
                    score -= 25f;

                // Diminishing returns if Strength is already stacked high
                if (effectiveStrength >= Mathf.Max(4, BuffAmount * 2))
                    score -= 28f;
                else if (effectiveStrength == 0)
                    score += 18f;

                score += rawValue * 4.5f;
                break;
            }

            case EnemyIntent.Debuff:
            {
                // Debuffing the player with Vulnerable is huge if player is not yet vulnerable
                if (!heroVulnerable)
                    score += 35f + (move.ApplyVulnerable > 0 ? move.ApplyVulnerable * 8f : 10f);
                else
                    score += 10f + (move.ApplyVulnerable > 0 ? move.ApplyVulnerable * 3f : 5f);

                if (effectiveStrength > 0)
                    score += effectiveStrength * 3f;

                break;
            }

            case EnemyIntent.AttackAndDefend:
            {
                if (totalAttackDmg >= heroEffectiveHp)
                    score += 110f;

                score += totalAttackDmg * 1.8f + ResolveMoveBlock(move) * 1.5f + 22f;

                if (enemyHpRatio <= 0.70f)
                    score += 15f;

                break;
            }
        }

        // --- Secondary effects shared by all intents ---

        // Extra Strength riders (e.g. Privilege Escalation, Emergency Dump)
        if (move.BonusStrength > 0)
        {
            bool strengthCapped = effectiveStrength >= Mathf.Max(4, BuffAmount * 2);
            score += move.BonusStrength * (strengthCapped ? 1.5f : 4f);
        }

        // Extra block riders on non-defend skills
        if (move.Intent != EnemyIntent.Defend && move.Intent != EnemyIntent.AttackAndDefend && move.BonusBlock > 0)
            score += move.BonusBlock * 1.2f;

        // Vulnerable application on non-debuff skills: high value when the player isn't already exposed
        if (move.Intent != EnemyIntent.Debuff && move.ApplyVulnerable > 0)
        {
            score += heroVulnerable ? move.ApplyVulnerable * 4f : 14f + move.ApplyVulnerable * 6f;
        }

        // Life drain: worth more the more HP the enemy is missing
        if (move.LifeDrain && move.IsAttack)
        {
            int missingHp = MaxHealth - CurrentHealth;
            int expectedUnblocked = Mathf.Max(0, totalAttackDmg - heroBlock);
            score += Mathf.Min(missingHp, expectedUnblocked) * 1.6f;
        }

        // Anti-spam heuristic: slightly discourage repeating the exact same primary intent back-to-back
        if (!isSecondAction && _lastPrimaryIntent.HasValue && _lastPrimaryIntent.Value == move.Intent)
        {
            score -= 12f;
        }

        // Small organic variance to break ties naturally
        score += Random.Range(-3.5f, 3.5f);
        return score;
    }

    private void ShuffleSkillList(List<EnemyMove> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Returns the move pool for the current FSM state based on HP%.
    /// Falls back to the default MovePool if no FSM state matches.
    /// </summary>
    private List<EnemyMove> GetCurrentMovePool()
    {
        if (enemyData != null && enemyData.FSMStates != null && enemyData.FSMStates.Count > 0)
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
        return enemyData != null ? enemyData.MovePool : null;
    }

    /// <summary>
    /// Resolves the numerical value (damage, block, or buff amount) for a specific <see cref="EnemyMove"/>.
    /// </summary>
    public int ResolveMoveValue(EnemyMove move)
    {
        if (move != null && move.OverrideValue > 0)
            return move.OverrideValue;

        EnemyIntent intent = move != null ? move.Intent : CurrentIntent;
        return intent switch
        {
            EnemyIntent.Attack => AttackPower,
            EnemyIntent.Defend => BlockPower,
            EnemyIntent.Buff => BuffAmount,
            EnemyIntent.Debuff => move != null && move.ApplyVulnerable > 0 ? move.ApplyVulnerable : 1,
            EnemyIntent.AttackAndDefend => AttackPower,
            _ => AttackPower
        };
    }

    /// <summary>
    /// Resolves the amount of Block a move grants the enemy.
    /// Defend: base value + BonusBlock. AttackAndDefend: BonusBlock (or BlockPower). Others: BonusBlock.
    /// </summary>
    public int ResolveMoveBlock(EnemyMove move)
    {
        if (move == null) return 0;
        return move.Intent switch
        {
            EnemyIntent.Defend => ResolveMoveValue(move) + move.BonusBlock,
            EnemyIntent.AttackAndDefend => move.BonusBlock > 0 ? move.BonusBlock : BlockPower,
            _ => move.BonusBlock
        };
    }

    /// <summary>
    /// Returns the value associated with the current primary intent's move,
    /// or falls back to the enemy's default stat if no override is set.
    /// </summary>
    public int GetIntentValue()
    {
        if (PlannedMoves != null && PlannedMoves.Count > 0)
        {
            return ResolveMoveValue(PlannedMoves[0]);
        }

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
            EnemyIntent.Debuff => 1,
            _ => AttackPower
        };
    }

    public string GetIntentTooltip(bool isSecondary)
    {
        // Hardcore Mode: Enemy intents and tooltips are completely removed
        if (GameState.IsHardcoreMode)
            return string.Empty;

        // Multi-skill combo tooltip support
        if (PlannedMoves != null && PlannedMoves.Count >= 2)
        {
            if (isSecondary)
            {
                int priorStr = (PlannedMoves[0].Intent == EnemyIntent.Buff ? ResolveMoveValue(PlannedMoves[0]) : 0) + PlannedMoves[0].BonusStrength;
                return FormatMoveTooltip(PlannedMoves[1], priorStr);
            }
            return FormatMoveTooltip(PlannedMoves[0], 0);
        }

        EnemyMove primaryMove = (PlannedMoves != null && PlannedMoves.Count > 0) ? PlannedMoves[0] : null;

        if (CurrentIntent == EnemyIntent.AttackAndDefend)
        {
            int atkVal = (primaryMove != null ? ResolveMoveValue(primaryMove) : AttackPower) + Strength;
            if (isSecondary)
            {
                int blkVal = primaryMove != null ? ResolveMoveBlock(primaryMove) : BlockPower;
                return $"<color=#4488FF>Defend</color>\nThis enemy intends to gain <color=#4488FF>{blkVal}</color> Block.";
            }
            else
            {
                return primaryMove != null
                    ? FormatMoveTooltip(primaryMove)
                    : $"<color=#FF4444>Attack</color>\nThis enemy intends to deal <color=#FF4444>{atkVal}</color> damage.";
            }
        }

        // Secondary slot for composite riders (e.g. Attack + Vulnerable, Attack + BonusBlock, or Attack + BonusStrength)
        if (isSecondary && primaryMove != null)
        {
            if (primaryMove.ApplyVulnerable > 0 && primaryMove.Intent != EnemyIntent.Debuff)
            {
                return $"<color=#CC66FF>Debuff</color>\nThis enemy intends to apply <color=#CC66FF>{primaryMove.ApplyVulnerable} Vulnerable</color> to you (takes 50% more damage).";
            }
            if (primaryMove.BonusBlock > 0 && primaryMove.Intent != EnemyIntent.Defend)
            {
                return $"<color=#4488FF>Defend</color>\nThis enemy intends to gain <color=#4488FF>{primaryMove.BonusBlock}</color> Block.";
            }
            if (primaryMove.BonusStrength > 0 && primaryMove.Intent != EnemyIntent.Buff)
            {
                return $"<color=#44FF44>Buff</color>\nThis enemy intends to gain <color=#44FF44>+{primaryMove.BonusStrength}</color> Strength.";
            }
        }

        if (primaryMove != null)
            return FormatMoveTooltip(primaryMove);

        return CurrentIntent switch
        {
            EnemyIntent.Attack => $"<color=#FF4444>Attack</color>\nThis enemy intends to deal <color=#FF4444>{AttackPower + Strength}</color> damage.",
            EnemyIntent.Defend => $"<color=#4488FF>Defend</color>\nThis enemy intends to gain <color=#4488FF>{BlockPower}</color> Block.",
            EnemyIntent.Buff => $"<color=#44FF44>Buff</color>\nThis enemy intends to gain <color=#44FF44>+{BuffAmount}</color> Strength.",
            EnemyIntent.Debuff => $"<color=#CC66FF>Debuff</color>\nThis enemy intends to apply <color=#CC66FF>Vulnerable</color> to you.",
            _ => "Unknown Intent"
        };
    }

    private string FormatMoveTooltip(EnemyMove move, int extraStrength = 0)
    {
        int val = ResolveMoveValue(move);
        int hits = move.GetEffectiveHits();
        int block = ResolveMoveBlock(move);
        int effStr = Strength + extraStrength;
        string skillHeader = $"{move.GetDisplayName()} <color=#44FF44>({move.GetEffectiveEnergyCost()} EN)</color>";
        string dmgText = hits > 1
            ? $"<color=#FF4444>{val + effStr}</color> damage <color=#FF4444>{hits} times</color>"
            : $"<color=#FF4444>{val + effStr}</color> damage";

        string body = move.Intent switch
        {
            EnemyIntent.Attack => $"<color=#FF4444>{skillHeader}</color>\nThis enemy intends to deal {dmgText}.",
            EnemyIntent.Defend => $"<color=#4488FF>{skillHeader}</color>\nThis enemy intends to gain <color=#4488FF>{block}</color> Block.",
            EnemyIntent.Buff => $"<color=#44FF44>{skillHeader}</color>\nThis enemy intends to gain <color=#44FF44>+{val + move.BonusStrength}</color> Strength.",
            EnemyIntent.Debuff => $"<color=#CC66FF>{skillHeader}</color>\nThis enemy intends to apply <color=#CC66FF>{(move.ApplyVulnerable > 0 ? move.ApplyVulnerable : val)} Vulnerable</color> to you.",
            EnemyIntent.AttackAndDefend => $"<color=#FFAA00>{skillHeader}</color>\nDeals {dmgText} and gains <color=#4488FF>{block}</color> Block.",
            _ => "Unknown Intent"
        };

        if (move.Intent != EnemyIntent.Defend && move.Intent != EnemyIntent.AttackAndDefend && block > 0)
            body += $"\nAlso gains <color=#4488FF>{block}</color> Block.";
        if (move.Intent != EnemyIntent.Buff && move.BonusStrength > 0)
            body += $"\nGains <color=#44FF44>+{move.BonusStrength}</color> Strength.";
        if (move.Intent != EnemyIntent.Debuff && move.ApplyVulnerable > 0)
            body += $"\nApplies <color=#CC66FF>{move.ApplyVulnerable} Vulnerable</color> to you.";
        if (move.LifeDrain)
            body += "\n<color=#66FF66>Life Drain:</color> heals for unblocked damage dealt.";
        return body;
    }

    private void UpdateAttackText()
    {
        if (attackText != null)
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
        if (vulnerableText != null) vulnerableText.gameObject.SetActive(false);
    }

    private void UpdateIntentDisplay()
    {
        HideIntent();

        // Hardcore Mode: completely hide all enemy intents above enemies
        if (GameState.IsHardcoreMode)
        {
            return;
        }

        EnemyMove primaryMove = (PlannedMoves != null && PlannedMoves.Count > 0) ? PlannedMoves[0] : null;
        EnemyMove secondaryMove = (PlannedMoves != null && PlannedMoves.Count > 1) ? PlannedMoves[1] : null;

        int primaryValue = primaryMove != null ? ResolveMoveValue(primaryMove) : GetIntentValue();

        if (intentIcon != null && intentSprites != null && intentSprites.Length >= 4)
        {
            intentIcon.gameObject.SetActive(true);
            if (intentText != null) intentText.gameObject.SetActive(true);

            float rightEdgeX = 2.0f; // Pushed further right
            float yPosPrimary = 7.5f;
            float yPosSecondary = 6.5f; // Stacked vertically below
            float textOffsetX = -0.45f;  // Value closer to the left of the icon

            SetSingleIntentSlot(intentIcon, intentText, CurrentIntent, primaryValue, rightEdgeX, yPosPrimary, textOffsetX, primaryMove, 0);

            if (CurrentIntent == EnemyIntent.AttackAndDefend && secondaryIntentIcon != null && secondaryIntentText != null)
            {
                secondaryIntentIcon.gameObject.SetActive(true);
                secondaryIntentText.gameObject.SetActive(true);
                secondaryIntentIcon.sprite = intentSprites[1];
                secondaryIntentText.text = $"{(primaryMove != null ? ResolveMoveBlock(primaryMove) : BlockPower)}";
                secondaryIntentIcon.transform.position = new Vector3(rightEdgeX, yPosSecondary, 0f);
                secondaryIntentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosSecondary, 0f);
            }
            else if (secondaryMove != null && secondaryIntentIcon != null && secondaryIntentText != null)
            {
                int secondaryValue = ResolveMoveValue(secondaryMove);
                int priorStr = primaryMove != null
                    ? (primaryMove.Intent == EnemyIntent.Buff ? ResolveMoveValue(primaryMove) : 0) + primaryMove.BonusStrength
                    : 0;
                secondaryIntentIcon.gameObject.SetActive(true);
                secondaryIntentText.gameObject.SetActive(true);
                SetSingleIntentSlot(secondaryIntentIcon, secondaryIntentText, secondaryMove.Intent, secondaryValue, rightEdgeX, yPosSecondary, textOffsetX, secondaryMove, priorStr);
            }
            else if (primaryMove != null && primaryMove.ApplyVulnerable > 0 && primaryMove.Intent != EnemyIntent.Debuff && secondaryIntentIcon != null && secondaryIntentText != null)
            {
                // Composite rider: Primary is attack/buff/defend, secondary displays Debuff (intentSprites[3])
                secondaryIntentIcon.gameObject.SetActive(true);
                secondaryIntentText.gameObject.SetActive(true);
                secondaryIntentIcon.sprite = intentSprites[3];
                secondaryIntentText.text = $"{primaryMove.ApplyVulnerable}";
                secondaryIntentIcon.transform.position = new Vector3(rightEdgeX, yPosSecondary, 0f);
                secondaryIntentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosSecondary, 0f);
            }
            else if (primaryMove != null && primaryMove.BonusBlock > 0 && primaryMove.Intent != EnemyIntent.Defend && primaryMove.Intent != EnemyIntent.AttackAndDefend && secondaryIntentIcon != null && secondaryIntentText != null)
            {
                // Composite rider: Primary is attack/buff/debuff, secondary displays Defend (intentSprites[1])
                secondaryIntentIcon.gameObject.SetActive(true);
                secondaryIntentText.gameObject.SetActive(true);
                secondaryIntentIcon.sprite = intentSprites[1];
                secondaryIntentText.text = $"{primaryMove.BonusBlock}";
                secondaryIntentIcon.transform.position = new Vector3(rightEdgeX, yPosSecondary, 0f);
                secondaryIntentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosSecondary, 0f);
            }
            else if (primaryMove != null && primaryMove.BonusStrength > 0 && primaryMove.Intent != EnemyIntent.Buff && secondaryIntentIcon != null && secondaryIntentText != null)
            {
                // Composite rider: Primary is attack/defend/debuff, secondary displays Buff (intentSprites[2])
                secondaryIntentIcon.gameObject.SetActive(true);
                secondaryIntentText.gameObject.SetActive(true);
                secondaryIntentIcon.sprite = intentSprites[2];
                secondaryIntentText.text = $"{primaryMove.BonusStrength}";
                secondaryIntentIcon.transform.position = new Vector3(rightEdgeX, yPosSecondary, 0f);
                secondaryIntentText.transform.position = new Vector3(rightEdgeX + textOffsetX, yPosSecondary, 0f);
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
                        intentText.text = $"<color=#FF4444>ATK {primaryValue + Strength}</color>";
                        break;
                    case EnemyIntent.Defend:
                        intentText.text = $"<color=#4488FF>DEF {primaryValue}</color>";
                        break;
                    case EnemyIntent.AttackAndDefend:
                        intentText.text = $"<color=#FFAA00>ATK {primaryValue + Strength} + DEF {BlockPower}</color>";
                        break;
                    case EnemyIntent.Buff:
                        intentText.text = $"<color=#44FF44>BUFF +{primaryValue}</color>";
                        break;
                    case EnemyIntent.Debuff:
                        intentText.text = $"<color=#CC66FF>DEBUFF {primaryValue}</color>";
                        break;
                }
            }
        }
    }

    private void SetSingleIntentSlot(SpriteRenderer iconRenderer, TMP_Text valueText, EnemyIntent intent, int val, float x, float y, float textOffsetX, EnemyMove move = null, int extraStrength = 0)
    {
        iconRenderer.transform.position = new Vector3(x, y, 0f);
        if (valueText != null)
            valueText.transform.position = new Vector3(x + textOffsetX, y, 0f);

        int hits = move != null ? move.GetEffectiveHits() : 1;
        string suffix = "";
        if (move != null && move.ApplyVulnerable > 0 && intent != EnemyIntent.Debuff) suffix += "<color=#CC66FF>!</color>";
        if (move != null && move.LifeDrain) suffix += "<color=#66FF66>+</color>";

        switch (intent)
        {
            case EnemyIntent.Attack:
            case EnemyIntent.AttackAndDefend:
                iconRenderer.sprite = intentSprites[0];
                int atkVal = val + Strength + extraStrength;
                if (valueText != null) valueText.text = (hits > 1 ? $"{atkVal}x{hits}" : $"{atkVal}") + suffix;
                break;
            case EnemyIntent.Defend:
                iconRenderer.sprite = intentSprites[1];
                if (valueText != null) valueText.text = $"{(move != null ? ResolveMoveBlock(move) : val)}" + suffix;
                break;
            case EnemyIntent.Buff:
                iconRenderer.sprite = intentSprites[2];
                if (valueText != null) valueText.text = $"{val + (move != null ? move.BonusStrength : 0)}" + suffix;
                break;
            case EnemyIntent.Debuff:
                iconRenderer.sprite = intentSprites[3];
                int debuffVal = (move != null && move.ApplyVulnerable > 0) ? move.ApplyVulnerable : val;
                if (valueText != null) valueText.text = $"{debuffVal}" + suffix;
                break;
        }
    }
}
