using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AssignBGMAudioConfig
{
    static AssignBGMAudioConfig()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        string configPath = "Assets/_Project/Resources/AudioConfig.asset";
        AudioConfig config = AssetDatabase.LoadAssetAtPath<AudioConfig>(configPath);
        if (config == null) return;

        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Others/BackgroundMusic.mp3");

        config.BackgroundMusic = bgm;
        config.BGMVolume = 0.2f; // Low volume so it doesn't overpower

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }
}
