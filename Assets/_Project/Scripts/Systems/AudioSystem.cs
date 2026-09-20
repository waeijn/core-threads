using UnityEngine;

public class AudioSystem : Singleton<AudioSystem>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("UI SFX")]
    public AudioClip ButtonClickClip;
    public AudioClip ButtonHoverClip;
    public AudioClip ErrorClip;

    [Header("Gameplay SFX")]
    public AudioClip CardHoverClip;
    public AudioClip EndTurnClip;
    public AudioClip StartTurnClip;
    public AudioClip ShuffleDeckClip;
    public AudioClip GameOverClip;

    [Header("Music")]
    public AudioClip DefaultMusicClip;

    protected override void Awake()
    {
        base.Awake();
        
        // Auto-create AudioSources if not set in inspector
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (DefaultMusicClip != null)
        {
            PlayMusic(DefaultMusicClip);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    // --- Convenience Methods for common sounds ---

    public void PlayButtonClick() => PlaySFX(ButtonClickClip);
    public void PlayButtonHover() => PlaySFX(ButtonHoverClip, 0.5f); // Hover usually quieter
    public void PlayError() => PlaySFX(ErrorClip);
    public void PlayCardHover() => PlaySFX(CardHoverClip, 0.6f);
    public void PlayEndTurn() => PlaySFX(EndTurnClip);
    public void PlayStartTurn() => PlaySFX(StartTurnClip);
    public void PlayShuffleDeck() => PlaySFX(ShuffleDeckClip);
    public void PlayGameOver() => PlaySFX(GameOverClip);
}
