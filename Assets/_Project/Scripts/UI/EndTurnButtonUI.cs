using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonUI : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            var trigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter || e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
            
            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((data) => { if (button.interactable) AudioSystem.Instance?.PlayButtonHover(); });
            trigger.triggers.Add(enterEntry);

            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
            trigger.triggers.Add(exitEntry);
        }
    }

    public void onClick()
    {
        AudioSystem.Instance?.PlayEndTurn();
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
