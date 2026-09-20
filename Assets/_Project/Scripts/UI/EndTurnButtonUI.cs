using UnityEngine;

public class EndTurnButtonUI : MonoBehaviour
{
    public void onClick()
    {
        AudioSystem.Instance?.PlayEndTurn();
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
