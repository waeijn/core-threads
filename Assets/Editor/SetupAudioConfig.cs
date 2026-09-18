using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SetupAudioConfig
{
    static SetupAudioConfig()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        string configPath = "Assets/_Project/Resources/AudioConfig.asset";
        AudioConfig config = AssetDatabase.LoadAssetAtPath<AudioConfig>(configPath);
        if (config == null) return;

        AudioClip click = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/ButtonClick.mp3");
        AudioClip endTurn = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/EndTurn.mp3");
        AudioClip gameOver = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/GameOver.mp3");

        config.ButtonClick = click;
        config.EndTurn = endTurn;
        config.GameOver = gameOver;

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }
}
