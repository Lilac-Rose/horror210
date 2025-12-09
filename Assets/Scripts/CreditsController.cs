using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The three title card images that appear with each knock")]
    public Image titleImage1;
    public Image titleImage2;
    public Image titleImage3;

    [Tooltip("The scrolling credits container")]
    public RectTransform creditsContainer;

    [Header("Audio")]
    [Tooltip("The knock sound effect")]
    public AudioClip knockSound;

    [Tooltip("Optional background music for credits")]
    public AudioClip creditsMusic;

    [Header("Timing Settings")]
    [Tooltip("Delay between each knock (seconds)")]
    public float knockInterval = 0.3f;

    [Tooltip("How long to display all three title images after final knock (seconds)")]
    public float titleDisplayDuration = 1f;

    [Tooltip("Speed at which credits scroll (units per second)")]
    public float scrollSpeed = 50f;

    [Tooltip("How long to wait after credits finish before returning to menu (seconds)")]
    public float endDelay = 2f;

    [Header("Scene")]
    [Tooltip("Name of the main menu scene to return to")]
    public string mainMenuSceneName = "MainMenu";

    private AudioSource audioSource;
    private bool creditsStarted = false;

    void Start()
    {
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D sound

        // Make sure all title images start hidden
        if (titleImage1 != null) titleImage1.enabled = false;
        if (titleImage2 != null) titleImage2.enabled = false;
        if (titleImage3 != null) titleImage3.enabled = false;

        // Make sure credits are hidden
        if (creditsContainer != null)
        {
            creditsContainer.gameObject.SetActive(false);
        }

        // Start the credits sequence
        StartCoroutine(CreditsSequence());
    }

    void Update()
    {
        // Allow player to skip credits by pressing Escape or Space
        if (creditsStarted && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)))
        {
            ReturnToMenu();
        }
    }

    private IEnumerator CreditsSequence()
    {
        // Wait a moment before starting
        yield return new WaitForSeconds(0.5f);

        // Play 3 knocks with title images appearing
        yield return StartCoroutine(PlayKnocksWithImages());

        // Hold all three images on screen
        yield return new WaitForSeconds(titleDisplayDuration);

        // Move title images into the credits container so they scroll with credits
        if (titleImage1 != null) titleImage1.transform.SetParent(creditsContainer, true);
        if (titleImage2 != null) titleImage2.transform.SetParent(creditsContainer, true);
        if (titleImage3 != null) titleImage3.transform.SetParent(creditsContainer, true);

        // Show and start scrolling credits (with title images now inside)
        if (creditsContainer != null)
        {
            creditsContainer.gameObject.SetActive(true);
            creditsStarted = true;

            // Play credits music if available
            if (creditsMusic != null)
            {
                audioSource.clip = creditsMusic;
                audioSource.loop = false;
                audioSource.Play();
            }

            yield return StartCoroutine(ScrollCredits());
        }

        // Wait before returning to menu
        yield return new WaitForSeconds(endDelay);

        // Return to main menu
        ReturnToMenu();
    }

    private IEnumerator PlayKnocksWithImages()
    {
        // First knock - show first image
        if (knockSound != null)
        {
            audioSource.PlayOneShot(knockSound);
        }
        if (titleImage1 != null)
        {
            titleImage1.enabled = true;
        }
        yield return new WaitForSeconds(knockInterval);

        // Second knock - show second image
        if (knockSound != null)
        {
            audioSource.PlayOneShot(knockSound);
        }
        if (titleImage2 != null)
        {
            titleImage2.enabled = true;
        }
        yield return new WaitForSeconds(knockInterval);

        // Third knock - show third image
        if (knockSound != null)
        {
            audioSource.PlayOneShot(knockSound);
        }
        if (titleImage3 != null)
        {
            titleImage3.enabled = true;
        }
    }

    private IEnumerator ScrollCredits()
    {
        if (creditsContainer == null) yield break;

        // Get the starting position and calculate end position
        float startY = creditsContainer.anchoredPosition.y;

        // Calculate how far to scroll (until credits are off screen)
        // This assumes credits start below screen and scroll upward
        RectTransform canvasRect = creditsContainer.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float canvasHeight = canvasRect.rect.height;
        float creditsHeight = creditsContainer.rect.height;

        // Scroll until all credits have passed the top of the screen
        float targetY = canvasHeight + creditsHeight;

        while (creditsContainer.anchoredPosition.y < targetY)
        {
            // Move credits upward
            Vector2 pos = creditsContainer.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;
            creditsContainer.anchoredPosition = pos;

            yield return null;
        }
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}