using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Default Settings")]
    public float defaultMasterVolume = 1f;
    public float defaultMouseSensitivity = 200f;

    // Current settings
    private float masterVolume;
    private float fov;
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
        }
    }

    public float MouseSensitivity
    {
        get => mouseSensitivity;
        set
        {
            mouseSensitivity = Mathf.Clamp(value, 150f, 800f);
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
            ApplyMouseSensitivity();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyAllSettings();
    }

    void OnEnable()
    {
        ApplyAllSettings();
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
    }

    public void ApplyAllSettings()
    {
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