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
    private float debugLogInterval = 1f;
    private bool sequenceStarted = false;

    void Awake()
    {
        Debug.Log("[PaddedRoom] Awake called - Scene is loading");
    }

    void Start()
    {
        Debug.Log("[PaddedRoom] Start called - Initializing sequence");

        // CRITICAL: Find components in the NEW scene (not from old scene)
        if (playerController == null)
        {
            // Use FindAnyObjectByType which works better after scene load
            playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
                Debug.Log($"PaddedRoomSequence: Auto-found PlayerController on {playerController.gameObject.name}");
            else
                Debug.LogError("PaddedRoomSequence: FAILED to find PlayerController!");
        }

        if (mouseLook == null)
        {
            mouseLook = FindAnyObjectByType<MouseLook>();
            if (mouseLook != null)
                Debug.Log($"PaddedRoomSequence: Auto-found MouseLook on {mouseLook.gameObject.name}");
            else
                Debug.LogError("PaddedRoomSequence: FAILED to find MouseLook!");
        }

        // Auto-find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
                Debug.Log($"PaddedRoomSequence: Auto-found main camera: {mainCamera.gameObject.name}");
            else
                Debug.LogError("PaddedRoomSequence: FAILED to find camera!");
        }

        // CRITICAL: Force enable components in case they were disabled
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("PaddedRoomSequence: PlayerController force-enabled");
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = true;
            Debug.Log("PaddedRoomSequence: MouseLook force-enabled");
        }

        // Make sure camera is enabled and active
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            mainCamera.gameObject.SetActive(true);
            Debug.Log("PaddedRoomSequence: Camera enabled and active");
        }

        // Ensure the fade image is set up correctly
        if (fadeToBlackImage != null)
        {
            fadeToBlackImage.enabled = true;

            // Ensure it has a CanvasGroup for smooth fading
            canvasGroup = fadeToBlackImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = fadeToBlackImage.gameObject.AddComponent<CanvasGroup>();

            // Start fully black
            canvasGroup.alpha = 1f;
            Debug.Log("PaddedRoomSequence: Fade image set to black");

            // Temporarily disable player controls for fade sequence
            if (playerController != null)
                playerController.enabled = false;
            if (mouseLook != null)
                mouseLook.enabled = false;

            // Make sure object starts disabled
            if (objectToSpawn != null)
            {
                objectToSpawn.SetActive(false);
                Debug.Log($"PaddedRoomSequence: Object to spawn '{objectToSpawn.name}' disabled");
            }
            else
            {
                Debug.LogWarning("PaddedRoomSequence: No object to spawn assigned!");
            }

            // Start the sequence
            sequenceStarted = true;
            StartCoroutine(FadeSequence());
            Debug.Log("PaddedRoomSequence: Fade sequence started!");
        }
        else
        {
            Debug.LogError("PaddedRoomSequence: fadeToBlackImage not assigned! Sequence cannot start!");
        }
    }

    void Update()
    {
        if (!sequenceStarted || creditsTriggered) return;

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
                if (timeSinceControlEnabled >= spawnDelay)
                {
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

        bool isLookingAway = angle > 60f;

        if (debugMode && debugLogTimer == 0f)
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
        Debug.Log($"[PaddedRoom] Starting fade sequence - waiting {initialBlackScreenDuration}s on black screen");

        // 0. Wait on black screen
        yield return new WaitForSeconds(initialBlackScreenDuration);

        Debug.Log($"[PaddedRoom] Fading in over {fadeInDuration}s");

        // 1. Fade in from black
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        Debug.Log("[PaddedRoom] Fade in complete - enabling player controls");

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
        Debug.Log("[PaddedRoom] Fading to credits...");

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
        float fadeDuration = 2f;

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