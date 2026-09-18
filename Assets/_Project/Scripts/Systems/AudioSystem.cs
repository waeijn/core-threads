using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    public static AudioSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        // Stub for Raymark's missing AudioSystem
        if (clip != null)
        {
            Debug.Log($"[AudioSystem Stub] Played SFX: {clip.name} at volume {volume}");
        }
    }
}
