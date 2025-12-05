using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Image interactImage;
    public Image lockImage;
    public LayerMask interactMask;
    public Transform playerBody;

    [Header("UI Feedback")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    private Interactable currentTarget;
    private bool isInteracting = false;

    void Update()
    {
        if (!isInteracting)
        {
            HandleLook();
            HandleInteraction();
        }
    }

    void HandleLook()
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

                // Change interact image if door is locked
                if (interact.type == InteractableType.Door && interact.IsLocked)
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

        // No interactable
        currentTarget = null;
        interactImage.enabled = false;
        lockImage.enabled = false;
    }

    void HandleInteraction()
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

                case InteractableType.Generic:
                    Debug.Log("Interacted with generic object.");
                    break;
            }
        }
    }

    void HandleDoorInteraction()
    {
        // Check if door is locked before teleporting
        if (currentTarget.IsLocked)
        {
            Debug.Log("This door is locked!");
            return;
        }

        // Use the door (handles locking logic)
        if (currentTarget.UseDoor())
        {
            // Store the target before clearing currentTarget
            Transform doorTarget = currentTarget.doorTarget;

            // Hide UI and start interaction
            isInteracting = true;
            interactImage.enabled = false;
            lockImage.enabled = false;
            currentTarget = null;

            // Fade + teleport
            ScreenFader.Instance.FadeAndTeleport(playerBody, doorTarget);

            // Re-enable interaction after a short delay
            StartCoroutine(ReEnableInteraction());
        }
    }

    void HandleLanternInteraction()
    {
        // Pick up the lantern
        if (currentTarget.PickupLantern())
        {
            // Hide UI
            interactImage.enabled = false;
            lockImage.enabled = false;
            currentTarget = null;
        }
    }

    System.Collections.IEnumerator ReEnableInteraction()
    {
        // Wait for fade duration + a bit extra
        yield return new WaitForSeconds(1f);
        isInteracting = false;
    }
}