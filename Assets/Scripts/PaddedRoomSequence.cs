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

    [Tooltip("Time delay before spawning object while player looks away (seconds)")]
    public float spawnDelay = 10f;

    [Tooltip("Angle from center of screen to trigger credits (degrees)")]
    public float centerDetectionAngle = 15f;

    [Header("Timing Settings")]
    [Tooltip("Duration of initial black screen before fade in (seconds)")]
    public float initialBlackScreenDuration = 5f;

    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 2f;

    [Header("Ending Settings")]
    [Tooltip("Time to wait on black screen before loading credits")]
    public float blackScreenDuration = 2f;

    private CanvasGroup canvasGroup;
    private bool objectSpawned = false;
    private float lookAwayTimer = 0f;
    private bool creditsTriggered = false;

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
            // Handle object spawning while player looks away
            if (!objectSpawned && objectToSpawn != null)
            {
                if (IsPlayerLookingAwayFromSpawnPoint())
                {
                    lookAwayTimer += Time.deltaTime;

                    if (lookAwayTimer >= spawnDelay)
                    {
                        SpawnObject();
                    }
                }
                else
                {
                    // Reset timer if player looks back
                    lookAwayTimer = 0f;
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
        if (mainCamera == null || objectToSpawn == null) return false;

        Vector3 directionToSpawnPoint = (objectToSpawn.transform.position - mainCamera.transform.position).normalized;
        float angle = Vector3.Angle(mainCamera.transform.forward, directionToSpawnPoint);

        // Player is looking away if angle is greater than a threshold (e.g., 60 degrees)
        return angle > 60f;
    }

    private bool IsPlayerLookingAtObject()
    {
        if (mainCamera == null || objectToSpawn == null) return false;

        Vector3 directionToObject = (objectToSpawn.transform.position - mainCamera.transform.position).normalized;
        float angle = Vector3.Angle(mainCamera.transform.forward, directionToObject);

        // Player is looking at object if it's near the center of their view
        return angle <= centerDetectionAngle;
    }

    private void SpawnObject()
    {
        objectSpawned = true;
        objectToSpawn.SetActive(true);
        Debug.Log("Object spawned after player looked away for " + spawnDelay + " seconds");
    }

    private void TriggerCredits()
    {
        creditsTriggered = true;
        Debug.Log("Player looked at object - triggering credits");
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

        Debug.Log("Player controls enabled in padded room - waiting for player to look away");
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