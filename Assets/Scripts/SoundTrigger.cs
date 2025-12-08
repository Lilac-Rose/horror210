using UnityEngine;
using System.Collections;

public class SoundTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The SoundPlayer objects to trigger")]
    public SoundPlayer[] soundPlayers;
    [Tooltip("Tag required to trigger (leave empty for any object)")]
    public string requiredTag = "Player";
    [Tooltip("Only trigger once, then disable")]
    public bool triggerOnce = false;

    [Header("Light Range Settings")]
    [Tooltip("Optional: Light to change range when triggered")]
    public Light lightToChange;
    [Tooltip("Target light spot")]
    public float targetLightRange = 100f;
    [Tooltip("Duration of light range change in seconds")]
    public float lightChangeDuration = 2f;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if already triggered and set to trigger once
        if (triggerOnce && hasTriggered)
            return;

        // Check if the object has the required tag
        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            // Play all assigned sounds
            foreach (SoundPlayer player in soundPlayers)
            {
                if (player != null)
                {
                    player.PlaySound();
                }
            }

            // Change light intensity if assigned
            if (lightToChange != null)
            {
                StartCoroutine(ChangeLightIntensity());
            }

            hasTriggered = true;
        }
    }

    private IEnumerator ChangeLightIntensity()
    {
        float startRange = lightToChange.range;
        float elapsed = 0f;

        while (elapsed < lightChangeDuration)
        {
            elapsed += Time.deltaTime;
            lightToChange.range = Mathf.Lerp(startRange, targetLightRange, elapsed / lightChangeDuration);
            yield return null;
        }

        lightToChange.range = targetLightRange;
    }
}