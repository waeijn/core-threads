using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BuildCreditsScene : Editor
{
    [MenuItem("Tools/Build Credits Scene")]
    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Setup Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        
        // Setup Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Add Credits Manager Script
        GameObject managerObj = new GameObject("CreditsManager");
        // We will create the CreditsController script separately. For now, add it if it exists, but let's just make the script later and attach it manually, or just use a dummy.
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = Color.black;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Container for scrolling
        GameObject containerObj = new GameObject("CreditsContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 1f); // Pivot at Top
        containerRect.anchoredPosition = new Vector2(0, 300); // Logo will be perfectly centered at start
        containerRect.sizeDelta = new Vector2(1400, 0); // Height will be driven by ContentSizeFitter

        VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 80;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;

        ContentSizeFitter csf = containerObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Setup Manager Component
        CreditsController controller = managerObj.AddComponent<CreditsController>();
        controller.creditsContainer = containerRect;

        // Add Logo
        GameObject logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(containerObj.transform, false);
        Image logoImg = logoObj.AddComponent<Image>();
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/UI/jberg_logo.png");
        logoImg.sprite = logoSprite;
        logoImg.preserveAspect = true;
        LayoutElement logoLe = logoObj.AddComponent<LayoutElement>();
        logoLe.minHeight = 400;

        // Add Spacing after Logo
        GameObject spacerObj = new GameObject("Spacer");
        spacerObj.transform.SetParent(containerObj.transform, false);
        LayoutElement spacerLe = spacerObj.AddComponent<LayoutElement>();
        spacerLe.minHeight = 400; // Big gap before credits start

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // Two-column credits data
        string[,] credits = new string[,] {
            { "John Wayne S. Landong", "Project Manager\nGame Designer" },
            { "Raymark L. Oronan", "Lead Programmer" },
            { "Benedic L. Sarmiento", "Artist / Animator" },
            { "Gabriel C. Montablan", "Sound Designer" },
            { "Elisabeth A. Pulma", "QA Tester" }
        };

        for (int i = 0; i < credits.GetLength(0); i++)
        {
            GameObject rowObj = new GameObject($"CreditRow_{i}");
            rowObj.transform.SetParent(containerObj.transform, false);
            HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 100;
            hlg.childAlignment = TextAnchor.UpperCenter;

            // Name (Left)
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(rowObj.transform, false);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = credits[i, 0];
            nameTmp.fontSize = 42;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.TopRight;
            if (font != null) nameTmp.font = font;
            LayoutElement nameLe = nameObj.AddComponent<LayoutElement>();
            nameLe.minWidth = 600;

            // Role (Right)
            GameObject roleObj = new GameObject("Role");
            roleObj.transform.SetParent(rowObj.transform, false);
            TextMeshProUGUI roleTmp = roleObj.AddComponent<TextMeshProUGUI>();
            roleTmp.text = credits[i, 1];
            roleTmp.fontSize = 42;
            roleTmp.color = new Color(0.7f, 0.7f, 0.7f); // Slightly gray for roles
            roleTmp.alignment = TextAlignmentOptions.TopLeft;
            if (font != null) roleTmp.font = font;
            LayoutElement roleLe = roleObj.AddComponent<LayoutElement>();
            roleLe.minWidth = 600;
        }

        // Add "Press ESC to skip" hint
        GameObject hintObj = new GameObject("SkipHint");
        hintObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI hintTmp = hintObj.AddComponent<TextMeshProUGUI>();
        hintTmp.text = "Press ESC to skip";
        hintTmp.fontSize = 24;
        hintTmp.color = new Color(1, 1, 1, 0.5f);
        hintTmp.alignment = TextAlignmentOptions.BottomRight;
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(1, 0);
        hintRect.anchorMax = new Vector2(1, 0);
        hintRect.pivot = new Vector2(1, 0);
        hintRect.anchoredPosition = new Vector2(-50, 50);
        hintRect.sizeDelta = new Vector2(400, 50);
        if (font != null) hintTmp.font = font;

        // Save Scene
        EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/CreditsScene.unity");
        Debug.Log("CreditsScene built!");
        
        // Add to Build Settings
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        bool found = false;
        foreach (var s in scenes)
        {
            if (s.path == "Assets/_Project/Scenes/CreditsScene.unity")
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            System.Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[scenes.Length] = new EditorBuildSettingsScene("Assets/_Project/Scenes/CreditsScene.unity", true);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log("Added CreditsScene to Build Settings!");
        }
    }
}
