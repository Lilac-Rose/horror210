using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 5f;
    public Image interactImage;
    public Image lockImage;
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
    private bool hasTeleported = false; // Prevent multiple teleports

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
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactMask))
        {
            Interactable interact = hit.collider.GetComponent<Interactable>();

            if (interact != null)
            {
                currentTarget = interact;
                interactImage.enabled = true;

                if ((interact.type == InteractableType.Door && interact.IsLocked) ||
                    (interact.type == InteractableType.Window && interact.IsWindowLocked))
                {
                    interactImage.enabled = false;
                    lockImage.enabled = true;
                }
                else
                {
                    interactImage.color = unlockedColor;
                    lockImage.enabled = false;
                }

                return;
            }
        }

        // No interactable hit
        currentTarget = null;
        interactImage.enabled = false;
        lockImage.enabled = false;
    }

    private void HandleInteraction()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (currentTarget.type)
            {
                case InteractableType.Door:
                    HandleDoorInteraction();
                    break;

                case InteractableType.Lantern:
                    HandleLanternInteraction();
                    break;

                case InteractableType.Window:
                    HandleWindowInteraction();
                    break;

                case InteractableType.Generic:
                    Debug.Log("Interacted with generic object.");
                    break;
            }
        }
    }

    private void HandleDoorInteraction()
    {
        if (!currentTarget.UseDoor())
        {
            Debug.Log("Cannot open door");
            return;
        }

        Transform doorTarget = currentTarget.doorTarget;

        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;
        currentTarget = null;

        // Stop footsteps immediately
        PlayerController pc = playerBody.GetComponent<PlayerController>();
        if (pc != null)
            pc.StopFootsteps();

        ScreenFader.Instance.FadeAndTeleport(playerBody, doorTarget);

        StartCoroutine(ReEnableInteraction());
    }

    private void HandleLanternInteraction()
    {
        if (currentTarget.PickupLantern())
        {
            interactImage.enabled = false;
            lockImage.enabled = false;
            currentTarget = null;
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

                float zOffset = firstNumber - secondNumber;
                Vector3 newPos = playerBody.position;
                newPos.z += zOffset;

                PlayerController pc = playerBody.GetComponent<PlayerController>();
                if (pc != null)
                {
                    // Stop footsteps before teleport
                    pc.StopFootsteps();
                    Debug.Log("Teleporting player safely...");
                    pc.Teleport(newPos);
                }
                else
                {
                    Debug.LogWarning("PlayerController not found on playerBody! Teleport failed.");
                }
            }
        }
    }

    private System.Collections.IEnumerator ReEnableInteraction()
    {
        yield return new WaitForSeconds(1f);
        isInteracting = false;
    }
}
