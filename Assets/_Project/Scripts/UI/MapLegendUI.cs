using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggleable legend panel showing what each node type does.
/// Attach to a UI panel and wire the toggle button in the Inspector.
/// </summary>
public class MapLegendUI : MonoBehaviour
{
    [SerializeField] private GameObject legendPanel;

    private void Awake()
    {
        if (legendPanel != null) legendPanel.SetActive(true);
    }
}
