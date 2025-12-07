using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Default Settings")]
    public float defaultMasterVolume = 1f;
    public float defaultFOV = 60f;
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

    public float FOV
    {
        get => fov;
        set
        {
            fov = Mathf.Clamp(value, 30f, 120f);
            PlayerPrefs.SetFloat("FOV", fov);
            ApplyFOV();
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
            DontDestroyOnLoad(gameObject);
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
        fov = PlayerPrefs.GetFloat("FOV", defaultFOV);
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity);
    }

    public void ApplyAllSettings()
    {
        AudioListener.volume = masterVolume;
        ApplyFOV();
        ApplyMouseSensitivity();
    }

    private void ApplyFOV()
    {
        MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
        if (mouseLook != null)
        {
            Camera cam = mouseLook.GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = fov;
            }
        }
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
        FOV = defaultFOV;
        MouseSensitivity = defaultMouseSensitivity;
    }
}