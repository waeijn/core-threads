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
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        
        AddHoverSound(mapButton);
        AddHoverSound(settingsButton);
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        // Prevent duplicate triggers if OnEnable is called multiple times
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter);
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnDisable()
    {
        if (mapButton != null) mapButton.onClick.RemoveListener(OnMapClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    private void OnMapClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        // Don't allow during combat actions
        if (ActionSystem.Instance != null && ActionSystem.Instance.IsPerforming) return;

        var mapScene = SceneManager.GetSceneByName("MapScene");
        if (mapScene.isLoaded)
        {
            // Toggle off — unload MapScene overlay
            SceneManager.UnloadSceneAsync("MapScene");
        }
        else
        {
            // Toggle on — load MapScene additively
            SceneManager.LoadScene("MapScene", LoadSceneMode.Additive);
        }
    }

    private void OnSettingsClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();

        if (settingsPanel == null)
        {
            var allSettings = Resources.FindObjectsOfTypeAll<SettingsPanelUI>();
            foreach (var s in allSettings)
            {
                if (s.gameObject.scene.isLoaded)
                {
                    settingsPanel = s.gameObject;
                    break;
                }
            }
        }

        // Close deck if open
        if (deckViewer != null && deckViewer.IsOpen)
        {
            deckViewer.Hide();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
        else
        {
            Debug.LogWarning("[MenuButtonsUI] SettingsPanel not found in scene!");
        }
    }
}
