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

                if (interact.type == InteractableType.BathroomSink &&
                    interact.requiresCrowbar && !Interactable.HasCrowbar)
                {
                    currentTarget = null;
                    interactImage.enabled = false;
                    lockImage.enabled = false;
                    return;
                }

                bool isLocked = false;

                if (interact.type == InteractableType.Door)
                    isLocked = interact.IsLocked;

                else if (interact.type == InteractableType.Window)
                    isLocked = interact.IsWindowLocked;

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
            case InteractableType.Crowbar: HandleCrowbarInteraction(); break;
            case InteractableType.Window: HandleWindowInteraction(); break;
            case InteractableType.Photo: HandlePhotoInteraction(); break;
            case InteractableType.BathroomSink: HandleBathroomSinkInteraction(); break;
            case InteractableType.Generic: Debug.Log("Interacted with generic object."); break;
        }
    }

    private void HandleDoorInteraction()
    {
        if (!currentTarget.UseDoor()) return;

        Transform doorTarget;

        if (Interactable.HasCrowbar && currentTarget.postCrowbarPickupTarget != null)
            doorTarget = currentTarget.postCrowbarPickupTarget;

        else if (Interactable.AllWindowsLocked && currentTarget.postHouseSwitchTarget != null)
            doorTarget = currentTarget.postHouseSwitchTarget;

        else
            doorTarget = currentTarget.doorTarget;

        if (doorTarget == null)
        {
            Debug.Log("No valid door target found!");
            return;
        }

        if (currentTarget.DoorOpenSound != null)
            AudioSource.PlayClipAtPoint(currentTarget.DoorOpenSound, transform.position, 0.6f);

        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;

        AudioClip doorCloseSound = currentTarget.DoorCloseSound;

        currentTarget = null;

        PlayerController pc = playerBody.GetComponent<PlayerController>();
        pc?.StopFootsteps();

        ScreenFader.Instance.FadeAndTeleport(playerBody, doorTarget, doorCloseSound);
        StartCoroutine(ReEnableInteraction());
    }

    private void HandleLanternInteraction()
    {
        if (currentTarget.PickupLantern())
        {
            if (lanternImage != null)
                lanternImage.enabled = true;

            interactImage.enabled = false;
            lockImage.enabled = false;
            currentTarget = null;
        }
    }

    private void HandleCrowbarInteraction()
    {
        if (currentTarget.PickupCrowbar())
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
            if (Interactable.AllWindowsLocked && !hasTeleported)
            {
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

        if (currentTarget.photoPickupSound != null)
            AudioSource.PlayClipAtPoint(currentTarget.photoPickupSound, transform.position);

        StartCoroutine(PhotoSequence(currentTarget));

        currentTarget.gameObject.SetActive(false);
    }

    private void HandleBathroomSinkInteraction()
    {
        if (!currentTarget.UseBathroomSink()) return;
        StartCoroutine(BathroomSinkSequence(currentTarget));
    }

    private IEnumerator BathroomSinkSequence(Interactable sinkInteractable)
    {
        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;

        PlayerController pc = playerBody.GetComponent<PlayerController>();
        pc?.StopFootsteps();

        bool wasMovementEnabled = pc != null && pc.enabled;
        if (pc != null) pc.enabled = false;

        MouseLook mouseLook = GetComponent<MouseLook>();
        bool wasMouseLookEnabled = mouseLook != null && mouseLook.enabled;
        if (mouseLook != null) mouseLook.enabled = false;

        if (sinkInteractable.faucetOnSound != null)
            AudioSource.PlayClipAtPoint(sinkInteractable.faucetOnSound, sinkInteractable.transform.position);

        yield return new WaitForSeconds(sinkInteractable.faucetOnDuration);

        if (sinkInteractable.waterSplashingSound != null)
            AudioSource.PlayClipAtPoint(sinkInteractable.waterSplashingSound, sinkInteractable.transform.position);

        yield return new WaitForSeconds(sinkInteractable.splashingDuration);

        if (sinkInteractable.faucetOffSound != null)
            AudioSource.PlayClipAtPoint(sinkInteractable.faucetOffSound, sinkInteractable.transform.position);

        yield return new WaitForSeconds(sinkInteractable.faucetOffDuration);

        if (pc != null && wasMovementEnabled)
            pc.enabled = true;

        if (mouseLook != null && wasMouseLookEnabled)
            mouseLook.enabled = true;

        currentTarget = null;
        isInteracting = false;
    }

    private IEnumerator PhotoSequence(Interactable photoInteractable)
    {
        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;

        PlayerController pc = playerBody.GetComponent<PlayerController>();
        pc?.StopFootsteps();

        bool wasMovementEnabled = pc != null && pc.enabled;
        if (pc != null) pc.enabled = false;

        MouseLook mouseLook = GetComponent<MouseLook>();
        bool wasMouseLookEnabled = mouseLook != null && mouseLook.enabled;
        if (mouseLook != null) mouseLook.enabled = false;

        CanvasGroup photoCanvasGroup = photoInteractable.photoUIImage.GetComponent<CanvasGroup>();
        if (photoCanvasGroup == null)
            photoCanvasGroup = photoInteractable.photoUIImage.gameObject.AddComponent<CanvasGroup>();

        photoInteractable.photoUIImage.enabled = true;
        photoCanvasGroup.alpha = 0f;

        float fadeInTime = photoInteractable.photoFadeInDuration;
        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            photoCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInTime);
            yield return null;
        }

        photoCanvasGroup.alpha = 1f;

        if (photoInteractable.photoDialogueTrigger != null)
            photoInteractable.photoDialogueTrigger.TriggerText();

        yield return new WaitForSeconds(photoInteractable.photoDisplayDuration);

        Vector3 newPos = playerBody.position;
        float relativeZ = 184.5f - 110.5f;
        newPos.z += relativeZ;
        playerBody.position = newPos;

        float fadeOutTime = photoInteractable.photoFadeOutDuration;
        elapsed = 0f;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            photoCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            yield return null;
        }

        photoInteractable.photoUIImage.enabled = false;

        if (pc != null && wasMovementEnabled)
            pc.enabled = true;

        if (mouseLook != null && wasMouseLookEnabled)
            mouseLook.enabled = true;

        currentTarget = null;
        isInteracting = false;
    }

    private IEnumerator ReEnableInteraction()
    {
        yield return new WaitForSeconds(1f);
        isInteracting = false;
    }
}
