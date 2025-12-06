using UnityEngine;
using UnityEngine.UI;

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
                    isLocked = interact.IsLocked || interact.isJammedDoor;
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
            case InteractableType.Generic: Debug.Log("Interacted with generic object."); break;
        }
    }

    private void HandleDoorInteraction()
    {
        if (!currentTarget.UseDoor()) return;

        Transform doorTarget = currentTarget.doorTarget;

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

    private System.Collections.IEnumerator ReEnableInteraction()
    {
        yield return new WaitForSeconds(1f);
        isInteracting = false;
    }
}