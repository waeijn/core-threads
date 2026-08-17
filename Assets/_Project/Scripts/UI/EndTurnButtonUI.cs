using UnityEngine;

public class EndTurnButtonUI : MonoBehaviour
{
    public void onClick()
    {
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
