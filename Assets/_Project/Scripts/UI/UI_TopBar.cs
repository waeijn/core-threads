using UnityEngine;
using TMPro;

public class UI_TopBar : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private TMP_Text leftStatusText;
    [SerializeField] private TMP_Text centerTitleText;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        bool isMapView = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MapScene";

        // Left Status Panel
        if (leftStatusText != null)
        {
            if (isMapView)
            {
                int hp = GameState.PlayerCurrentHP > 0 ? GameState.PlayerCurrentHP : GameState.PlayerMaxHP;
                leftStatusText.text = $"SYSTEM HEALTH: {hp}/{GameState.PlayerMaxHP}";
                leftStatusText.color = Color.white;
            }
            else
            {
                string name = GameState.HeroData != null ? GameState.HeroData.HeroName : "THREAD-04";
                leftStatusText.text = name;
                leftStatusText.color = Color.yellow;
            }
        }

        // Center Title Panel
        if (centerTitleText != null)
        {
            string sectorName = "Unknown";
            switch (GameState.CurrentAct)
            {
                case 1: sectorName = "Floppy Sector"; break;
                case 2: sectorName = "System RAM"; break;
                case 3: sectorName = "CPU Core"; break;
            }
            centerTitleText.text = $"ACT {GameState.CurrentAct} [{sectorName}]";
        }
    }
}
