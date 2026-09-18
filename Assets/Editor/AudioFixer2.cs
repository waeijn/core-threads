using UnityEditor;
using UnityEngine;

public static class AudioFixer2
{
    public static void Run()
    {
        string configPath = "Assets/_Project/Resources/AudioConfig.asset";
        AudioConfig config = AssetDatabase.LoadAssetAtPath<AudioConfig>(configPath);
        if (config == null) return;

        config.BackgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/BackgroundMusic.mp3");
        config.ButtonClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/ButtonClick.mp3");
        config.EndTurn = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/EndTurn.mp3");
        config.GameOver = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/GameOver.mp3");

        config.BGMVolume = 0.15f; 

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Antigravity: Success");
    }
}
