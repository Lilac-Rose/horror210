using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SceneIntroController : MonoBehaviour
{
    [Header("Fade")]
    public Image blackImage;
    public float fadeDuration = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public float delayBeforeSound = 2f;
    public AudioClip sheetsRustlingSound;

    [Header("Camera")]
    public Camera introCamera;
    public GameObject playerController; //Player game object

    [Header("Animation")]
    public Animator introCameraAnimator; //The animator with the animation clip
    public AnimationClip wakeUpClip; // the actual animation clip

    [Header("Fog")]
    public UniversalRendererData rendererData;
    private PlayerFogFeature fogFeature;

    void Start()
    {
        // Ensure player is off during intro
        playerController.SetActive(false);
        // Ensure intro camera is active
        introCamera.gameObject.SetActive(true);

        // Find the fog feature and set initial radius
        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is PlayerFogFeature)
                {
                    fogFeature = feature as PlayerFogFeature;
                    fogFeature.fogRadius = 20f; // Start with large radius
                    break;
                }
            }
        }

        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // 1. Wait before sound
        yield return new WaitForSeconds(delayBeforeSound);

        // 2. Play sound
        if (audioSource != null) audioSource.Play();

        // Additional delay between sound and fade
        yield return new WaitForSeconds(2f);

        // 3. Fade from black
        yield return StartCoroutine(FadeFromBlack());

        // 4. Play sheets rustling sound and wake-up animation
        if (sheetsRustlingSound != null)
        {
            AudioSource.PlayClipAtPoint(sheetsRustlingSound, introCamera.transform.position, 1f);
        }

        if (introCameraAnimator != null)
        {
            introCameraAnimator.SetTrigger("Play");
            // Wait for animation to finish
            yield return new WaitForSeconds(wakeUpClip.length);
        }

        // 5. Reduce fog radius before switching to player
        if (fogFeature != null)
        {
            fogFeature.fogRadius = 10f;
        }

        // 6. Switch cameras
        introCamera.gameObject.SetActive(false);
        playerController.SetActive(true);
    }

    IEnumerator FadeFromBlack()
    {
        float t = 0f;
        Color c = blackImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / fadeDuration;
            // Starts slow (groggy), speeds up (realization/surprise from knock), then completes
            float curvedTime = EaseOutCirc(normalizedTime);
            float alpha = Mathf.Lerp(1f, 0f, curvedTime);
            blackImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        // Ensure it's fully transparent at the end
        blackImage.color = new Color(c.r, c.g, c.b, 0f);
    }

    // Easing function: starts slow, then accelerates
    float EaseOutCirc(float x)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
    }
}