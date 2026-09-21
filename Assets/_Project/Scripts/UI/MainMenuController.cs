using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button archiveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private GameObject archivePanel;

    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (archiveButton != null) archiveButton.onClick.AddListener(OnArchiveClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        
        AddHoverSound(playButton);
        AddHoverSound(archiveButton);
        AddHoverSound(settingsButton);
        AddHoverSound(quitButton);
        AddHoverSound(settingsBackButton);

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (archivePanel != null) archivePanel.SetActive(false);
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnPlayClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        Debug.Log("[MainMenu] Play Clicked -> Loading MapScene");
        SceneManager.LoadScene("MapScene");
    }

    private void OnArchiveClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (archivePanel != null)
        {
            var archiveUI = archivePanel.GetComponent<ArchiveUI>();
            if (archiveUI != null)
                archiveUI.Show();
            else
                archivePanel.SetActive(true);
        }
    }

    private void OnSettingsClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    private void OnSettingsBackClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnQuitClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        Debug.Log("[MainMenu] Quit Clicked");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
