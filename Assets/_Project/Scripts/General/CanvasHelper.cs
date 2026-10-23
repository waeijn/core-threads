using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Universal helper to safely locate the scene-level UI Canvas.
/// Excludes DontDestroyOnLoad and overlay canvases like TooltipCanvas.
/// </summary>
public static class CanvasHelper
{
    /// <summary>
    /// Returns the main UI Canvas belonging to the given scene (or active scene).
    /// Prevents inadvertently parenting UI to persistent or tooltip canvases.
    /// </summary>
    public static Canvas GetSceneCanvas(Scene scene = default)
    {
        if (!scene.IsValid())
            scene = SceneManager.GetActiveScene();

        Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);

        // 1. Preferred: Canvas directly belonging to the target scene and not a tooltip
        foreach (var c in allCanvases)
        {
            if (c == null) continue;
            if (IsIgnoredCanvas(c)) continue;
            if (c.gameObject.scene == scene)
                return c;
        }

        // 2. Secondary: Any Canvas not marked ignored
        foreach (var c in allCanvases)
        {
            if (c == null) continue;
            if (IsIgnoredCanvas(c)) continue;
            return c;
        }

        return null;
    }

    private static bool IsIgnoredCanvas(Canvas c)
    {
        if (c.gameObject.name.Contains("Tooltip") || c.transform.root.name.Contains("Tooltip"))
            return true;
        if (c.gameObject.name.Contains("ConfirmationCanvas"))
            return true;
        return false;
    }
}
