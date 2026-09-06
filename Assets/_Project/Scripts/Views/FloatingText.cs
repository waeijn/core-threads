using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Self-destroying floating text that animates upward and fades out.
/// Spawned by CombatFeedbackSystem to show damage, block, and heal numbers.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    private float _lifetime   = 0.9f;
    private float _floatSpeed = 2.5f;

    private void Awake()
    {
        // Auto-resolve if not wired in the prefab Inspector
        if (label == null)
            label = GetComponent<TextMeshPro>();
    }

    public void Init(string text, Color color)
    {
        if (label != null)
        {
            label.text  = text;
            label.color = color;
        }
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed   = 0f;
        Color startColor = label != null ? label.color : Color.white;

        while (elapsed < _lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _lifetime;

            // Float upward
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            // Fade out in the second half of the lifetime
            if (label != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.4f) / 0.6f));
                label.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
