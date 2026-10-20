using UnityEngine;

public class AttackHeroGA : GameAction
{
    public EnemyView Attacker { get; private set; }
    public int? OverrideBaseDamage { get; private set; }

    /// <summary>If true, the attacker heals for the unblocked HP damage this hit deals.</summary>
    public bool LifeDrain { get; private set; }

    /// <summary>Strength gained by the attacker after this hit resolves (for Attack + Buff rider skills).</summary>
    public int BonusStrengthAfterHit { get; private set; }

    public AttackHeroGA(EnemyView attacker, int? overrideBaseDamage = null, bool lifeDrain = false, int bonusStrengthAfterHit = 0)
    {
        Attacker = attacker;
        OverrideBaseDamage = overrideBaseDamage;
        LifeDrain = lifeDrain;
        BonusStrengthAfterHit = bonusStrengthAfterHit;
    }
}

/// <summary>
/// Resolves after an enemy's LifeDrain hit: heals the enemy for the HP the hero actually lost.
/// </summary>
public class EnemyDrainGA : GameAction
{
    public EnemyView Enemy { get; private set; }
    public CombatantView Victim { get; private set; }
    public int VictimHpBefore { get; private set; }

    public EnemyDrainGA(EnemyView enemy, CombatantView victim, int victimHpBefore)
    {
        Enemy = enemy;
        Victim = victim;
        VictimHpBefore = victimHpBefore;
    }
}
