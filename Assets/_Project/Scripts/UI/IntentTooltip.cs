using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class IntentTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public EnemyView Enemy;
    public bool IsSecondary;

    private void OnMouseEnter()
    {
        ShowTooltip();
    }

    private void OnMouseExit()
    {
        HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (GameState.IsHardcoreMode) return;
        if (Enemy == null) return;

        string text = Enemy.GetIntentTooltip(IsSecondary);
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
