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
    public Image gunImage;
    public LayerMask interactMask;
    public Transform playerBody;

    [Header("Gun Settings")]
    public float gunRange = 50f;
    public LayerMask timothyMask;

    [Header("UI Feedback")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("Teleport Settings")]
    public float firstNumber = -66.75f;
    public float secondNumber = 41f;

    private Interactable currentTarget;
    private bool isInteracting = false;
    private bool hasTeleported = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (!isInteracting)
        {
            HandleLook();
            HandleInteraction();
        }

        // Gun shooting
        if (Interactable.HasGun && Input.GetMouseButtonDown(0))
        {
            HandleGunShoot();
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
            case InteractableType.Gun: HandleGunInteraction(); break;
            case InteractableType.Window: HandleWindowInteraction(); break;
            case InteractableType.Photo: HandlePhotoInteraction(); break;
            case InteractableType.BathroomSink: HandleBathroomSinkInteraction(); break;
            case InteractableType.FinalDoor: HandleFinalDoorInteraction(); break;
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

    private void HandleGunInteraction()
    {
        if (currentTarget.PickupGun())
        {
            // Gun pickup handles its own UI display via gunUIImage field
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

    private void HandleFinalDoorInteraction()
    {
        StartCoroutine(FinalDoorSequence(currentTarget));
    }

    private void HandleGunShoot()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gunRange, timothyMask))
        {
            TimothyAI timothy = hit.collider.GetComponent<TimothyAI>();

            if (timothy != null && timothy.IsActive)
            {
                // Play gun sound
                Interactable gunInteractable = FindFirstObjectByType<Interactable>();
                if (gunInteractable != null && gunInteractable.gunShootSound != null)
                {
                    audioSource.PlayOneShot(gunInteractable.gunShootSound);
                }

                // Trigger shot ending
                Interactable.shotEndingTriggered = true;
                Debug.Log("Timothy shot! Loading PaddedRoom scene.");

                // Disable player controls
                PlayerController pc = playerBody.GetComponent<PlayerController>();
                MouseLook mouseLook = GetComponent<MouseLook>();

                if (pc != null) pc.enabled = false;
                if (mouseLook != null) mouseLook.enabled = false;

                // Destroy Timothy
                Destroy(timothy.gameObject);

                // Load the PaddedRoom scene immediately
                UnityEngine.SceneManagement.SceneManager.LoadScene("PaddedRoom");
            }
        }
    }

    private IEnumerator FinalDoorSequence(Interactable doorInteractable)
    {
        isInteracting = true;
        interactImage.enabled = false;
        lockImage.enabled = false;

        PlayerController pc = playerBody.GetComponent<PlayerController>();
        MouseLook mouseLook = GetComponent<MouseLook>();

        // Stop player movement and footsteps
        pc?.StopFootsteps();
        bool wasMovementEnabled = pc != null && pc.enabled;
        bool wasMouseLookEnabled = mouseLook != null && mouseLook.enabled;

        if (pc != null) pc.movementLocked = true;
        if (mouseLook != null) mouseLook.lookLocked = true;

        // 1. Make objects appear
        if (doorInteractable.objectsToAppear != null)
        {
            foreach (GameObject obj in doorInteractable.objectsToAppear)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // 2. Increase player speed
        if (pc != null)
        {
            pc.walkSpeed *= doorInteractable.speedMultiplier;
        }

        // 3. Change the player's light range
        Light playerLight = doorInteractable.playerLight;
        float originalRange = 0f;
        if (playerLight != null)
        {
            originalRange = playerLight.range;
            yield return StartCoroutine(ChangeLightRange(playerLight, doorInteractable.targetLightRange, doorInteractable.lightChangeDuration));
        }

        // 4. Back the player away from the door
        yield return StartCoroutine(BackPlayerAway(playerBody, doorInteractable.playerBackAwayDistance, doorInteractable.playerBackAwaySpeed));

        // 5. Rotate door on left edge (horizontal swing)
        StartCoroutine(RotateDoorHorizontal(doorInteractable));

        // 6. Activate Timothy and move him forward slowly (coming out of door)
        if (doorInteractable.timothyObject != null)
        {
            doorInteractable.timothyObject.SetActive(true);

            // Set up Timothy's AI but don't activate it yet
            TimothyAI timothyAI = doorInteractable.timothyObject.GetComponent<TimothyAI>();
            if (timothyAI != null)
            {
                timothyAI.player = playerBody;
                timothyAI.chaseSpeed = doorInteractable.timothyMoveSpeed;
                timothyAI.killSound = doorInteractable.timothyKillSound;
            }

            // Move Timothy forward slowly (emerging from door) - wait for this to complete
            yield return StartCoroutine(MoveTimothyForward(doorInteractable.timothyObject, doorInteractable.timothyMoveSpeed * 0.3f, 2f));
        }

        // 7. Turn player camera 180 degrees (after Timothy emerges)
        yield return StartCoroutine(RotatePlayer180(playerBody));

        // 8. Start audio
        if (doorInteractable.finalDoorAudio != null)
        {
            audioSource.clip = doorInteractable.finalDoorAudio;
            audioSource.Play();
        }

        // 9. Activate Timothy's AI after player turns around
        if (doorInteractable.timothyObject != null)
        {
            TimothyAI timothyAI = doorInteractable.timothyObject.GetComponent<TimothyAI>();
            if (timothyAI != null)
            {
                // Make sure player reference is set before activation
                if (timothyAI.player == null)
                    timothyAI.player = playerBody;

                timothyAI.Activate();
                Debug.Log("Timothy activated from FinalDoorSequence!");
            }
            else
            {
                Debug.LogError("No TimothyAI component found on timothyObject!");
            }
        }

        // 10. Make objects disappear when chase starts
        if (doorInteractable.objectsToDisappear != null)
        {
            foreach (GameObject obj in doorInteractable.objectsToDisappear)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        // Re-enable controls
        if (pc != null) pc.movementLocked = false;
        if (mouseLook != null) mouseLook.lookLocked = false;

        // Start chase music when player regains control
        if (doorInteractable.chaseMusic != null)
        {
            audioSource.clip = doorInteractable.chaseMusic;
            audioSource.loop = true; // Chase music should loop
            audioSource.Play();
        }

        currentTarget = null;
        isInteracting = false;
    }

    private IEnumerator ChangeLightRange(Light light, float targetRange, float duration)
    {
        float startRange = light.range;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.range = Mathf.Lerp(startRange, targetRange, elapsed / duration);
            yield return null;
        }

        light.range = targetRange;
    }

    private IEnumerator BackPlayerAway(Transform player, float distance, float speed)
    {
        Vector3 startPosition = player.position;
        Vector3 targetPosition = startPosition - player.forward * distance;
        float elapsed = 0f;
        float duration = distance / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            player.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        player.position = targetPosition;
    }

    private IEnumerator RotateDoorHorizontal(Interactable doorInteractable)
    {
        Transform doorTransform = doorInteractable.transform;
        float targetRotation = doorInteractable.doorRotationDegrees;
        float rotationSpeed = doorInteractable.doorRotationSpeed;

        // Determine pivot point (left edge of door for hinge)
        Vector3 pivotPoint;
        if (doorInteractable.doorPivotPoint != null)
        {
            pivotPoint = doorInteractable.doorPivotPoint.position;
        }
        else
        {
            // Calculate left edge based on door's bounds (hinge side)
            Renderer doorRenderer = doorTransform.GetComponent<Renderer>();
            if (doorRenderer != null)
            {
                Bounds bounds = doorRenderer.bounds;
                // Use the minimum X (left edge) as the pivot for the hinge
                pivotPoint = new Vector3(bounds.min.x, doorTransform.position.y, doorTransform.position.z);
            }
            else
            {
                // Fallback: offset to the left edge
                pivotPoint = doorTransform.position + doorTransform.right * -0.5f;
            }
        }

        float rotated = 0f;

        while (rotated < targetRotation)
        {
            float deltaRotation = rotationSpeed * Time.deltaTime;

            if (rotated + deltaRotation > targetRotation)
            {
                deltaRotation = targetRotation - rotated;
            }

            // Rotate around the pivot point on Y axis (horizontal swing like a door)
            doorTransform.RotateAround(pivotPoint, Vector3.up, deltaRotation);

            rotated += deltaRotation;

            yield return null;
        }
    }

    private IEnumerator MoveTimothyForward(GameObject timothy, float speed, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Move forward in Timothy's forward direction
            Vector3 forwardMovement = timothy.transform.forward * speed * Time.deltaTime;
            // Add positive X offset
            Vector3 xOffset = Vector3.right * speed * Time.deltaTime;

            timothy.transform.position += forwardMovement + xOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator RotatePlayer180(Transform player)
    {
        float rotationSpeed = 360f; // degrees per second
        float rotated = 0f;

        while (rotated < 180f)
        {
            float deltaRotation = rotationSpeed * Time.deltaTime;
            player.Rotate(0, deltaRotation, 0);
            rotated += deltaRotation;
            yield return null;
        }
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

        // Play photo display sound before fadeout
        if (photoInteractable.photoDisplaySound != null)
            AudioSource.PlayClipAtPoint(photoInteractable.photoDisplaySound, transform.position);

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