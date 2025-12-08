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

    [Header("Timing Settings")]
    [Tooltip("Duration of fade in from black (seconds)")]
    public float fadeInDuration = 2f;

    [Tooltip("How long to wait before cutting to black (seconds)")]
    public float waitDuration = 3f;

    void Start()
    {
        // Ensure the fade image is set up correctly
        if (fadeToBlackImage != null)
        {
            // Make sure it covers the whole screen
            fadeToBlackImage.enabled = true;

            // Ensure it has a CanvasGroup for smooth fading
            CanvasGroup canvasGroup = fadeToBlackImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = fadeToBlackImage.gameObject.AddComponent<CanvasGroup>();

            // Start fully black
            canvasGroup.alpha = 1f;

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
        CanvasGroup canvasGroup = fadeToBlackImage.GetComponent<CanvasGroup>();

        // 1. Fade in from black
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // 2. Wait (player can see the room)
        yield return new WaitForSeconds(waitDuration);

        // 3. Hard cut to black (instant)
        canvasGroup.alpha = 1f;

        // Sequence complete - stays on black screen
        Debug.Log("PaddedRoom sequence complete");
    }
}