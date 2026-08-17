using UnityEngine;

public class AttackHeroGA : GameAction
{
    public EnemyView Attacker {  get; private set; }

    public AttackHeroGA(EnemyView attacker)
    {
        Attacker = attacker;
    }
}
