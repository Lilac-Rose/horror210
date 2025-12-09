using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Default Settings")]
    public float defaultMasterVolume = 1f;
    public float defaultMouseSensitivity = 200f;

    // Current settings
    private float masterVolume;
    private float mouseSensitivity;

    // Property accessors
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.Save(); // Force save immediately
        }
    }

    public float MouseSensitivity
    {
        get => mouseSensitivity;
        set
        {
            mouseSensitivity = Mathf.Clamp(value, 150f, 800f);
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
            PlayerPrefs.Save(); // Force save immediately
            ApplyMouseSensitivity();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            LoadSettings();
            ApplyAllSettings(); // Apply once on creation
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
    }

    private void ApplyAllSettings()
    {
        // Use the property setters to ensure consistency
        AudioListener.volume = masterVolume;
        ApplyMouseSensitivity();
    }

    private void ApplyMouseSensitivity()
    {
        MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
        if (mouseLook != null)
        {
            mouseLook.mouseSensitivity = mouseSensitivity;
        }
    }

    public void ResetToDefaults()
    {
        MasterVolume = defaultMasterVolume;
        MouseSensitivity = defaultMouseSensitivity;
    }
}