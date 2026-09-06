using System.Collections;
using UnityEngine;

/// <summary>
/// Centralized combat feedback singleton.
/// Provides screen shake, sprite flash, and floating number spawning.
/// Add this to a persistent GameObject in GameScene and assign the
/// FloatingText prefab and the Main Camera reference in the Inspector.
/// </summary>
public class CombatFeedbackSystem : MonoBehaviour
{
    public static CombatFeedbackSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject floatingTextPrefab;

    [Header("Screen Shake")]
    [SerializeField] private float defaultShakeDuration  = 0.18f;
    [SerializeField] private float defaultShakeIntensity = 0.18f;

    [Header("Sprite Flash")]
    [SerializeField] private float flashDuration = 0.12f;

    private Vector3 _cameraOrigin;
    private bool    _isShaking;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            _cameraOrigin = mainCamera.transform.localPosition;
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Shake the main camera. Safe to call even if camera is null.</summary>
    public void ScreenShake(float duration = -1f, float intensity = -1f)
    {
        if (mainCamera == null) return;
        if (!_isShaking)
            StartCoroutine(ShakeRoutine(
                duration  < 0 ? defaultShakeDuration  : duration,
                intensity < 0 ? defaultShakeIntensity : intensity));
    }

    /// <summary>Briefly flash a SpriteRenderer to the given hit color.</summary>
    public void FlashSprite(SpriteRenderer sr, Color flashColor)
    {
        if (sr == null) return;
        StartCoroutine(FlashRoutine(sr, flashColor));
    }

    /// <summary>Spawn a floating number at a world-space position.</summary>
    public void SpawnFloatingText(Vector3 worldPos, string text, Color color)
    {
        if (floatingTextPrefab == null) return;
        StartCoroutine(SpawnRoutine(worldPos, text, color));
    }

    private IEnumerator SpawnRoutine(Vector3 worldPos, string text, Color color)
    {
        // Slight random horizontal spread so multiple numbers don't stack
        Vector3 spawnPos = worldPos + new Vector3(Random.Range(-0.3f, 0.3f), 0.4f, -1f);
        GameObject go = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);

        // Wait one frame so Awake() has run on the spawned object
        yield return null;

        go.GetComponent<FloatingText>()?.Init(text, color);
    }

    // ── Coroutines ─────────────────────────────────────────────────────────

    private IEnumerator ShakeRoutine(float duration, float intensity)
    {
        _isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration); // Ease out

            float offsetX = Random.Range(-1f, 1f) * intensity * t;
            float offsetY = Random.Range(-1f, 1f) * intensity * t;
            mainCamera.transform.localPosition = _cameraOrigin + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        mainCamera.transform.localPosition = _cameraOrigin;
        _isShaking = false;
    }

    private IEnumerator FlashRoutine(SpriteRenderer sr, Color flashColor)
    {
        Color original = sr.color;
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        // Restore only if it hasn't been changed by something else
        if (sr != null)
            sr.color = original;
    }
}
