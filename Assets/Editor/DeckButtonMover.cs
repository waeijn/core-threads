using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class DeckButtonMover : Editor
{
    [MenuItem("Tools/Move Deck Button")]
    public static void MoveDeckButton()
    {
        string prefabPath = "Assets/_Project/Prefabs/UI/MenuButtons.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        // Find the DeckBtn
        Transform deckBtn = prefabRoot.transform.Find("DeckBtn");
        if (deckBtn != null)
        {
            // We want it to ignore the HorizontalLayoutGroup on MenuButtons
            LayoutElement le = deckBtn.GetComponent<LayoutElement>();
            if (le == null)
            {
                le = deckBtn.gameObject.AddComponent<LayoutElement>();
            }
            le.ignoreLayout = true;

            // Move to Bottom Left
            RectTransform rt = deckBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(25, 140); // Just above Draw Pile
            rt.sizeDelta = new Vector2(80, 80); // Typical icon button size

            // Ensure parent RectTransform covers full screen so bottom-left anchor is correct
            // Wait, MenuButtonsUI is usually anchored top-right!
            // If the parent is anchored top-right, anchoring bottom-left relative to parent might be weird
            // Let's check MenuButtonsUI anchors in GameScene just to be safe.
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Deck Button Moved to Bottom Left!");
    }
}
