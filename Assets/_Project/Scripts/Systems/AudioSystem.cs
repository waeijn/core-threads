using UnityEngine;

public class AudioSystem : PersistentSingleton<AudioSystem>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource hoverSource;

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

    [Header("Combat SFX")]
    public AudioClip AttackClip;
    public AudioClip BuffClip;
    public AudioClip BlockClip; // BufferShield
    public AudioClip DebuffClip;
    public AudioClip HealClip;

    [Header("Card SFX")]
    public AudioClip CardDrawClip;
    public AudioClip CardDiscardClip;

    [Header("Music")]
    public AudioClip DefaultMusicClip;

    private float lastHoverTime = 0f;

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

        if (hoverSource == null)
        {
            hoverSource = gameObject.AddComponent<AudioSource>();
            hoverSource.playOnAwake = false;
        }

        // Load saved volumes
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));

        if (DefaultMusicClip != null)
        {
            PlayMusic(DefaultMusicClip);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
        if (hoverSource != null) hoverSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;
        Debug.Log($"[AudioSystem] Playing music: {musicClip.name}");
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

    public void PlayHoverSFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            // Debounce: prevent machine-gun stutter if hovered rapidly
            if (Time.time - lastHoverTime < 0.05f) return;
            lastHoverTime = Time.time;

            // Use dedicated source so hover sounds interrupt each other instead of layering (stacking)
            hoverSource.volume = volumeScale;
            hoverSource.clip = clip;
            hoverSource.Play();
        }
    }

    public void StopHoverSFX()
    {
        if (hoverSource.isPlaying)
        {
            hoverSource.Stop();
        }
    }

    // --- Convenience Methods for common sounds ---

    public void PlayButtonClick() => PlaySFX(ButtonClickClip);
    public void PlayButtonHover() => PlayHoverSFX(ButtonHoverClip, 0.5f);
    public void PlayError() => PlaySFX(ErrorClip);
    public void PlayCardHover() => PlayHoverSFX(CardHoverClip, 0.6f);
    public void PlayEndTurn() => PlaySFX(EndTurnClip);
    public void PlayStartTurn() => PlaySFX(StartTurnClip);
    public void PlayShuffleDeck() => PlaySFX(ShuffleDeckClip);
    public void PlayGameOver() => PlaySFX(GameOverClip);
    
    public void PlayAttack() => PlaySFX(AttackClip);
    public void PlayBuff() => PlaySFX(BuffClip);
    public void PlayBlock() => PlaySFX(BlockClip);
    public void PlayDebuff() => PlaySFX(DebuffClip);
    public void PlayHeal() => PlaySFX(HealClip);
    
    public void PlayCardDraw() => PlaySFX(CardDrawClip);
    public void PlayCardDiscard() => PlaySFX(CardDiscardClip);
}
