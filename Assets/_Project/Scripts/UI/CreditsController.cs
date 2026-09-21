using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class CreditsController : MonoBehaviour
{
    public RectTransform creditsContainer;
    public float scrollSpeed = 150f;
    public float startDelay = 2.0f;
    
    private float timer = 0f;
    private bool isFinished = false;

    private void Start()
    {
        // Smoothly fade from white (continuing the boss explosion flash)
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            GameObject flashObj = new GameObject("WhiteFadeIn");
            flashObj.transform.SetParent(canvas.transform, false);
            flashObj.transform.SetAsLastSibling(); // Ensure it renders on top of everything
            
            Image flashImg = flashObj.AddComponent<Image>();
            flashImg.color = Color.white;
            
            RectTransform rect = flashObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            flashImg.DOFade(0f, 1.5f).SetEase(Ease.InOutSine).OnComplete(() => {
                Destroy(flashObj);
            });
        }
    }

    private void Update()
    {
        // Skip via Esc or Click
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            FinishCredits();
        }

        if (isFinished) return;

        timer += Time.deltaTime;
        
        // Wait for startDelay before scrolling
        if (timer > startDelay && creditsContainer != null)
        {
            creditsContainer.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            
            // If it has scrolled off screen (roughly), end it
            if (creditsContainer.anchoredPosition.y > 3500)
            {
                FinishCredits();
            }
        }
    }

    private void FinishCredits()
    {
        if (isFinished) return;
        isFinished = true;
        
        // Reset the run state
        GameState.ResetRun();
        
        // Return to main menu
        SceneManager.LoadScene("MainMenu");
    }
}
