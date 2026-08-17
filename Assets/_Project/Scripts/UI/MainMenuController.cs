using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;

    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MainMenu] Play Clicked -> Loading MapScene");
        SceneManager.LoadScene("MapScene");
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    private void OnSettingsBackClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quit Clicked");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
