using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
    [Header("Activation Settings")]
    [Tooltip("The GameObject(s) to activate when player enters trigger")]
    public GameObject[] objectsToActivate;

    [Tooltip("Tag required to trigger (leave empty for any object)")]
    public string requiredTag = "Player";

    [Tooltip("Only trigger once, then disable")]
    public bool triggerOnce = true;

    [Tooltip("Deactivate objects instead of activating them")]
    public bool deactivateInstead = false;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if already triggered and set to trigger once
        if (triggerOnce && hasTriggered)
            return;

        // Check if the object has the required tag
        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            // Activate/Deactivate all assigned objects
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(!deactivateInstead);
                }
            }

            hasTriggered = true;

            Debug.Log($"ObjectActivator triggered! Objects {(deactivateInstead ? "deactivated" : "activated")}.");
        }
    }
}