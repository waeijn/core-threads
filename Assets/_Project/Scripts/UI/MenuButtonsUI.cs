using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the top-right menu icon buttons: Map, Deck, Settings.
/// Attach to a parent object containing the 3 buttons.
/// </summary>
public class MenuButtonsUI : MonoBehaviour
{
    [SerializeField] private Button mapButton;
    [SerializeField] private Button deckButton;
    [SerializeField] private Button settingsButton;

    [Header("Panels")]
    [SerializeField] private DeckViewerUI deckViewer;
    [SerializeField] private GameObject settingsPanel;

    private void OnEnable()
    {
        if (mapButton != null) mapButton.onClick.AddListener(OnMapClicked);
        if (deckButton != null) deckButton.onClick.AddListener(OnDeckClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnDisable()
    {
        if (mapButton != null) mapButton.onClick.RemoveListener(OnMapClicked);
        if (deckButton != null) deckButton.onClick.RemoveListener(OnDeckClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    private void OnMapClicked()
    {
        // Don't allow during combat actions
        if (ActionSystem.Instance != null && ActionSystem.Instance.IsPerforming) return;

        var mapScene = SceneManager.GetSceneByName("MapScene");
        if (mapScene.isLoaded)
        {
            // Toggle off — unload MapScene overlay
            SceneManager.UnloadSceneAsync("MapScene");
        }
        else if (GameState.IsInitialized)
        {
            // Toggle on — load MapScene additively
            SceneManager.LoadScene("MapScene", LoadSceneMode.Additive);
        }
    }

    private void OnDeckClicked()
    {
        if (deckViewer != null)
        {
            if (deckViewer.IsOpen)
                deckViewer.Hide();
            else
                deckViewer.Show();
        }
        // Close settings if open
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
        // Close deck if open
        if (deckViewer != null) deckViewer.Hide();
    }
}
