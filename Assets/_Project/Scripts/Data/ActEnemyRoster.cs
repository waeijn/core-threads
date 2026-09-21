using UnityEngine;

[CreateAssetMenu(menuName = "Data/ActEnemyRoster")]
public class ActEnemyRoster : ScriptableObject
{
    [Tooltip("The Act number (1, 2, or 3)")]
    public int ActNumber;

    [Tooltip("All standard (non-boss) enemies native to this Act")]
    public EnemyData[] StandardEnemies;

    [Tooltip("The final boss for this Act")]
    public EnemyData BossEnemy;
}
