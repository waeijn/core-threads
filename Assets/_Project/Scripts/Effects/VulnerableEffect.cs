using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class VulnerableEffect : Effect
{
    [SerializeField] private int vulnerableAmount;

    public override GameAction GetGameAction()
    {
        List<CombatantView> targets = EnemySystem.Instance.EnemyViews
            .Cast<CombatantView>()
            .ToList();

        return new ApplyVulnerableGA(vulnerableAmount, targets);
    }
}
