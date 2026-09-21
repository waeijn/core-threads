using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixChoicePanelText : Editor
{
    [MenuItem("Tools/Fix Choice Panel Text Overflow")]
    public static void FixText()
    {
        // Fix in GameScene
        Scene gameScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/GameScene.unity", OpenSceneMode.Single);
        FixInScene(gameScene);
        EditorSceneManager.SaveScene(gameScene);
        
        // Fix in MapScene
        Scene mapScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MapScene.unity", OpenSceneMode.Single);
        FixInScene(mapScene);
        EditorSceneManager.SaveScene(mapScene);
        
        Debug.Log("Finished fixing text on all ChoicePanelUIs!");
    }
    
    private static void FixInScene(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var panels = root.GetComponentsInChildren<ChoicePanelUI>(true);
            foreach (var panel in panels)
            {
                // We need to use serialized fields to get the text references since they are private
                SerializedObject so = new SerializedObject(panel);
                SerializedProperty textAProp = so.FindProperty("optionAText");
                SerializedProperty textBProp = so.FindProperty("optionBText");
                
                if (textAProp != null && textAProp.objectReferenceValue != null)
                {
                    TMP_Text textA = textAProp.objectReferenceValue as TMP_Text;
                    textA.enableAutoSizing = true;
                    textA.fontSizeMin = 18;
                    textA.fontSizeMax = 36;
                    textA.enableWordWrapping = true;
                    textA.overflowMode = TextOverflowModes.Truncate;
                    EditorUtility.SetDirty(textA);
                }
                
                if (textBProp != null && textBProp.objectReferenceValue != null)
                {
                    TMP_Text textB = textBProp.objectReferenceValue as TMP_Text;
                    textB.enableAutoSizing = true;
                    textB.fontSizeMin = 18;
                    textB.fontSizeMax = 36;
                    textB.enableWordWrapping = true;
                    textB.overflowMode = TextOverflowModes.Truncate;
                    EditorUtility.SetDirty(textB);
                }
            }
        }
    }
}
