using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DealDamageEffect : Effect
{
    [SerializeField] private int damageAmount;

    public override GameAction GetGameAction()
    {
        // Target all enemies currently on the board
        List<CombatantView> targets = EnemySystem.Instance.EnemyViews
            .Cast<CombatantView>()
            .ToList();

        DealDamageGA dealDamageGA = new(damageAmount, targets);
        return dealDamageGA;
    }
}
