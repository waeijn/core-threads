using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MapSceneLayoutFixer : Editor
{
    [MenuItem("Tools/Fix MapScene DeckViewer Layout")]
    public static void FixLayout()
    {
        // Ensure we are operating on the MapScene
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "MapScene")
        {
            Debug.LogError("Please open the MapScene before running this tool!");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Fix DeckViewerPanel
        Transform deckViewerPanel = canvas.transform.Find("DeckViewerPanel");
        if (deckViewerPanel != null)
        {
            Transform titleText = deckViewerPanel.Find("TitleText");
            if (titleText != null)
            {
                RectTransform rt = titleText.GetComponent<RectTransform>();
                // Shift down explicitly for MapScene
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -70f);
            }

            Transform countText = deckViewerPanel.Find("CardCountText");
            if (countText != null)
            {
                RectTransform rt = countText.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -110f);
            }

            // Also shift the scroll area down so cards don't overlap the text
            Transform scrollArea = deckViewerPanel.Find("ScrollArea");
            if (scrollArea != null)
            {
                RectTransform rt = scrollArea.GetComponent<RectTransform>();
                // Increase the top padding (anchorMax Y from 0.85 to 0.8)
                rt.anchorMax = new Vector2(rt.anchorMax.x, 0.75f);
            }
        }

        // Fix PileViewerPanel (if MapScene has one, though it might not)
        Transform pileViewerPanel = canvas.transform.Find("PileViewerPanel");
        if (pileViewerPanel != null)
        {
            Transform titleText = pileViewerPanel.Find("TitleText");
            if (titleText != null)
            {
                RectTransform rt = titleText.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -70f);
            }

            Transform countText = pileViewerPanel.Find("CardCountText");
            if (countText != null)
            {
                RectTransform rt = countText.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -110f);
            }

            Transform scrollArea = pileViewerPanel.Find("ScrollArea");
            if (scrollArea != null)
            {
                RectTransform rt = scrollArea.GetComponent<RectTransform>();
                rt.anchorMax = new Vector2(rt.anchorMax.x, 0.75f);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Successfully adjusted the DeckViewer layout ONLY for the MapScene!");
    }
}
