using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The SoundPlayer objects to trigger")]
    public SoundPlayer[] soundPlayers;

    [Tooltip("Tag required to trigger (leave empty for any object)")]
    public string requiredTag = "Player";

    [Tooltip("Only trigger once, then disable")]
    public bool triggerOnce = false;

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

            hasTriggered = true;
        }
    }
}