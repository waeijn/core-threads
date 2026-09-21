using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the Deck button that sits near the Draw Pile in GameScene.
/// </summary>
public class DeckButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private DeckViewerUI deckViewer;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnDeckClicked);
            AddHoverSound(button);
        }
    }

    private void AddHoverSound(Button btn)
    {
        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter || e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnDeckClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        
        // Dynamically find it if not assigned in inspector
        if (deckViewer == null)
        {
            var viewers = Resources.FindObjectsOfTypeAll<DeckViewerUI>();
            foreach (var v in viewers)
            {
                // Ensure it's part of the scene, not a prefab asset
                if (v.gameObject.scene.isLoaded)
                {
                    deckViewer = v;
                    break;
                }
            }
        }

        if (deckViewer != null)
        {
            if (deckViewer.IsOpen)
                deckViewer.Hide();
            else
                deckViewer.Show();
        }
        else
        {
            Debug.LogWarning("[DeckButtonUI] DeckViewerUI not found in scene!");
        }
    }
}
