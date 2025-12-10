using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public Canvas optionsCanvas;
    public MouseLook mouseLook;
    public GameObject lanternObject;
    private bool isPaused = false;
    private bool wasLanternActive = false;

    void Awake()
    {
        // Subscribe to scene loaded event to refresh references
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"PauseMenuController: Scene loaded - {scene.name}");

        // Clear old references when scene changes
        mouseLook = null;
        lanternObject = null;

        // For special scenes like PaddedRoom or Credits, disable pause menu entirely
        if (scene.name == "PaddedRoom" || scene.name == "Credits" || scene.name == "HospitalEnding")
        {
            Debug.Log("PauseMenuController: Disabling for special scene");
            this.enabled = false;
            if (optionsCanvas != null)
                optionsCanvas.enabled = false;
            return;
        }

        // Re-enable for gameplay scenes
        this.enabled = true;

        // Try to find new references in the new scene
        mouseLook = FindFirstObjectByType<MouseLook>();
        if (mouseLook != null)
            Debug.Log("PauseMenuController: Found new MouseLook component");

        // Reset pause state
        if (isPaused)
        {
            ResumeGame();
        }
    }

    void Start()
    {
        if (optionsCanvas != null)
        {
            optionsCanvas.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        if (optionsCanvas != null)
        {
            optionsCanvas.enabled = true;
        }

        // Hide lantern if it exists
        if (lanternObject != null)
        {
            wasLanternActive = lanternObject.activeSelf;
            lanternObject.SetActive(false);
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable mouse look
        if (mouseLook != null)
        {
            mouseLook.lookLocked = true;
        }

        Time.timeScale = 0f; // Pause the game AFTER enabling UI
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Unpause the game FIRST

        if (optionsCanvas != null)
        {
            optionsCanvas.enabled = false;
        }

        // Restore lantern state
        if (lanternObject != null && wasLanternActive)
        {
            lanternObject.SetActive(true);
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Enable mouse look
        if (mouseLook != null)
        {
            mouseLook.lookLocked = false;
        }
    }
}