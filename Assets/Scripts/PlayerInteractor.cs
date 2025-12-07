using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 5f;
    public Image interactImage;
    public Image lockImage;
    public Image lanternImage;
    public LayerMask interactMask;
    public Transform playerBody;

    [Header("UI Feedback")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("Teleport Settings")]
    public float firstNumber = -66.75f;
    public float secondNumber = 41f;

    private Interactable currentTarget;
    private bool isInteracting = false;
    private bool hasTeleported = false;

    void Update()
    {
        if (!isInteracting)
        {
            HandleLook();
            HandleInteraction();
        }
    }

    private void HandleLook()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
        {
            Interactable interact = hit.collider.GetComponent<Interactable>();
            if (interact != null)
            {
                currentTarget = interact;

                // Check if this is a locked/jammed door or locked window
                bool isLocked = false;
                if (interact.type == InteractableType.Door)
                {
                    isLocked = interact.IsLocked;
                }
                else if (interact.type == InteractableType.Window)
                {
                    isLocked = interact.IsWindowLocked;
                }

                if (isLocked)
                {
                    interactImage.enabled = false;
                    lockImage.enabled = true;
                }
                else
                {
                    interactImage.enabled = true;
                    interactImage.color = unlockedColor;
                    lockImage.enabled = false;
                }

                return;
            }
        }

        currentTarget = null;
        interactImage.enabled = false;
        lockImage.enabled = false;
    }

    private void HandleInteraction()
    {
        if (currentTarget == null) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        switch (currentTarget.type)
        {
            case InteractableType.Door: HandleDoorInteraction(); break;
            case InteractableType.Lantern: HandleLanternInteraction(); break;
            case InteractableType.Window: HandleWindowInteraction(); break;
            case InteractableType.Photo: HandlePhotoInteraction(); break;
            case InteractableType.Generic: Debug.Log("Interacted with generic object."); break;
        }
    }

    private void HandleDoorInteraction()
    {
        if (!currentTarget.UseDoor()) return;

        // Choose the correct door target based on whether windows are locked
        Transform doorTarget;
        if (Interactable.AllWindowsLocked && currentTarget.postHouseSwitchTarget != null)
        {
            doorTarget = currentTarget.postHouseSwitchTarget;
        }
        else
        {
            doorTarget = currentTarget.doorTarget;
        }

        // Play door open sound immediately
        if (currentTarget.DoorOpenSound != null)
        {
            AudioSource.PlayClipAtPoint(currentTarget.DoorOpenSound, transform.position, 0.6f);
        }

        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;

        // Store reference to door close sound for after teleport
        AudioClip doorCloseSound = currentTarget.DoorCloseSound;

        currentTarget = null;

        // Stop footsteps immediately
        PlayerController pc = playerBody.GetComponent<PlayerController>();
        pc?.StopFootsteps();

        ScreenFader.Instance.FadeAndTeleport(playerBody, doorTarget, doorCloseSound);
        StartCoroutine(ReEnableInteraction());
    }

    private void HandleLanternInteraction()
    {
        if (currentTarget.PickupLantern())
        {
            interactImage.enabled = false;
            lockImage.enabled = false;
            currentTarget = null;

            // Enable the lantern UI image
            if (lanternImage != null)
            {
                lanternImage.enabled = true;
            }
        }
    }

    private void HandleWindowInteraction()
    {
        if (currentTarget.LockWindow())
        {
            Debug.Log("Window locked!");
            if (Interactable.AllWindowsLocked && !hasTeleported)
            {
                Debug.Log("All windows secured!");
                hasTeleported = true;
                Vector3 newPos = playerBody.position;
                newPos.z += firstNumber - secondNumber;

                PlayerController pc = playerBody.GetComponent<PlayerController>();
                pc?.StopFootsteps();
                pc?.Teleport(newPos);
            }
        }
    }

    private void HandlePhotoInteraction()
    {
        if (currentTarget.photoUIImage == null)
        {
            Debug.LogWarning("Photo interactable has no UI image assigned!");
            return;
        }

        StartCoroutine(PhotoSequence(currentTarget));

        // Hide the photo object in the world
        currentTarget.gameObject.SetActive(false);
    }

    private IEnumerator PhotoSequence(Interactable photoInteractable)
    {
        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;

        // Stop player movement
        PlayerController pc = playerBody.GetComponent<PlayerController>();
        pc?.StopFootsteps();
        bool wasMovementEnabled = true;
        if (pc != null)
        {
            wasMovementEnabled = pc.enabled;
            pc.enabled = false;
        }

        // Stop mouse look
        MouseLook mouseLook = GetComponent<MouseLook>();
        bool wasMouseLookEnabled = false;
        if (mouseLook != null)
        {
            wasMouseLookEnabled = mouseLook.enabled;
            mouseLook.enabled = false;
        }

        // Get the canvas group from the photo UI image
        CanvasGroup photoCanvasGroup = photoInteractable.photoUIImage.GetComponent<CanvasGroup>();
        if (photoCanvasGroup == null)
        {
            Debug.LogWarning("Photo UI Image doesn't have a CanvasGroup component!");
            photoCanvasGroup = photoInteractable.photoUIImage.gameObject.AddComponent<CanvasGroup>();
        }

        // Setup photo display
        photoInteractable.photoUIImage.enabled = true;
        photoCanvasGroup.alpha = 0f;

        // Fade in
        float fadeInTime = photoInteractable.photoFadeInDuration;
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            photoCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInTime);
            yield return null;
        }
        photoCanvasGroup.alpha = 1f;

        // Wait for display duration
        yield return new WaitForSeconds(photoInteractable.photoDisplayDuration);

        // Teleport player during photo display
        Vector3 newPos = playerBody.position;
        float relativeZ = 184.5f - 110.5f;
        newPos.z += relativeZ;
        playerBody.position = newPos;

        // Trigger dialogue
        if (photoInteractable.photoDialogueTrigger != null)
        {
            photoInteractable.photoDialogueTrigger.TriggerText();
        }

        // Fade out
        float fadeOutTime = photoInteractable.photoFadeOutDuration;
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            photoCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            yield return null;
        }
        photoCanvasGroup.alpha = 0f;

        // Hide photo display
        photoInteractable.photoUIImage.enabled = false;

        // Re-enable player controls
        if (pc != null && wasMovementEnabled)
        {
            pc.enabled = true;
        }

        if (mouseLook != null && wasMouseLookEnabled)
        {
            mouseLook.enabled = true;
        }

        currentTarget = null;
        isInteracting = false;
    }

    private System.Collections.IEnumerator ReEnableInteraction()
    {
        yield return new WaitForSeconds(1f);
        isInteracting = false;
    }
}