using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class DeckButtonFinalMover : Editor
{
    [MenuItem("Tools/Finalize Deck Button Migration")]
    public static void MigrateDeckButton()
    {
        // 1. Find DeckBtn in the current GameScene first!
        GameObject deckBtnInScene = null;
        Transform topBar = GameObject.Find("TopBar")?.transform;
        if (topBar != null)
        {
            var allTransforms = topBar.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t.name == "DeckBtn")
                {
                    deckBtnInScene = t.gameObject;
                    break;
                }
            }
        }

        if (deckBtnInScene == null)
        {
            Debug.LogWarning("DeckBtn not found in the open scene! Please open GameScene.");
            return;
        }

        // 2. Clone it so we have a completely clean object severed from the prefab
        GameObject newDeckBtn = Instantiate(deckBtnInScene);
        newDeckBtn.name = "DeckBtn";
        newDeckBtn.SetActive(true);

        // 3. Move the clone to the Canvas
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            newDeckBtn.transform.SetParent(canvas.transform, true);
            
            GameObject drawPile = GameObject.Find("DrawPileUI");
            if (drawPile != null)
            {
                newDeckBtn.transform.SetSiblingIndex(drawPile.transform.GetSiblingIndex() + 1);
            }
        }

        // 4. Set RectTransform on the clone
        RectTransform rt = newDeckBtn.GetComponent<RectTransform>();
        if (rt != null)
        {
            var le = newDeckBtn.GetComponent<LayoutElement>();
            if (le != null) DestroyImmediate(le);

            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(40, 140);
            rt.sizeDelta = new Vector2(80, 80);
        }

        // 5. Replace script on the clone
        DeckButtonUI deckUI = newDeckBtn.GetComponent<DeckButtonUI>();
        if (deckUI == null) deckUI = newDeckBtn.AddComponent<DeckButtonUI>();

        GameObject deckViewerPanel = GameObject.Find("DeckViewerPanel");
        if (deckViewerPanel != null)
        {
            DeckViewerUI viewerUI = deckViewerPanel.GetComponent<DeckViewerUI>();
            if (viewerUI != null)
            {
                SerializedObject so = new SerializedObject(deckUI);
                so.FindProperty("deckViewer").objectReferenceValue = viewerUI;
                so.FindProperty("button").objectReferenceValue = newDeckBtn.GetComponent<Button>();
                so.ApplyModifiedProperties();
            }
        }

        EditorUtility.SetDirty(newDeckBtn);

        // 6. Now that the clone is safe, completely obliterate it from the Prefab on disk
        string prefabPath = "Assets/_Project/Prefabs/UI/MenuButtons.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        Transform deckBtnInPrefab = prefabRoot.transform.Find("DeckBtn");
        if (deckBtnInPrefab != null)
        {
            DestroyImmediate(deckBtnInPrefab.gameObject);
            Debug.Log("Removed DeckBtn from MenuButtons prefab.");
        }
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Deck Button Migration Complete!");
    }
}
