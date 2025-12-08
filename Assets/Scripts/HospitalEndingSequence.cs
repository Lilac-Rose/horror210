using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HospitalEndingSequence : MonoBehaviour
{
    [Header("References")]
    [Tooltip("UI Image that covers the screen (should be black)")]
    public Image fadeToBlackImage;

    [Header("Camera Controls")]

    [Tooltip("Camera sensitivity during the sequence")]
    public float cameraSensitivity = 2f;

    [Tooltip("Camera rotation limits in degrees (minY, maxY)")]
    public Vector2 cameraRotationLimits = new Vector2(-30f, 30f);

    [Header("Timing Settings")]
    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 2f;

    [Tooltip("How long player can look around before cutting to black (seconds)")]
    public float lookAroundDuration = 10f;

    [Header("Ending Settings")]
    [Tooltip("Load next scene after black screen (optional)")]
    public string nextSceneName = "";

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

            // Store initial camera rotation
            if (Camera.main != null)
            {
                initialCameraRotation = Camera.main.transform.localRotation;
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
    }

    private void HandleCameraLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * cameraSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity;

        // Calculate new rotation
        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;

        // Clamp vertical rotation
        cameraRotationX = Mathf.Clamp(cameraRotationX, cameraRotationLimits.x, cameraRotationLimits.y);

        // Apply rotation to camera
        if (Camera.main != null)
        {
            Quaternion rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0f);
            Camera.main.transform.localRotation = initialCameraRotation * rotation;
        }
    }
}