using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Awake()
    {
        // Singleton
        Instance = this;

        // Make sure fadeImage is transparent and disabled at start
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.enabled = false;
    }

    /// <summary>
    /// Fades to black, teleports player, then fades back.
    /// Locks movement and mouse look during the process.
    /// </summary>
    /// <param name="player">Player Transform</param>
    /// <param name="target">Destination Transform</param>
    public void FadeAndTeleport(Transform player, Transform target)
    {
        StartCoroutine(FadeRoutine(player, target));
    }

    private IEnumerator FadeRoutine(Transform player, Transform target)
    {
        //  Lock movement and look 
        PlayerController pc = player.GetComponent<PlayerController>();
        MouseLook ml = player.GetComponentInChildren<MouseLook>();

        if (pc != null) pc.movementLocked = true;
        if (ml != null) ml.lookLocked = true;

        //  Enable fade image 
        fadeImage.enabled = true;

        //  Fade in to black 
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black;

        //  Disable CharacterController to prevent teleport issues 
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        //  Teleport 
        player.position = target.position;
        player.rotation = target.rotation;

        //  Re-enable CharacterController 
        if (cc != null) cc.enabled = true;

        //  Fade out from black 
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            fadeImage.color = new Color(0, 0, 0, 1 - (t / fadeDuration));
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.enabled = false;

        //  Unlock movement and look 
        if (pc != null) pc.movementLocked = false;
        if (ml != null) ml.lookLocked = false;
    }
}
