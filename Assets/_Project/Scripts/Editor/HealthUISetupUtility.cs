using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HealthUISetupUtility
{
    [MenuItem("Tools/Setup Symmetrical Health UI")]
    public static void SetupHealthUI()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in active scene!");
            return;
        }

        // Load Health Icons
        string iconPath = "Assets/_Project/Sprites/UI/Health Icons.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(iconPath);
        Sprite playerIcon = assets.OfType<Sprite>().FirstOrDefault(s => s.name == "Health Icons_0");
        Sprite enemyIcon = assets.OfType<Sprite>().FirstOrDefault(s => s.name == "Health Icons_1");

        if (playerIcon == null || enemyIcon == null)
        {
            Debug.LogError("Health Icons sprites not found!");
            return;
        }

        // 1. Find or create HealthUIContainer under Canvas
        Transform existingContainer = canvas.transform.Find("HealthUIContainer");
        if (existingContainer != null)
        {
            Object.DestroyImmediate(existingContainer.gameObject);
        }

        GameObject containerObj = new GameObject("HealthUIContainer", typeof(RectTransform));
        containerObj.transform.SetParent(canvas.transform, false);
        RectTransform containerRT = containerObj.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0, 1);
        containerRT.anchorMax = new Vector2(1, 1);
        containerRT.pivot = new Vector2(0.5f, 1);
        containerRT.anchoredPosition = new Vector2(0, -65); // Placed right below TopBar
        containerRT.sizeDelta = new Vector2(0, 60);

        // --- Player Health UI (Top Left) ---
        GameObject playerUI = new GameObject("PlayerHealthUI", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        playerUI.transform.SetParent(containerObj.transform, false);
        RectTransform playerRT = playerUI.GetComponent<RectTransform>();
        playerRT.anchorMin = new Vector2(0, 0.5f);
        playerRT.anchorMax = new Vector2(0, 0.5f);
        playerRT.pivot = new Vector2(0, 0.5f);
        playerRT.anchoredPosition = new Vector2(25, 0);
        playerRT.sizeDelta = new Vector2(320, 40);

        HorizontalLayoutGroup playerHLG = playerUI.GetComponent<HorizontalLayoutGroup>();
        playerHLG.childControlWidth = false;
        playerHLG.childControlHeight = false;
        playerHLG.childForceExpandWidth = false;
        playerHLG.childForceExpandHeight = false;
        playerHLG.spacing = 10;
        playerHLG.childAlignment = TextAnchor.MiddleLeft;

        CreateImage(playerUI, "PlayerHealthIcon", playerIcon, new Vector2(32, 32));
        TextMeshProUGUI playerHPText = CreateText(playerUI, "PlayerHealthText", "44/44", 22, Color.white);
        playerHPText.rectTransform.sizeDelta = new Vector2(90, 32);
        TextMeshProUGUI playerBlockText = CreateText(playerUI, "PlayerBlockText", "", 20, new Color(0.2f, 0.8f, 1f));
        playerBlockText.rectTransform.sizeDelta = new Vector2(90, 32);
        playerBlockText.gameObject.SetActive(false);

        // --- Enemy Health UI (Top Right - Symmetrical) ---
        GameObject enemyUI = new GameObject("EnemyHealthUI", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        enemyUI.transform.SetParent(containerObj.transform, false);
        RectTransform enemyRT = enemyUI.GetComponent<RectTransform>();
        enemyRT.anchorMin = new Vector2(1, 0.5f);
        enemyRT.anchorMax = new Vector2(1, 0.5f);
        enemyRT.pivot = new Vector2(1, 0.5f);
        enemyRT.anchoredPosition = new Vector2(-25, 0);
        enemyRT.sizeDelta = new Vector2(320, 40);

        HorizontalLayoutGroup enemyHLG = enemyUI.GetComponent<HorizontalLayoutGroup>();
        enemyHLG.childControlWidth = false;
        enemyHLG.childControlHeight = false;
        enemyHLG.childForceExpandWidth = false;
        enemyHLG.childForceExpandHeight = false;
        enemyHLG.spacing = 10;
        enemyHLG.childAlignment = TextAnchor.MiddleRight;

        TextMeshProUGUI enemyBlockText = CreateText(enemyUI, "EnemyBlockText", "", 20, new Color(0.2f, 0.8f, 1f));
        enemyBlockText.rectTransform.sizeDelta = new Vector2(90, 32);
        enemyBlockText.gameObject.SetActive(false);
        TextMeshProUGUI enemyHPText = CreateText(enemyUI, "EnemyHealthText", "22/22", 22, Color.white);
        enemyHPText.rectTransform.sizeDelta = new Vector2(90, 32);
        CreateImage(enemyUI, "EnemyHealthIcon", enemyIcon, new Vector2(32, 32));

        // --- Assign to HeroView in Scene ---
        HeroView heroView = Object.FindFirstObjectByType<HeroView>();
        if (heroView != null)
        {
            if (heroView.healthText != null && heroView.healthText != playerHPText)
            {
                heroView.healthText.gameObject.SetActive(false);
            }
            heroView.healthText = playerHPText;
            heroView.blockText = playerBlockText;
            EditorUtility.SetDirty(heroView);
        }

        // --- Clean up EnemyView Prefab ---
        string prefabPath = "Assets/_Project/Prefabs/Enemies/Basic/EnemyView.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot != null)
        {
            EnemyView enemyPrefab = prefabRoot.GetComponent<EnemyView>();
            if (enemyPrefab != null)
            {
                enemyPrefab.healthText = null;
                enemyPrefab.blockText = null;
            }

            Transform baseChild = prefabRoot.transform.Find("CombatantViewBase");
            if (baseChild != null)
            {
                Transform old3DHealth = baseChild.Find("HealthText");
                if (old3DHealth != null)
                {
                    Object.DestroyImmediate(old3DHealth.gameObject);
                }

                Transform old3DBlock = baseChild.Find("BlockText");
                if (old3DBlock != null)
                {
                    Object.DestroyImmediate(old3DBlock.gameObject);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        // --- Clean up 3D text in scene views ---
        EnemyView[] enemyViews = Object.FindObjectsByType<EnemyView>(FindObjectsSortMode.None);
        foreach (EnemyView ev in enemyViews)
        {
            Transform baseChild = ev.transform.Find("CombatantViewBase");
            if (baseChild != null)
            {
                Transform old3DHealth = baseChild.Find("HealthText");
                if (old3DHealth != null) old3DHealth.gameObject.SetActive(false);

                Transform old3DBlock = baseChild.Find("BlockText");
                if (old3DBlock != null) old3DBlock.gameObject.SetActive(false);
            }
            ev.healthText = enemyHPText;
            ev.blockText = enemyBlockText;
            EditorUtility.SetDirty(ev);
        }

        if (heroView != null)
        {
            Transform baseChild = heroView.transform.Find("CombatantViewBase");
            if (baseChild != null)
            {
                Transform old3DHealth = baseChild.Find("HealthText");
                if (old3DHealth != null) old3DHealth.gameObject.SetActive(false);

                Transform old3DBlock = baseChild.Find("BlockText");
                if (old3DBlock != null) old3DBlock.gameObject.SetActive(false);
            }
            heroView.healthText = playerHPText;
            heroView.blockText = playerBlockText;
            EditorUtility.SetDirty(heroView);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Successfully setup symmetrical Player and Enemy Health UI!");
    }

    private static TextMeshProUGUI CreateText(GameObject parent, string name, string text, float fontSize, Color color)
    {
        GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Midline;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    private static Image CreateImage(GameObject parent, string name, Sprite sprite, Vector2 size)
    {
        GameObject imgObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        imgObj.transform.SetParent(parent.transform, false);
        RectTransform rt = imgObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        Image img = imgObj.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        return img;
    }
}
