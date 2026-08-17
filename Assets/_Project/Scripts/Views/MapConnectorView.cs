using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a stretched UI line (Image) between two RectTransform positions.
/// Attach to a UI GameObject with an Image component.
/// </summary>
[RequireComponent(typeof(Image))]
public class MapConnectorView : MonoBehaviour
{
    private Image _line;

    private void Awake()
    {
        _line = GetComponent<Image>();
    }

    /// <summary>
    /// Positions and rotates this connector to span from worldPosA to worldPosB.
    /// Both positions should be in Canvas (RectTransform) local space.
    /// </summary>
    public void Setup(Vector2 localPosA, Vector2 localPosB, MapConfig config, bool isActive)
    {
        _line = GetComponent<Image>();

        Vector2 dir    = localPosB - localPosA;
        float   length = dir.magnitude;
        float   angle  = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Position at midpoint
        var rt = GetComponent<RectTransform>();
        rt.anchoredPosition = (localPosA + localPosB) * 0.5f;
        rt.sizeDelta        = new Vector2(length, config.connectorWidth);
        rt.localRotation    = Quaternion.Euler(0, 0, angle);

        _line.color = isActive ? config.connectorActiveColor : config.connectorLockedColor;
    }

    public void RefreshColor(MapConfig config, bool isActive)
    {
        if (_line == null) _line = GetComponent<Image>();
        _line.color = isActive ? config.connectorActiveColor : config.connectorLockedColor;
    }
}
