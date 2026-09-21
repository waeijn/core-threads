using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeButton; // If they have an X button on the panel

    private void OnEnable()
    {
        transform.SetAsLastSibling();
        if (CardsSystem.Instance != null) CardsSystem.Instance.HideHand();
    }

    private void OnDisable()
    {
        if (CardsSystem.Instance != null) CardsSystem.Instance.ShowHand();
    }

    private void Start()
    {
        // Initialize sliders to current volume
        if (masterSlider != null)
        {
            masterSlider.value = AudioListener.volume;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            AddHoverSound(masterSlider.gameObject);
        }

        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            AddHoverSound(musicSlider.gameObject);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            AddHoverSound(sfxSlider.gameObject);
        }

        // Hook up Quit button
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
            AddHoverSound(quitButton.gameObject);
            
            // Dynamically change text based on scene
            var tmpro = quitButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmpro != null)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
                    tmpro.text = "BACK";
                else
                    tmpro.text = "MAIN MENU";
            }
        }

        // Hook up Close button if they assign one
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
            AddHoverSound(closeButton.gameObject);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.SetSFXVolume(value);
        }
    }

    private void OnQuitClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            // Just close the settings panel since there's already a giant Quit button on the main screen
            Debug.Log("[Settings] Back Clicked (Closing Panel)");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("[Settings] Return to Main Menu");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnCloseClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            OnCloseClicked();
        }
    }

    private void AddHoverSound(GameObject obj)
    {
        if (obj == null) return;
        var trigger = obj.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? obj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }
}
