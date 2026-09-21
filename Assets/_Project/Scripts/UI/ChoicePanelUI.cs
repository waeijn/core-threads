using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable binary choice panel for map node interactions (Treasure, Rest) and post-combat rewards.
/// Shows a title, two option buttons, and a skip button.
/// </summary>
public class ChoicePanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button optionAButton;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private Button optionBButton;
    [SerializeField] private TMP_Text optionBText;
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text skipText;

    private Action onOptionA;
    private Action onOptionB;
    private Action onSkip;

    private void Awake()
    {
        if (optionAButton != null) 
        {
            optionAButton.onClick.AddListener(OnOptionAClicked);
            AddHoverSound(optionAButton);
        }
        if (optionBButton != null)
        {
            optionBButton.onClick.AddListener(OnOptionBClicked);
            AddHoverSound(optionBButton);
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
            AddHoverSound(skipButton);
        }

        // Fix text overflow programmatically so long hardware names shrink to fit
        if (optionAText != null)
        {
            optionAText.enableAutoSizing = true;
            optionAText.fontSizeMin = 16;
            optionAText.fontSizeMax = optionAText.fontSize; // cap at original size
            optionAText.enableWordWrapping = true;
        }

        if (optionBText != null)
        {
            optionBText.enableAutoSizing = true;
            optionBText.fontSizeMin = 16;
            optionBText.fontSizeMax = optionBText.fontSize; // cap at original size
            optionBText.enableWordWrapping = true;
        }

        gameObject.SetActive(false);
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter || e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);
        
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { if (button.interactable) AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    private Vector2 optionAOriginalPos;
    private Color optionAOriginalColor = Color.white;
    private bool hasOriginalPos = false;

    /// <summary>
    /// Displays the choice panel with two options and a skip button.
    /// </summary>
    public void Show(string title, string optA, string optB, Action onA, Action onB, Action onSkipAction,
        bool optionBEnabled = true)
    {
        if (titleText != null) titleText.text = title;
        if (optionAText != null) optionAText.text = optA;
        if (optionBText != null) optionBText.text = optB;

        onOptionA = onA;
        onOptionB = onB;
        onSkip = onSkipAction;

        if (optionAButton != null && !hasOriginalPos)
        {
            optionAOriginalPos = optionAButton.GetComponent<RectTransform>().anchoredPosition;
            var img = optionAButton.GetComponent<Image>();
            if (img != null) optionAOriginalColor = img.color;
            hasOriginalPos = true;
        }

        // Dynamic coloring for Option A
        if (optionAButton != null)
        {
            Image btnAImage = optionAButton.GetComponent<Image>();
            if (btnAImage != null)
            {
                if (optA.ToUpper().Contains("DUPLICATE"))
                {
                    ColorUtility.TryParseHtmlString("#2D6A8B", out Color blueColor);
                    btnAImage.color = blueColor;
                }
                else if (optA.ToUpper().Contains("OVERCLOCK"))
                {
                    // Amber/Gold for Energy
                    ColorUtility.TryParseHtmlString("#B8860B", out Color goldColor);
                    btnAImage.color = goldColor;
                }
                else
                {
                    btnAImage.color = optionAOriginalColor;
                }
            }
        }

        if (optionBButton != null) 
        {
            if (string.IsNullOrEmpty(optB) || onB == null)
            {
                optionBButton.gameObject.SetActive(false);
                if (optionAButton != null)
                {
                    optionAButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, optionAOriginalPos.y);
                }
            }
            else
            {
                optionBButton.gameObject.SetActive(true);
                optionBButton.interactable = optionBEnabled;
                if (optionAButton != null)
                {
                    optionAButton.GetComponent<RectTransform>().anchoredPosition = optionAOriginalPos;
                }

                // Dynamic coloring for Option B
                Image btnBImage = optionBButton.GetComponent<Image>();
                if (btnBImage != null)
                {
                    if (optB.ToUpper().Contains("EXPANDED"))
                    {
                        // Deep Purple/Teal for System Expansion
                        ColorUtility.TryParseHtmlString("#533B75", out Color purpleColor);
                        btnBImage.color = purpleColor;
                    }
                    else if (optB.ToUpper().Contains("DRAFT") || optB.ToUpper().Contains("ACQUIRE"))
                    {
                        btnBImage.color = optionAOriginalColor;
                    }
                    else if (optB.ToUpper().Contains("GARBAGE") || optB.ToUpper().Contains("PURGE"))
                    {
                        ColorUtility.TryParseHtmlString("#8B2D2D", out Color redColor);
                        btnBImage.color = redColor;
                    }
                }
            }
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(onSkip != null);
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            if (skipButton != null && skipButton.gameObject.activeSelf && onSkip != null)
            {
                OnSkipClicked();
            }
        }
    }

    private void OnOptionAClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
        onOptionA?.Invoke();
    }

    private void OnOptionBClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
        onOptionB?.Invoke();
    }

    private void OnSkipClicked()
    {
        AudioSystem.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
        onSkip?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
