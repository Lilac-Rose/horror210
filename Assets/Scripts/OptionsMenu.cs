using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider fovSlider;
    public Slider sensitivitySlider;

    [Header("Value Display (Optional)")]
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI fovText;
    public TextMeshProUGUI sensitivityText;

    [Header("Navigation")]
    public Button backButton;
    public TextMeshProUGUI backButtonText;

    [Header("Button Colors")]
    public Color normalColor = Color.black;
    public Color hoverColor = Color.gray;

    void Start()
    {
        LoadCurrentSettings();

        // Add listeners
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        fovSlider.onValueChanged.AddListener(OnFOVChanged);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToMainMenu);
        }

        if (backButton != null && backButtonText != null)
        {
            AddHoverListener(backButton, backButtonText);
        }
    }

    private void LoadCurrentSettings()
    {
        if (GameSettings.Instance != null)
        {
            volumeSlider.value = GameSettings.Instance.MasterVolume;
            fovSlider.value = GameSettings.Instance.FOV;
            sensitivitySlider.value = GameSettings.Instance.MouseSensitivity;

            UpdateDisplayTexts();
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.MasterVolume = value;
            UpdateDisplayTexts();
        }
    }

    private void OnFOVChanged(float value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.FOV = value;
            UpdateDisplayTexts();
        }
    }

    private void OnSensitivityChanged(float value)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.MouseSensitivity = value;
            UpdateDisplayTexts();
        }
    }

    private void UpdateDisplayTexts()
    {
        if (volumeText != null)
            volumeText.text = $"{Mathf.RoundToInt(GameSettings.Instance.MasterVolume * 100)}%";

        if (fovText != null)
            fovText.text = Mathf.RoundToInt(GameSettings.Instance.FOV).ToString();

        if (sensitivityText != null)
        {
            // Map 150-800 to 1-100
            float displayValue = Mathf.Lerp(1f, 100f, (GameSettings.Instance.MouseSensitivity - 150f) / 650f);
            sensitivityText.text = Mathf.RoundToInt(displayValue).ToString();
        }
    }

    private void AddHoverListener(Button button, TextMeshProUGUI text)
    {
        if (button == null || text == null) return;

        text.color = normalColor;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        // Pointer Enter (hover start)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            text.color = hoverColor;
        });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit (hover end)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            text.color = normalColor;
        });
        trigger.triggers.Add(entryExit);
    }

    public void ResetToDefaults()
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.ResetToDefaults();
            LoadCurrentSettings();
        }
    }

    public void GoBackToMainMenu()
    {
        // Check if we're in the main menu or in-game
        MenuController menuController = FindFirstObjectByType<MenuController>();
        if (menuController != null)
        {
            // We're in main menu
            menuController.CloseOptions();
        }
        else
        {
            // We're in-game, find pause menu controller
            PauseMenuController pauseController = FindFirstObjectByType<PauseMenuController>();
            if (pauseController != null)
            {
                pauseController.ResumeGame();
            }
        }
    }
}