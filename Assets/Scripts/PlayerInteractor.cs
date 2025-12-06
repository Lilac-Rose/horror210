using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 5f;
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

                // Change interact image based on type and state
                if (interact.type == InteractableType.Door && interact.IsLocked)
                {
                    interactImage.enabled = false;
                    lockImage.enabled = true;
                }
                else if (interact.type == InteractableType.Window && interact.IsWindowLocked)
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

                case InteractableType.Window:
                    HandleWindowInteraction();
                    break;

                case InteractableType.Generic:
                    Debug.Log("Interacted with generic object.");
                    break;
            }
        }
    }

    void HandleDoorInteraction()
    {
        // Try to use the door - it will return false if locked or requires lantern
        bool doorOpened = currentTarget.UseDoor();

        if (!doorOpened)
        {
            // Door couldn't be opened (locked or missing lantern)
            Debug.Log("Cannot open door");
            return;
        }

        // Door successfully opened - teleport player
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

    void HandleWindowInteraction()
    {
        // Lock the window
        if (currentTarget.LockWindow())
        {
            // Window successfully locked
            Debug.Log("Window locked!");

            // Check if all windows are locked
            if (Interactable.AllWindowsLocked)
            {
                Debug.Log("All windows secured!");
                // You can trigger additional events here
            }
        }
    }

    System.Collections.IEnumerator ReEnableInteraction()
    {
        // Wait for fade duration + a bit extra
        yield return new WaitForSeconds(1f);
        isInteracting = false;
    }
}