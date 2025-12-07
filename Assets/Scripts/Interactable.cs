using UnityEngine;
using UnityEngine.UI;

public enum InteractableType { Generic, Door, Lantern, Window, Photo, Crowbar, BathroomSink }

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

    [Header("Lantern Settings")]
    public AudioClip lanternPickupSound;

    [Header("Bathroom Sink Settings")]
    public AudioClip faucetOnSound;
    public AudioClip waterSplashingSound;
    public AudioClip faucetOffSound;
    public float faucetOnDuration = 0.5f;
    public float splashingDuration = 3f;
    public float faucetOffDuration = 0.5f;
    public bool requiresCrowbar = true;

    [Header("Window Settings")]
    public AudioClip windowLockSound;

    [Header("Photo Settings")]
    public Image photoUIImage;
    public TextTrigger photoDialogueTrigger;
    public float photoFadeInDuration = 0.5f;
    public float photoDisplayDuration = 2f;
    public float photoFadeOutDuration = 0.5f;
    public AudioClip photoPickupSound;

    private bool isLocked = false;
    private static bool jammedDoorChecked = false;

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
            totalWindows++;

        if (type == InteractableType.Door && startLocked)
            isLocked = true;
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

        if (isJammedDoor)
        {
            if (hasCrowbar)
            {
                if (unjamDoorSound != null)
                    AudioSource.PlayClipAtPoint(unjamDoorSound, transform.position);

                isJammedDoor = false;
                jammedDoorChecked = true;
                return false;
            }
            else
            {
                if (lockedMessageTrigger != null)
                    lockedMessageTrigger.TriggerText();

                if (jammedDoorSound != null)
                    AudioSource.PlayClipAtPoint(jammedDoorSound, transform.position);

                if (objectToDeleteOnJammed != null)
                    Destroy(objectToDeleteOnJammed);

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

        return true;
    }

    public bool PickupLantern()
    {
        if (type != InteractableType.Lantern) return false;

        hasLantern = true;

        if (lanternPickupSound != null)
            AudioSource.PlayClipAtPoint(lanternPickupSound, transform.position);

        Destroy(gameObject);
        return true;
    }

    public bool PickupCrowbar()
    {
        if (type != InteractableType.Crowbar) return false;

        hasCrowbar = true;

        if (crowbarPickupTrigger != null)
            crowbarPickupTrigger.TriggerText();

        Destroy(gameObject);
        return true;
    }

    public bool UseBathroomSink()
    {
        if (type != InteractableType.BathroomSink) return false;

        if (requiresCrowbar && !hasCrowbar)
        {
            if (lockedMessageTrigger != null)
                lockedMessageTrigger.TriggerText();
            return false;
        }

        return true;
    }

    public bool LockWindow()
    {
        if (type != InteractableType.Window) return false;
        if (isWindowLocked) return false;

        isWindowLocked = true;
        lockedWindows++;

        if (lockedWindows >= totalWindows)
            allWindowsLocked = true;

        if (windowLockSound != null)
            AudioSource.PlayClipAtPoint(windowLockSound, transform.position);

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
