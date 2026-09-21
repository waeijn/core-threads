using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameOverSystem : Singleton<GameOverSystem>
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private Button returnButton;

    public bool IsGameOver { get; private set; }

    void OnEnable()
    {
        IsGameOver = false;
        ActionSystem.SubscribeReaction<DealDamageGA>(CheckGameOver, ReactionTiming.POST);
        if (returnButton != null)
            returnButton.onClick.AddListener(OnReturnClicked);
    }

    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<DealDamageGA>(CheckGameOver, ReactionTiming.POST);
        if (returnButton != null)
            returnButton.onClick.RemoveListener(OnReturnClicked);
    }

    private void CheckGameOver(DealDamageGA dealDamageGA)
    {
        // Check if the hero is dead
        if (HeroSystem.Instance.HeroView.IsDead)
        {
            ShowGameOver("SYSTEM COMPROMISED\n<size=60%>CONNECTION TERMINATED</size>", Color.red, false);
            return;
        }

        // Check if all enemies are dead
        var enemies = EnemySystem.Instance.EnemyViews;
        if (enemies.Count > 0 && enemies.All(e => e.IsDead))
        {
            int playerHP = HeroSystem.Instance.HeroView.CurrentHealth;

            // Check if this was a boss node
            bool bossDefeated = GameState.SelectedNode?.Type == NodeType.Boss;

            GameState.CompleteSelectedNode(playerHP);

            if (bossDefeated)
            {
                IsGameOver = true;
                if (CardViewHoverSystem.Instance != null) CardViewHoverSystem.Instance.Hide();
                if (CardsSystem.Instance != null) CardsSystem.Instance.HideHand();

                if (GameState.CurrentAct >= 3)
                {
                    StartCoroutine(PlayFinalBossSequence());
                }
                else
                {
                    if (RewardSystem.Instance != null)
                    {
                        RewardSystem.Instance.ShowBossReward();
                    }
                    else
                    {
                        // Fallback
                        GameState.CurrentAct = Mathf.Clamp(GameState.CurrentAct + 1, 1, 3);
                        GameState.GeneratedMap = null;
                        GameState.SelectedNode = null;
                        ShowGameOver("SECTOR CLEARED\n<size=60%>ALL THREATS NEUTRALIZED</size>", Color.white, true);
                    }
                }
            }
            else
            {
                // Non-boss victory — show reward choices instead of game over panel
                IsGameOver = true;

                if (CardViewHoverSystem.Instance != null)
                    CardViewHoverSystem.Instance.Hide();
                if (CardsSystem.Instance != null)
                    CardsSystem.Instance.HideHand();

                if (RewardSystem.Instance != null)
                {
                    RewardSystem.Instance.ShowPostCombatReward();
                }
                else
                {
                    // Fallback if RewardSystem is not in scene
                    ShowGameOver("THREAT NEUTRALIZED\n<size=60%>SYSTEM SECURE</size>", Color.white, true);
                }
            }
            return;
        }
    }

    private void ShowGameOver(string message, Color color, bool isVictory)
    {
        IsGameOver = true;

        if (!isVictory)
        {
            AudioSystem.Instance?.PlayGameOver();
        }

        if (CardViewHoverSystem.Instance != null)
        {
            CardViewHoverSystem.Instance.Hide();
        }

        if (CardsSystem.Instance != null)
        {
            CardsSystem.Instance.HideHand();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.color = color;
        }

        // Show return button after delay
        if (returnButton != null)
        {
            returnButton.gameObject.SetActive(false);
            StartCoroutine(ShowReturnButtonAfterDelay(1.5f, isVictory));
        }
    }

    private IEnumerator ShowReturnButtonAfterDelay(float delay, bool isVictory)
    {
        yield return new WaitForSeconds(delay);
        if (returnButton != null)
        {
            returnButton.gameObject.SetActive(true);
            var btnText = returnButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.text = isVictory ? "RETURN TO MAP" : "RESTART";
            }
        }
    }

    private void OnReturnClicked()
    {
        if (HeroSystem.Instance.HeroView.IsDead)
        {
            // Defeat — full reset
            GameState.ResetRun();
        }

        // Return to map (always)
        if (GameState.IsInitialized)
        {
            SceneManager.LoadScene("MapScene");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator PlayFinalBossSequence()
    {
        var boss = EnemySystem.Instance.EnemyViews.FirstOrDefault();
        if (boss != null)
        {
            // Flash boss red and shake violently
            boss.SpriteTransform.DOShakePosition(2.0f, 0.8f, 30);
            boss.spriteRenderer.DOColor(Color.red, 2.0f);
            
            AudioSystem.Instance?.PlayGameOver(); // Reusing game over sound for dramatic effect
            yield return new WaitForSeconds(2.0f);

            // Explode particles via code
            GameObject psObj = new GameObject("BossExplosion");
            psObj.transform.position = boss.SpriteTransform.position;
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Stop the default "Play On Awake" before modifying duration
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.duration = 1f;
            main.startSpeed = 15f;
            main.startSize = 0.8f;
            main.startColor = Color.red;
            main.maxParticles = 200;
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0f, 150) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            
            var psr = psObj.GetComponent<ParticleSystemRenderer>();
            psr.material = new Material(Shader.Find("Sprites/Default"));
            
            ps.Play();

            // Hide boss sprite
            boss.SpriteTransform.gameObject.SetActive(false);
            
            yield return new WaitForSeconds(1.5f);
        }

        // Flash screen white
        GameObject flashObj = new GameObject("WhiteFlash");
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            flashObj.transform.SetParent(canvas.transform, false);
            Image flashImg = flashObj.AddComponent<Image>();
            flashImg.color = new Color(1, 1, 1, 0);
            RectTransform rect = flashObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            flashImg.DOFade(1f, 1.5f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(2.0f);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        // Load Credits
        SceneManager.LoadScene("CreditsScene");
    }
}
