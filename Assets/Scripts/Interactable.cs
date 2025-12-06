using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public enum InteractableType { Generic, Door, Lantern, Window }

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
    [Tooltip("If true, this door requires the player to have picked up the lantern")]
    public bool requiresLantern = false;
    [Tooltip("TextTrigger to activate when door is locked/requires lantern")]
    public TextTrigger lockedMessageTrigger;
    private bool isLocked = false;

    [Header("Lantern Settings")]
    [Tooltip("The UI Image component to enable when the lantern is picked up")]
    public Image lanternUIImage;
    [Tooltip("Light to enable when lantern is picked up (optional)")]
    public Light lightToEnable;
    [Tooltip("Light to disable when lantern is picked up (optional)")]
    public Light lightToDisable;

    [Header("Window Settings")]
    [Tooltip("Sound to play when window is locked")]
    public AudioClip windowLockSound;
    [Tooltip("Volume for window lock sound")]
    public float windowLockVolume = 0.7f;

    private bool isWindowLocked = false;
    private static bool hasLantern = false;
    private static int totalWindows = 0;
    private static int lockedWindows = 0;
    private static bool allWindowsLocked = false;

    public bool IsLocked
    {
        get { return isLocked; }
    }

    public bool IsWindowLocked
    {
        get { return isWindowLocked; }
    }

    public static bool HasLantern
    {
        get { return hasLantern; }
    }

    public static bool AllWindowsLocked
    {
        get { return allWindowsLocked; }
    }

    void Awake()
    {
        // Count total windows in the scene
        if (type == InteractableType.Window)
        {
            totalWindows++;
        }
    }

    void OnDestroy()
    {
        // Decrease count if window is destroyed
        if (type == InteractableType.Window)
        {
            totalWindows--;
            if (isWindowLocked)
            {
                lockedWindows--;
            }
        }
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
            if (lockedMessageTrigger != null)
            {
                lockedMessageTrigger.TriggerText();
            }
            return false;
        }

        // Check if this door requires the lantern
        if (requiresLantern && !hasLantern)
        {
            Debug.Log("You need to find a light source before leaving...");
            if (lockedMessageTrigger != null)
            {
                lockedMessageTrigger.TriggerText();
            }
            return false; // Fixed: was missing this return false!
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

        // Set the static flag
        hasLantern = true;
        Debug.Log("Lantern picked up! hasLantern flag set to true");

        // Enable the UI image
        lanternUIImage.enabled = true;

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
    /// Call this method when the player locks a window
    /// Returns true if successfully locked
    /// </summary>
    public bool LockWindow()
    {
        if (type != InteractableType.Window)
        {
            Debug.LogWarning("LockWindow called on non-window interactable!");
            return false;
        }

        if (isWindowLocked)
        {
            Debug.Log("This window is already locked!");
            return false;
        }

        // Lock the window
        isWindowLocked = true;
        lockedWindows++;

        Debug.Log($"Window locked! ({lockedWindows}/{totalWindows})");

        // Play lock sound
        if (windowLockSound != null)
        {
            AudioSource.PlayClipAtPoint(windowLockSound, transform.position, windowLockVolume);
        }

        // Check if all windows are now locked
        if (lockedWindows >= totalWindows)
        {
            allWindowsLocked = true;
            Debug.Log("All windows are now locked!");
        }

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

    /// <summary>
    /// Reset all static flags (useful for testing or restarting game)
    /// </summary>
    public static void ResetAllFlags()
    {
        hasLantern = false;
        lockedWindows = 0;
        allWindowsLocked = false;
        Debug.Log("All flags reset");
    }
}