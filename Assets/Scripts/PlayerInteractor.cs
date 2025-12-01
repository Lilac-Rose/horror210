using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Image interactImage;
    public LayerMask interactMask;
    public Transform playerBody;

    private Interactable currentTarget;

    void Update()
    {
        HandleLook();
        HandleInteraction();
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
                return;
            }
        }

        // No interactable
        currentTarget = null;
        interactImage.enabled = false;
    }

    void HandleInteraction()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentTarget.type == InteractableType.Door)
            {
                // Fade + teleport
                ScreenFader.Instance.FadeAndTeleport(playerBody, currentTarget.doorTarget);
            }
            else
            {
                Debug.Log("Interacted with generic object.");
            }
        }
    }
}
