using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class IntentTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public EnemyView Enemy;
    public bool IsSecondary;

    private void OnMouseEnter()
    {
        Debug.Log($"[IntentTooltip] OnMouseEnter on {gameObject.name}");
        ShowTooltip();
    }

    private void OnMouseExit()
    {
        Debug.Log($"[IntentTooltip] OnMouseExit on {gameObject.name}");
        HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[IntentTooltip] OnPointerEnter on {gameObject.name}");
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[IntentTooltip] OnPointerExit on {gameObject.name}");
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (Enemy == null) {
            Debug.LogWarning("[IntentTooltip] Enemy is null!");
            return;
        }
        string text = Enemy.GetIntentTooltip(IsSecondary);
        Debug.Log($"[IntentTooltip] Showing text: {text}");
        if (!string.IsNullOrEmpty(text))
        {
            TooltipManager.Show(text);
        }
    }

    private void HideTooltip()
    {
        TooltipManager.Hide();
    }
    
    private void OnDisable()
    {
        HideTooltip();
    }
}
