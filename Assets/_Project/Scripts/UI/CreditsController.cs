using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    [Header("Scroll Configuration")]
    public RectTransform creditsContainer;
    public float scrollSpeed = 150f;
    public float startDelay = 2.0f;

    private float timer = 0f;
    private bool isFinished = false;

    private void Start()
    {
        // Ensure Hardcore Mode is unlocked upon reaching the credits
        GameState.UnlockHardcoreMode();

        // 1. Clean up any stray persistent white flash overlays from prior scenes
        CleanStrayOverlays();

        // 2. Smoothly fade in from white into the credits using a pure Coroutine
        StartCoroutine(FadeFromWhiteRoutine());
    }

    /// <summary>
    /// Checks persistent objects (like TooltipCanvas or DontDestroyOnLoad roots)
    /// and removes any stray WhiteFlash/WhiteFadeIn overlays that survived scene transitions.
    /// </summary>
    private void CleanStrayOverlays()
    {
        // Check TooltipManager hierarchy
        var tooltipManager = TooltipManager.Instance;
        if (tooltipManager != null)
        {
            for (int i = tooltipManager.transform.childCount - 1; i >= 0; i--)
            {
                var child = tooltipManager.transform.GetChild(i);
                if (child.name != "TooltipCanvas")
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    for (int j = child.childCount - 1; j >= 0; j--)
                    {
                        var grandChild = child.GetChild(j);
                        if (grandChild.name != "TooltipPanel")
                        {
                            Destroy(grandChild.gameObject);
                        }
                    }
                }
            }
        }

        // Also search for any stray "WhiteFlash" or "WhiteFadeIn" in any loaded scene/root
        var strayFlashes = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in strayFlashes)
        {
            if (go == null) continue;
            if (go.name == "WhiteFlash" || (go.name == "WhiteFadeIn" && go.scene.name != gameObject.scene.name))
            {
                Destroy(go);
            }
        }
    }

    private IEnumerator FadeFromWhiteRoutine()
    {
        Canvas canvas = CanvasHelper.GetSceneCanvas(gameObject.scene);
        if (canvas == null) yield break;

        GameObject flashObj = new GameObject("WhiteFadeIn");
        flashObj.transform.SetParent(canvas.transform, false);
        flashObj.transform.SetAsLastSibling();

        Image flashImg = flashObj.AddComponent<Image>();
        flashImg.raycastTarget = false;
        flashImg.color = Color.white;

        RectTransform rect = flashObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (flashImg != null)
            {
                flashImg.color = new Color(1f, 1f, 1f, 1f - t);
            }
            yield return null;
        }

        if (flashObj != null)
        {
            Destroy(flashObj);
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

            // If it has scrolled off screen, end credits
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
