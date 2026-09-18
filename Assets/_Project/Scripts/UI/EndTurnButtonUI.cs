using UnityEngine;

public class EndTurnButtonUI : MonoBehaviour
{
    public void onClick()
    {
        if (AudioSystem.Instance != null) AudioSystem.Instance.PlayEndTurn();
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
