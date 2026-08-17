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
        if (optionAButton != null) optionAButton.onClick.AddListener(OnOptionAClicked);
        if (optionBButton != null) optionBButton.onClick.AddListener(OnOptionBClicked);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);
        gameObject.SetActive(false);
    }

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

        if (optionBButton != null) optionBButton.interactable = optionBEnabled;

        gameObject.SetActive(true);
    }

    private void OnOptionAClicked()
    {
        gameObject.SetActive(false);
        onOptionA?.Invoke();
    }

    private void OnOptionBClicked()
    {
        gameObject.SetActive(false);
        onOptionB?.Invoke();
    }

    private void OnSkipClicked()
    {
        gameObject.SetActive(false);
        onSkip?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
