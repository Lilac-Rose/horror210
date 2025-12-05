using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class TextTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [TextArea(3, 10)]
    public string displayText = "Enter your text here";
    public float baseDisplayTime = 2f;
    public float timePerCharacter = 0.05f;

    [Header("UI References")]
    public TextMeshProUGUI textUI;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool isDisplaying = false;
    private bool hasTriggered = false;
    private Coroutine displayCorutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        if (other.CompareTag(playerTag) && !isDisplaying &&  !hasTriggered)
        {
            hasTriggered = true;
            if (displayCorutine != null)
            {
                StopCoroutine(displayCorutine);
            }
            displayCorutine = StartCoroutine(DisplayText());
        }
    }

    private IEnumerator DisplayText()
    {
        isDisplaying = true;

        if (textUI != null)
        {
            textUI.text = displayText;
        }

        float displayDuration = baseDisplayTime + (displayText.Length * timePerCharacter);

        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeOutDuration));

        isDisplaying = false;
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if(canvasGroup != null)
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
