using UnityEngine;
public class PauseMenuController : MonoBehaviour
{
    public Canvas optionsCanvas;
    public MouseLook mouseLook;
    public GameObject lanternObject;
    private bool isPaused = false;
    private bool wasLanternActive = false;
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