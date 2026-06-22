using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The parent canvas or group for the start menu UI.")]
    public GameObject startCanvasGroup;

    [Tooltip("The sub-panel holding the delay adjustment slider and buttons.")]
    public GameObject delaySettingsPanel;

    [Header("Delay Settings Elements")]
    [Tooltip("The main trigger button that opens/closes the settings.")]
    public Button delayMenuButton;

    [Tooltip("The text inside the delay trigger button showing current state.")]
    public TextMeshProUGUI delayMenuButtonText;

    [Tooltip("Slider for selecting the delay value (-1.00s to +1.00s).")]
    public Slider delaySlider;

    [Tooltip("Display text for the current delay value.")]
    public TextMeshProUGUI delayValueText;

    [Tooltip("Minus button on the left of the slider.")]
    public Button minusButton;

    [Tooltip("Plus button on the right of the slider.")]
    public Button plusButton;

    [Tooltip("Close settings button.")]
    public Button closeSettingsButton;

    private PlayerController player;
    private bool isSettingsOpen = false;

    private void Start()
    {
        // Find PlayerController in the scene
        player = Object.FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController not found in scene by StartMenuManager!");
            return;
        }

        // Initialize UI Elements
        if (delaySlider != null)
        {
            delaySlider.minValue = -1.00f;
            delaySlider.maxValue = 1.00f;
            delaySlider.value = player.audioDelay;
            delaySlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Bind Button Actions
        if (delayMenuButton != null) delayMenuButton.onClick.AddListener(ToggleSettingsPanel);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettingsPanel);
        if (minusButton != null) minusButton.onClick.AddListener(DecreaseDelay);
        if (plusButton != null) plusButton.onClick.AddListener(IncreaseDelay);

        // Set initial states
        UpdateDelayText(player.audioDelay);
        if (delaySettingsPanel != null) delaySettingsPanel.SetActive(false);
        if (startCanvasGroup != null) startCanvasGroup.SetActive(true);

        player.isSettingsUIOpen = false;
    }

    private void Update()
    {
        if (player == null) return;

        // Once the player starts playing, automatically hide the entire Start Canvas
        var isPlayingField = player.GetType().GetField("isPlaying", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool isPlaying = false;
        if (isPlayingField != null)
        {
            isPlaying = (bool)isPlayingField.GetValue(player);
        }

        if (isPlaying && startCanvasGroup != null && startCanvasGroup.activeSelf)
        {
            startCanvasGroup.SetActive(false);
        }
    }

    private void ToggleSettingsPanel()
    {
        isSettingsOpen = !isSettingsOpen;
        ApplyPanelState();
    }

    private void CloseSettingsPanel()
    {
        isSettingsOpen = false;
        ApplyPanelState();
    }

    private void ApplyPanelState()
    {
        if (delaySettingsPanel != null)
        {
            delaySettingsPanel.SetActive(isSettingsOpen);
        }

        // Notify the player whether the settings are open, to block starting the game on click
        if (player != null)
        {
            player.isSettingsUIOpen = isSettingsOpen;
        }
    }

    private void OnSliderChanged(float value)
    {
        // Round to 2 decimal places (0.01s precision)
        float roundedValue = Mathf.Round(value * 100f) / 100f;
        
        // Prevent infinite loop when we force snap the slider to 0.01 precision
        if (Mathf.Abs(delaySlider.value - roundedValue) > 0.001f)
        {
            delaySlider.value = roundedValue;
        }

        if (player != null)
        {
            player.audioDelay = roundedValue;
        }

        UpdateDelayText(roundedValue);
    }

    private void DecreaseDelay()
    {
        if (delaySlider != null)
        {
            float targetValue = Mathf.Clamp(delaySlider.value - 0.01f, delaySlider.minValue, delaySlider.maxValue);
            delaySlider.value = (float)System.Math.Round(targetValue, 2);
        }
    }

    private void IncreaseDelay()
    {
        if (delaySlider != null)
        {
            float targetValue = Mathf.Clamp(delaySlider.value + 0.01f, delaySlider.minValue, delaySlider.maxValue);
            delaySlider.value = (float)System.Math.Round(targetValue, 2);
        }
    }

    private void UpdateDelayText(float value)
    {
        string sign = value > 0 ? "+" : "";
        string text = $"Delay: {sign}{value:F2}s";
        
        if (delayValueText != null)
        {
            delayValueText.text = text;
        }
        if (delayMenuButtonText != null)
        {
            delayMenuButtonText.text = $"{text}";
        }
    }
}
