using UnityEngine;
using UnityEngine.UI;
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

    [Header("Timing Settings")]
    [Tooltip("Duration of initial black screen before fade in (seconds)")]
    public float initialBlackScreenDuration = 5f;

    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 2f;

    [Tooltip("How long player has control before cutting to black (seconds)")]
    public float playerControlDuration = 10f;

    [Header("Ending Settings")]
    [Tooltip("Time to wait on black screen before loading next scene")]
    public float blackScreenDuration = 3f;

    private CanvasGroup canvasGroup;

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

            // Disable player controls initially
            if (playerController != null)
                playerController.enabled = false;

            if (mouseLook != null)
                mouseLook.enabled = false;

            // Start the sequence
            StartCoroutine(FadeSequence());
        }
        else
        {
            Debug.LogError("PaddedRoomSequence: fadeToBlackImage not assigned!");
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

        // 2. Enable player controls
        if (playerController != null)
            playerController.enabled = true;

        if (mouseLook != null)
            mouseLook.enabled = true;

        Debug.Log("Player controls enabled in padded room");

        // 3. Wait while player has control (10 seconds)
        yield return new WaitForSeconds(playerControlDuration);

        // 4. Disable player controls before cutting to black
        if (playerController != null)
            playerController.enabled = false;

        if (mouseLook != null)
            mouseLook.enabled = false;

        // 5. Hard cut to black (instant)
        canvasGroup.alpha = 1f;

        Debug.Log("PaddedRoom sequence complete - black screen");
    }
}