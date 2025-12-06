using UnityEngine;
using UnityEngine.UI;

public enum InteractableType { Generic, Door, Lantern, Window }

public class Interactable : MonoBehaviour
{
    public InteractableType type = InteractableType.Generic;

    [Header("Door Settings")]
    public Transform doorTarget;
    public bool oneTimeUse = false;
    public Interactable doorToEnableOnLock;
    public Interactable doorToLockOnUse;
    public bool requiresLantern = false;
    public TextTrigger lockedMessageTrigger;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    [Header("Jammed Door Settings")]
    public bool isJammedDoor = false;
    public AudioClip jammedDoorSound;
    [Tooltip("Object to delete when jammed door is interacted with")]
    public GameObject objectToDeleteOnJammed;

    [Header("Window Settings")]
    public AudioClip windowLockSound;

    private bool isLocked = false;
    private static bool jammedDoorChecked = false;

    // Lantern and window stuff
    private bool isWindowLocked = false;
    private static bool hasLantern = false;
    private static int totalWindows = 0;
    private static int lockedWindows = 0;
    private static bool allWindowsLocked = false;

    public bool IsLocked => isLocked;
    public bool IsWindowLocked => isWindowLocked;
    public static bool HasLantern => hasLantern;
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
            Debug.Log("This door is jammed!");
            if (lockedMessageTrigger != null)
                lockedMessageTrigger.TriggerText();
            if (jammedDoorSound != null)
                AudioSource.PlayClipAtPoint(jammedDoorSound, transform.position, 1f);
            jammedDoorChecked = true;
            if (objectToDeleteOnJammed != null)
            {
                Destroy(objectToDeleteOnJammed);
                Debug.Log($"Deleted object {objectToDeleteOnJammed.name} due to jammed door interaction.");
            }
            return false;
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
        lockedWindows = 0;
        allWindowsLocked = false;
        jammedDoorChecked = false;
    }
}