using TMPro;
using UnityEngine;

public class HeroView : CombatantView
{
    public void Setup(HeroData heroData)
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var pUI = canvas.transform.Find("HealthUIContainer/PlayerHealthUI") ?? canvas.transform.Find("PlayerHealthUI");
            if (pUI != null)
            {
                var hpTr  = pUI.Find("PlayerHealthText");
                var blkTr = pUI.Find("PlayerBlockText");
                var strTr = pUI.Find("PlayerStrengthText");
                if (hpTr != null)  healthText = hpTr.GetComponent<TMP_Text>();
                if (blkTr != null) blockText  = blkTr.GetComponent<TMP_Text>();
                if (strTr != null) strengthText = strTr.GetComponent<TMP_Text>();
            }
        }

        if (healthText == null)
        {
            var hpObj = GameObject.Find("PlayerHealthText");
            if (hpObj != null) healthText = hpObj.GetComponent<TMP_Text>();
        }
        if (blockText == null)
        {
            var blkObj = GameObject.Find("PlayerBlockText");
            if (blkObj != null) blockText = blkObj.GetComponent<TMP_Text>();
        }
        if (strengthText == null)
        {
            var strObj = GameObject.Find("PlayerStrengthText");
            if (strObj != null) strengthText = strObj.GetComponent<TMP_Text>();
        }

        SetupBase(heroData.Health, heroData.Image);
    }
}
