using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameOverSystem : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    void OnEnable()
    {
        ActionSystem.SubscribeReaction<DealDamageGA>(CheckGameOver, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<DealDamageGA>(CheckGameOver, ReactionTiming.POST);
    }

    private void CheckGameOver(DealDamageGA dealDamageGA)
    {
        // Check if the hero is dead
        if (HeroSystem.Instance.HeroView.IsDead)
        {
            ShowGameOver("SYSTEM COMPROMISED\n<size=60%>CONNECTION TERMINATED</size>", Color.red);
            return;
        }

        // Check if all enemies are dead
        var enemies = EnemySystem.Instance.EnemyViews;
        if (enemies.Count > 0 && enemies.All(e => e.IsDead))
        {
            ShowGameOver("THREAT NEUTRALIZED\n<size=60%>SYSTEM SECURE</size>", Color.cyan);
            return;
        }
    }

    private void ShowGameOver(string message, Color color)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.color = color;
        }
    }
}
