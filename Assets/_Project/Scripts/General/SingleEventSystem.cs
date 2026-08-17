using UnityEngine;

/// <summary>
/// Ensures exactly ONE EventSystem exists across all loaded scenes (Single-instance pattern).
/// Automatically destroys duplicate EventSystems loaded via additive scene loading.
/// </summary>
public class SingleEventSystem : MonoBehaviour
{
    private static SingleEventSystem _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // Duplicate EventSystem detected from additive scene load -> destroy immediately
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
