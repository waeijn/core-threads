using UnityEngine;

[CreateAssetMenu(menuName = "Data/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Header("Background Music")]
    public AudioClip BackgroundMusic;
    [Range(0f, 1f)] public float BGMVolume = 0.5f;

    [Header("Global UI Audio")]
    public AudioClip ButtonClick;
    public AudioClip EndTurn;
    public AudioClip GameOver;
}
