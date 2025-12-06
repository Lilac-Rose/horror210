using UnityEngine;
using TMPro;
using System.Collections;

public class TextTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [TextArea(3, 10)]
    public string[] textLines; // Changed to array for multiple lines
    public float baseDisplayTime = 2f;
    public float timePerCharacter = 0.05f;
    public float timeBetweenLines = 0.5f; // Delay between each line

    [Header("UI References")]
    public TextMeshProUGUI textUI;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;

    [Header("Trigger Mode")]
    public bool autoTriggerOnEnter = true;

    public bool oneTimeOnly = true;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool isDisplaying = false;
    private bool hasTriggered = false;
    private Coroutine displayCoroutine;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        else if (textUI != null)
        {
            textUI.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (autoTriggerOnEnter && other.CompareTag(playerTag))
        {
            TriggerText();
        }
    }

    public void TriggerText()
    {
        if (isDisplaying || (oneTimeOnly && hasTriggered))
        {
            return;
        }

        hasTriggered = true;

        if(displayCoroutine != null)
        {
            StopCoroutine (displayCoroutine);
        }

        displayCoroutine = StartCoroutine(DisplayAllLines());
    }

    private IEnumerator DisplayAllLines()
    {
        isDisplaying = true;

        // Loop through each line of text
        for (int i = 0; i < textLines.Length; i++)
        {
            yield return StartCoroutine(DisplaySingleLine(textLines[i]));

            // Add delay between lines (except after the last line)
            if (i < textLines.Length - 1)
            {
                yield return new WaitForSeconds(timeBetweenLines);
            }
        }

        isDisplaying = false;
    }

    private IEnumerator DisplaySingleLine(string line)
    {
        if (textUI != null)
        {
            textUI.text = line;
        }

        float displayDuration = baseDisplayTime + (line.Length * timePerCharacter);

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeInDuration));

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeOutDuration));
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = end;
        }
        else if (textUI != null)
        {
            textUI.gameObject.SetActive(end > 0.3f);
        }
    }
}