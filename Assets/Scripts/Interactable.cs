using UnityEngine;
using UnityEngine.UI;

public enum InteractableType { Generic, Door, Lantern }

public class Interactable : MonoBehaviour
{
    public InteractableType type = InteractableType.Generic;

    [Header("Door Settings")]
    public Transform doorTarget;
    [Tooltip("If true, this door can only be used once and will lock after use")]
    public bool oneTimeUse = false;
    [Tooltip("The door that gets enabled when this door locks (optional)")]
    public Interactable doorToEnableOnLock;
    [Tooltip("The door that should be locked when this door is used (optional)")]
    public Interactable doorToLockOnUse;
    private bool isLocked = false;

    [Header("Lantern Settings")]
    [Tooltip("The UI Image component to enable when the lantern is picked up")]
    public Image lanternUIImage;
    [Tooltip("Light to enable when lantern is picked up (optional)")]
    public Light lightToEnable;
    [Tooltip("Light to disable when lantern is picked up (optional)")]
    public Light lightToDisable;

    public bool IsLocked
    {
        get { return isLocked; }
    }

    /// <summary>
    /// Call this method when the player uses the door
    /// Returns true if the door was successfully used, false if locked
    /// </summary>
    public bool UseDoor()
    {
        if (type != InteractableType.Door)
        {
            Debug.LogWarning("UseDoor called on non-door interactable!");
            return false;
        }

        if (isLocked)
        {
            Debug.Log("This door is locked!");
            return false;
        }

        // Handle one-time use logic
        if (oneTimeUse)
        {
            LockDoor();

            // Enable the linked door if specified
            if (doorToEnableOnLock != null)
            {
                doorToEnableOnLock.UnlockDoor();
            }

            // Lock the specified door if set
            if (doorToLockOnUse != null)
            {
                doorToLockOnUse.LockDoor();
            }
        }

        return true;
    }

    /// <summary>
    /// Call this method when the player interacts with a lantern
    /// Returns true if successfully picked up
    /// </summary>
    public bool PickupLantern()
    {
        if (type != InteractableType.Lantern)
        {
            Debug.LogWarning("PickupLantern called on non-lantern interactable!");
            return false;
        }

        if (lanternUIImage == null)
        {
            Debug.LogError("Lantern UI Image is not assigned!");
            return false;
        }

        // Enable the UI image
        lanternUIImage.enabled = true;
        Debug.Log("Lantern picked up!");

        // Handle light switching
        if (lightToEnable != null)
        {
            lightToEnable.enabled = true;
            Debug.Log($"Enabled light: {lightToEnable.gameObject.name}");
        }

        if (lightToDisable != null)
        {
            lightToDisable.enabled = false;
            Debug.Log($"Disabled light: {lightToDisable.gameObject.name}");
        }

        // Destroy this game object
        Destroy(gameObject);

        return true;
    }

    /// <summary>
    /// Locks this door, preventing further use
    /// </summary>
    public void LockDoor()
    {
        isLocked = true;
        Debug.Log($"Door {gameObject.name} is now locked");
    }

    /// <summary>
    /// Unlocks this door, allowing it to be used
    /// </summary>
    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log($"Door {gameObject.name} is now unlocked");
    }
}