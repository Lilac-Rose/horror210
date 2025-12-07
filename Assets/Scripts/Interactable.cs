using UnityEngine;
using UnityEngine.UI;

public enum InteractableType { Generic, Door, Lantern, Window, Photo, Crowbar }

public class Interactable : MonoBehaviour
{
    public InteractableType type = InteractableType.Generic;

    [Header("Door Settings")]
    public Transform doorTarget;
    public Transform postHouseSwitchTarget;
    public Transform postCrowbarPickupTarget;
    public bool oneTimeUse = false;
    public bool startLocked = false;
    public Interactable doorToEnableOnLock;
    public Interactable doorToLockOnUse;
    public bool requiresLantern = false;
    public TextTrigger lockedMessageTrigger;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    [Header("Jammed Door Settings")]
    public bool isJammedDoor = false;
    public AudioClip jammedDoorSound;
    public AudioClip unjamDoorSound;
    [Tooltip("Object to delete when jammed door is interacted with (without crowbar)")]
    public GameObject objectToDeleteOnJammed;

    [Header("Crowbar Settings")]
    public TextTrigger crowbarPickupTrigger;

    [Header("Window Settings")]
    public AudioClip windowLockSound;

    [Header("Photo Settings")]
    public Image photoUIImage;
    public TextTrigger photoDialogueTrigger;
    public float photoFadeInDuration = 0.5f;
    public float photoDisplayDuration = 2f;
    public float photoFadeOutDuration = 0.5f;

    private bool isLocked = false;
    private static bool jammedDoorChecked = false;

    // Lantern, window, and crowbar stuff
    private bool isWindowLocked = false;
    private static bool hasLantern = false;
    private static bool hasCrowbar = false;
    private static int totalWindows = 0;
    private static int lockedWindows = 0;
    private static bool allWindowsLocked = false;

    public bool IsLocked => isLocked;
    public bool IsWindowLocked => isWindowLocked;
    public static bool HasLantern => hasLantern;
    public static bool HasCrowbar => hasCrowbar;
    public static bool AllWindowsLocked => allWindowsLocked;
    public static bool JammedDoorChecked => jammedDoorChecked;
    public AudioClip DoorOpenSound => doorOpenSound;
    public AudioClip DoorCloseSound => doorCloseSound;

    void Awake()
    {
        if (type == InteractableType.Window)
        {
            totalWindows++;
        }

        // Set initial locked state for doors
        if (type == InteractableType.Door && startLocked)
        {
            isLocked = true;
        }
    }

    void OnDestroy()
    {
        if (type == InteractableType.Window)
        {
            totalWindows--;
            if (isWindowLocked)
                lockedWindows--;
        }
    }

    public bool UseDoor()
    {
        if (type != InteractableType.Door)
        {
            Debug.LogWarning("UseDoor called on non-door interactable!");
            return false;
        }

        // Check for jammed door first
        if (isJammedDoor)
        {
            // If player has crowbar, unjam the door
            if (hasCrowbar)
            {
                Debug.Log("Using crowbar to unjam the door!");
                if (unjamDoorSound != null)
                    AudioSource.PlayClipAtPoint(unjamDoorSound, transform.position, 1f);

                // Unjam the door so it can be used normally
                isJammedDoor = false;
                jammedDoorChecked = true;

                // Don't open the door yet, just unjam it
                // Player will need to interact again to open
                return false;
            }
            else
            {
                // Door is jammed and player doesn't have crowbar
                Debug.Log("This door is jammed!");

                // Trigger the locked message
                if (lockedMessageTrigger != null)
                    lockedMessageTrigger.TriggerText();

                // Play jammed sound
                if (jammedDoorSound != null)
                    AudioSource.PlayClipAtPoint(jammedDoorSound, transform.position, 1f);

                // Delete the specified object (like a wall)
                if (objectToDeleteOnJammed != null)
                {
                    Destroy(objectToDeleteOnJammed);
                    Debug.Log($"Deleted object {objectToDeleteOnJammed.name} due to jammed door interaction.");
                }

                jammedDoorChecked = true;
                return false;
            }
        }

        if (isLocked)
        {
            if (lockedMessageTrigger != null)
                lockedMessageTrigger.TriggerText();
            return false;
        }

        if (requiresLantern && !hasLantern)
        {
            if (lockedMessageTrigger != null)
                lockedMessageTrigger.TriggerText();
            return false;
        }

        // Determine which target to use
        Transform targetToUse = doorTarget;

        if (hasCrowbar && postCrowbarPickupTarget != null)
        {
            targetToUse = postCrowbarPickupTarget;
        }
        else if (allWindowsLocked && postHouseSwitchTarget != null)
        {
            targetToUse = postHouseSwitchTarget;
        }

        // If no valid target, trigger locked message and don't do anything
        if (targetToUse == null)
        {
            Debug.Log("Door has no valid teleport target!");
            if (lockedMessageTrigger != null)
                lockedMessageTrigger.TriggerText();
            return false;
        }

        if (oneTimeUse)
        {
            LockDoor();
            if (doorToEnableOnLock != null) doorToEnableOnLock.UnlockDoor();
            if (doorToLockOnUse != null) doorToLockOnUse.LockDoor();
        }

        return true;
    }

    public bool PickupLantern()
    {
        if (type != InteractableType.Lantern) return false;
        hasLantern = true;
        Destroy(gameObject);
        return true;
    }

    public bool PickupCrowbar()
    {
        if (type != InteractableType.Crowbar) return false;
        hasCrowbar = true;

        // Trigger text message if assigned
        if (crowbarPickupTrigger != null)
        {
            crowbarPickupTrigger.TriggerText();
        }

        Destroy(gameObject);
        return true;
    }

    public bool LockWindow()
    {
        if (type != InteractableType.Window) return false;
        if (isWindowLocked) return false;
        isWindowLocked = true;
        lockedWindows++;
        if (lockedWindows >= totalWindows) allWindowsLocked = true;

        // Play window lock sound
        if (windowLockSound != null)
        {
            AudioSource.PlayClipAtPoint(windowLockSound, transform.position, 1f);
        }

        return true;
    }

    public void LockDoor() => isLocked = true;
    public void UnlockDoor() => isLocked = false;

    public static void ResetAllFlags()
    {
        hasLantern = false;
        hasCrowbar = false;
        lockedWindows = 0;
        allWindowsLocked = false;
        jammedDoorChecked = false;
    }
}