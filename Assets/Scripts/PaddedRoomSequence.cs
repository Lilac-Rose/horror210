using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PaddedRoomSequence : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main camera in the scene")]
    public Camera mainCamera;

    [Tooltip("UI Image that covers the screen (should be black)")]
    public Image fadeToBlackImage;

    [Header("Player References")]
    [Tooltip("Reference to the player controller")]
    public PlayerController playerController;

    [Tooltip("Reference to the mouse look component")]
    public MouseLook mouseLook;

    [Header("Object Spawn Settings")]
    [Tooltip("Object to spawn after player looks away")]
    public GameObject objectToSpawn;

    [Tooltip("Time after player gets control before checking if looking away (seconds)")]
    public float spawnDelay = 10f;

    [Tooltip("Angle from center of screen to trigger credits (degrees)")]
    public float centerDetectionAngle = 40f;

    [Header("Timing Settings")]
    [Tooltip("Duration of initial black screen before fade in (seconds)")]
    public float initialBlackScreenDuration = 5f;

    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 2f;

    [Header("Ending Settings")]
    [Tooltip("Time to wait on black screen before loading credits")]
    public float blackScreenDuration = 2f;

    [Header("Debug Settings")]
    [Tooltip("Enable detailed debug logging")]
    public bool debugMode = true;

    private CanvasGroup canvasGroup;

    private bool objectSpawned = false;
    private float timeSinceControlEnabled = 0f;
    private bool creditsTriggered = false;
    private float debugLogTimer = 0f;
    private float debugLogInterval = 1f; // Log every second

    void Start()
    {
        // Auto-find player components if not assigned
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
                Debug.Log("PaddedRoomSequence: Auto-found PlayerController");
        }

        if (mouseLook == null)
        {
            mouseLook = FindFirstObjectByType<MouseLook>();
            if (mouseLook != null)
                Debug.Log("PaddedRoomSequence: Auto-found MouseLook");
        }

        // IMPORTANT: Re-enable components in case they were disabled in previous scene
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("PaddedRoomSequence: PlayerController enabled");
        }
        else
        {
            Debug.LogWarning("PaddedRoomSequence: PlayerController not found!");
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = true;
            Debug.Log("PaddedRoomSequence: MouseLook enabled");
        }
        else
        {
            Debug.LogWarning("PaddedRoomSequence: MouseLook not found!");
        }

        // Ensure the fade image is set up correctly
        if (fadeToBlackImage != null)
        {
            // Make sure it covers the whole screen
            fadeToBlackImage.enabled = true;

            // Ensure it has a CanvasGroup for smooth fading
            canvasGroup = fadeToBlackImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = fadeToBlackImage.gameObject.AddComponent<CanvasGroup>();

            // Start fully black
            canvasGroup.alpha = 1f;

            // Now disable player controls temporarily for the fade sequence
            if (playerController != null)
                playerController.enabled = false;
            if (mouseLook != null)
                mouseLook.enabled = false;

            // Make sure object starts disabled
            if (objectToSpawn != null)
                objectToSpawn.SetActive(false);

            // Auto-find camera if not assigned
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Start the sequence
            StartCoroutine(FadeSequence());
        }
        else
        {
            Debug.LogError("PaddedRoomSequence: fadeToBlackImage not assigned!");
        }
    }

    void Update()
    {
        if (creditsTriggered) return;

        // After player has control, check for object spawning and looking
        if (playerController != null && playerController.enabled)
        {
            // Increment time since control was enabled
            if (!objectSpawned)
            {
                timeSinceControlEnabled += Time.deltaTime;
            }

            // Debug logging at intervals
            if (debugMode)
            {
                debugLogTimer += Time.deltaTime;
                if (debugLogTimer >= debugLogInterval && !objectSpawned)
                {
                    debugLogTimer = 0f;
                    bool lookingAway = IsPlayerLookingAwayFromSpawnPoint();
                    Debug.Log($"[PaddedRoom] Time elapsed: {timeSinceControlEnabled:F2}s / {spawnDelay}s, Looking away: {lookingAway}");
                }
            }

            // Handle object spawning after delay if player is looking away
            if (!objectSpawned && objectToSpawn != null)
            {
                // Check if enough time has passed
                if (timeSinceControlEnabled >= spawnDelay)
                {
                    // Check if player is looking away at this moment
                    if (IsPlayerLookingAwayFromSpawnPoint())
                    {
                        SpawnObject();
                    }
                    else if (debugMode)
                    {
                        Debug.Log($"[PaddedRoom] {spawnDelay}s elapsed but player is looking at spawn point - waiting...");
                    }
                }
            }

            // Check if player is looking at the spawned object
            if (objectSpawned && objectToSpawn != null && objectToSpawn.activeSelf)
            {
                if (IsPlayerLookingAtObject())
                {
                    TriggerCredits();
                }
            }
        }
    }

    private bool IsPlayerLookingAwayFromSpawnPoint()
    {
        if (mainCamera == null || objectToSpawn == null)
        {
            if (debugMode)
                Debug.LogWarning("[PaddedRoom] Camera or object to spawn is null!");
            return false;
        }

        Vector3 directionToSpawnPoint = (objectToSpawn.transform.position - mainCamera.transform.position).normalized;
        float angle = Vector3.Angle(mainCamera.transform.forward, directionToSpawnPoint);

        // Player is looking away if angle is greater than a threshold (e.g., 60 degrees)
        bool isLookingAway = angle > 60f;

        if (debugMode && debugLogTimer == 0f) // Only log on debug interval
        {
            Debug.Log($"[PaddedRoom] Angle to spawn point: {angle:F1}° (looking away if > 60°) - Result: {isLookingAway}");
        }

        return isLookingAway;
    }

    private bool IsPlayerLookingAtObject()
    {
        if (mainCamera == null || objectToSpawn == null) return false;

        Vector3 directionToObject = (objectToSpawn.transform.position - mainCamera.transform.position).normalized;
        float angle = Vector3.Angle(mainCamera.transform.forward, directionToObject);

        // Player is looking at object if it's near the center of their view
        bool isLookingAt = angle <= centerDetectionAngle;

        if (debugMode && isLookingAt)
        {
            Debug.Log($"[PaddedRoom] Player looking at object! Angle: {angle:F1}° (trigger at <= {centerDetectionAngle}°)");
        }

        return isLookingAt;
    }

    private void SpawnObject()
    {
        objectSpawned = true;
        objectToSpawn.SetActive(true);

        if (debugMode)
        {
            Debug.Log($"[PaddedRoom] ✓ OBJECT SPAWNED at {timeSinceControlEnabled:F2}s (player was looking away)!");
        }
    }

    private void TriggerCredits()
    {
        creditsTriggered = true;

        if (debugMode)
        {
            Debug.Log("[PaddedRoom] ✓ CREDITS TRIGGERED - Player looked at spawned object!");
        }

        StartCoroutine(FadeToCredits());
    }

    private IEnumerator FadeSequence()
    {
        // 0. Wait on black screen for 5 seconds
        yield return new WaitForSeconds(initialBlackScreenDuration);

        // 1. Fade in from black
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // 2. Enable player controls
        if (playerController != null)
            playerController.enabled = true;
        if (mouseLook != null)
            mouseLook.enabled = true;

        // Lock and hide cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (debugMode)
        {
            Debug.Log("[PaddedRoom] ✓ Player controls enabled - starting spawn timer");
            if (objectToSpawn != null)
            {
                Debug.Log($"[PaddedRoom] Object to spawn: {objectToSpawn.name} at position {objectToSpawn.transform.position}");
                Debug.Log($"[PaddedRoom] Will check after {spawnDelay}s if player is looking away (angle > 60°)");
            }
            else
            {
                Debug.LogError("[PaddedRoom] ERROR: No object to spawn assigned!");
            }
        }
    }

    private IEnumerator FadeToCredits()
    {
        // Disable player controls
        if (playerController != null)
            playerController.enabled = false;
        if (mouseLook != null)
            mouseLook.enabled = false;

        // Unlock cursor before transitioning
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fade to black
        float elapsed = 0f;
        float fadeDuration = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait on black screen
        yield return new WaitForSeconds(blackScreenDuration);

        // Load credits scene
        Debug.Log("Loading Credits scene");
        SceneManager.LoadScene("Credits");
    }
}