using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private List<EnemyData> fallbackEnemyDatas;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    private void Start()
    {
        List<EnemyData> enemiesToSpawn;

        MapNodeRuntime selectedNode = GameState.SelectedNode;

        if (GameState.IsInitialized && selectedNode != null)
        {
            // Handle non-combat nodes that somehow routed here — skip back to map
            if (selectedNode.Type == NodeType.Rest || selectedNode.Type == NodeType.Treasure)
            {
                Debug.Log($"[MatchSetupSystem] Non-combat node ({selectedNode.Type}) — returning to map.");
                GameState.CompleteSelectedNode(GameState.PlayerCurrentHP > 0
                    ? GameState.PlayerCurrentHP
                    : GameState.PlayerMaxHP);
                SceneManager.LoadScene("MapScene");
                return;
            }

            // Boss node uses BossEnemy field
            if (selectedNode.Type == NodeType.Boss)
            {
                enemiesToSpawn = selectedNode.BossEnemy != null
                    ? new List<EnemyData> { selectedNode.BossEnemy }
                    : fallbackEnemyDatas;
            }
            else
            {
                // Regular combat
                enemiesToSpawn = selectedNode.Enemies != null && selectedNode.Enemies.Length > 0
                    ? new List<EnemyData>(selectedNode.Enemies)
                    : fallbackEnemyDatas;
            }

            // Swap background if available
            if (backgroundRenderer != null && selectedNode.BossEnemy == null)
            {
                // No background swap needed for now
            }
        }
        else
        {
            // Fallback: direct play without map (for testing)
            enemiesToSpawn = fallbackEnemyDatas;
        }

        HeroSystem.Instance.Setup(heroData);

        // Restore player HP from previous fights
        if (GameState.PlayerCurrentHP > 0)
        {
            HeroSystem.Instance.HeroView.SetCurrentHP(GameState.PlayerCurrentHP);
        }

        EnemySystem.Instance.Setup(enemiesToSpawn);
        CardsSystem.Instance.Setup(GameState.PlayerDeck ?? heroData.Deck);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
