using UnityEditor;
using UnityEngine;
using TMPro;

public static class PolishTopBarEditor
{
    [MenuItem("Tools/Polish TopBar")]
    public static void Polish()
    {
        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/_Project/Prefabs/UI/TopBar.prefab"))
        {
            var root = scope.prefabContentsRoot;

            var playerText = root.transform.Find("PlayerInfoText");
            if (playerText != null) {
                var rt = playerText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0.25f, 1f);
                rt.offsetMin = new Vector2(30, 4);
                rt.offsetMax = new Vector2(0, -4);
                var tmp = playerText.GetComponent<TMP_Text>();
                tmp.fontSize = 20;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }

            var hpText = root.transform.Find("HPText");
            if (hpText != null) {
                var rt = hpText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.25f, 0f);
                rt.anchorMax = new Vector2(0.45f, 1f);
                rt.offsetMin = new Vector2(0, 4);
                rt.offsetMax = new Vector2(0, -4);
                var tmp = hpText.GetComponent<TMP_Text>();
                tmp.fontSize = 18;
                tmp.fontStyle = FontStyles.Normal;
                tmp.color = new Color(0.55f, 0.85f, 0.55f, 1f);
                tmp.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }

            var actText = root.transform.Find("ActText");
            if (actText != null) {
                var rt = actText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.35f, 0f);
                rt.anchorMax = new Vector2(0.65f, 1f);
                rt.offsetMin = new Vector2(0, 4);
                rt.offsetMax = new Vector2(0, -4);
                var tmp = actText.GetComponent<TMP_Text>();
                tmp.fontSize = 20;
                tmp.fontStyle = FontStyles.Normal;
                tmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                tmp.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }

            var menuBtns = root.transform.Find("MenuButtons");
            if (menuBtns != null) {
                var rt = menuBtns.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.offsetMin = new Vector2(-260, 0);
                rt.offsetMax = new Vector2(-16, 0);
            }
        }
    }
}
