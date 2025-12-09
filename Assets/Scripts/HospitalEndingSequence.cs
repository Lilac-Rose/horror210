using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HospitalEndingSequence : MonoBehaviour
{
    [Header("References")]
    [Tooltip("UI Image that covers the screen (should be black)")]
    public Image fadeToBlackImage;

    [Tooltip("Main camera to control (leave empty to auto-find)")]
    public Camera playerCamera;

    [Header("Camera Controls")]
    [Tooltip("Camera sensitivity during the sequence")]
    public float cameraSensitivity = 2f;

    [Tooltip("Camera rotation limits in degrees - 30 degrees in all directions")]
    public Vector2 cameraRotationLimitsX = new Vector2(-30f, 30f);
    public Vector2 cameraRotationLimitsY = new Vector2(-30f, 30f);

    [Header("Timing Settings")]
    [Tooltip("Duration of initial black screen before fade in (seconds)")]
    public float initialBlackScreenDuration = 5f;

    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 2f;

    [Tooltip("How long player can look around before cutting to black (seconds)")]
    public float lookAroundDuration = 10f;

    [Header("Ending Settings")]
    [Tooltip("Load next scene after black screen")]
    public string nextSceneName = "Credits";

    [Tooltip("Time to wait on black screen before loading next scene")]
    public float blackScreenDuration = 3f;

    private CanvasGroup canvasGroup;
    private bool isLookingAround = false;

    // Camera look variables
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    private Quaternion initialCameraRotation;

    void Start()
    {
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

            // Find camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            // Store initial camera rotation
            if (playerCamera != null)
            {
                initialCameraRotation = playerCamera.transform.localRotation;
                Debug.Log("HospitalEndingSequence: Camera found and initialized");
            }
            else
            {
                Debug.LogError("HospitalEndingSequence: No camera found! Please assign playerCamera in Inspector.");
            }

            // Start the sequence
            StartCoroutine(FadeSequence());
        }
        else
        {
            Debug.LogError("HospitalEndingSequence: fadeToBlackImage not assigned!");
        }
    }

    void Update()
    {
        // Only process mouse input during the look around phase
        if (isLookingAround)
        {
            HandleCameraLook();
        }
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

        // 2. Enable looking around
        isLookingAround = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Camera look enabled - player can look around for " + lookAroundDuration + " seconds");

        // 3. Wait while player can look around (10 seconds)
        yield return new WaitForSeconds(lookAroundDuration);

        // 4. Stop looking around
        isLookingAround = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 5. Hard cut to black (instant)
        canvasGroup.alpha = 1f;

        Debug.Log("Hospital ending sequence complete - black screen");

        // 6. Wait on black screen, then load Credits
        yield return new WaitForSeconds(blackScreenDuration);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("Loading Credits scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private void HandleCameraLook()
    {
        if (playerCamera == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * cameraSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity;

        // Calculate new rotation
        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;

        // Clamp both horizontal and vertical rotation to 30 degrees
        cameraRotationX = Mathf.Clamp(cameraRotationX, cameraRotationLimitsX.x, cameraRotationLimitsX.y);
        cameraRotationY = Mathf.Clamp(cameraRotationY, cameraRotationLimitsY.x, cameraRotationLimitsY.y);

        // Apply rotation to camera
        Quaternion rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0f);
        playerCamera.transform.localRotation = initialCameraRotation * rotation;
    }
}