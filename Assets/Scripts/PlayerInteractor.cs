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
    [Tooltip("Layer mask for Timothy - set to 'Everything' or create a 'Timothy' layer")]
    public LayerMask timothyMask = -1; // -1 means everything by default

    [Tooltip("Light to use as muzzle flash")]
    public Light muzzleFlashLight;

    [Tooltip("Maximum range for muzzle flash light")]
    public float muzzleFlashMaxRange = 50f;

    [Tooltip("How fast the muzzle flash appears (seconds)")]
    public float muzzleFlashRiseTime = 0.05f;

    [Tooltip("How long the muzzle flash stays at max brightness (seconds)")]
    public float muzzleFlashHoldTime = 0.05f;

    [Tooltip("How fast the muzzle flash fades out (seconds)")]
    public float muzzleFlashFadeTime = 0.1f;

    [Tooltip("Maximum angle from center to shoot Timothy (degrees)")]
    public float shootAngleThreshold = 15f;

    [Header("UI Feedback")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("Teleport Settings")]
    public float firstNumber = -66.75f;
    public float secondNumber = 41f;

    [Header("Screen Shake Settings")]
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 25f;

    private Interactable currentTarget;
    private bool isInteracting = false;
    private bool hasTeleported = false;
    private AudioSource audioSource;
    private bool isScreenShaking = false;
    private Coroutine screenShakeCoroutine;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        // Make sure muzzle flash light starts off
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
            muzzleFlashLight.range = 0f;
        }
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

        // Stop screen shake if either ending is triggered
        if ((Interactable.shotEndingTriggered || Interactable.caughtEndingTriggered) && isScreenShaking)
        {
            StopScreenShake();
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
        // Check if this is a jammed door and trigger delayed audio
        if (currentTarget.isJammedDoor && currentTarget.delayedJammedAudio != null)
        {
            StartCoroutine(PlayDelayedJammedAudio(currentTarget.delayedJammedAudio, currentTarget.transform.position));
        }

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

    private IEnumerator PlayDelayedJammedAudio(AudioClip audioClip, Vector3 position)
    {
        yield return new WaitForSeconds(1f);

        if (audioClip != null)
        {
            AudioSource.PlayClipAtPoint(audioClip, position, 1.5f);
        }
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
        // Debug: Check if gun is equipped
        if (!Interactable.HasGun)
        {
            Debug.Log("Gun not equipped - cannot shoot");
            return;
        }

        Debug.Log("Gun equipped, attempting to shoot...");

        Ray ray = new Ray(transform.position, transform.forward);

        // Debug: Show raycast info
        Debug.DrawRay(transform.position, transform.forward * gunRange, Color.red, 1f);

        // Use RaycastAll to detect trigger colliders
        RaycastHit[] hits = Physics.RaycastAll(ray, gunRange, timothyMask, QueryTriggerInteraction.Collide);

        if (hits.Length == 0)
        {
            Debug.Log("Raycast didn't hit anything in timothyMask");
            return; // Don't shoot or play sound if nothing hit
        }

        Debug.Log($"Raycast hit {hits.Length} object(s)");

        // Check each hit for Timothy
        foreach (RaycastHit hit in hits)
        {
            Debug.Log($"Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}");

            TimothyAI timothy = hit.collider.GetComponent<TimothyAI>();

            if (timothy != null)
            {
                Debug.Log($"Timothy found! IsActive: {timothy.IsActive}");

                if (timothy.IsActive)
                {
                    // Check if player is looking at Timothy (within angle threshold)
                    Vector3 directionToTimothy = (timothy.transform.position - transform.position).normalized;
                    float angleToTimothy = Vector3.Angle(transform.forward, directionToTimothy);

                    Debug.Log($"Angle to Timothy: {angleToTimothy:F1}° (threshold: {shootAngleThreshold}°)");

                    if (angleToTimothy <= shootAngleThreshold)
                    {
                        Debug.Log("Timothy is active and in crosshairs, triggering shot ending!");

                        // Start muzzle flash and shooting sequence
                        StartCoroutine(MuzzleFlashEffect());
                        StartCoroutine(ShootTimothySequence(timothy));
                        return; // Exit after starting the sequence
                    }
                    else
                    {
                        Debug.Log($"Timothy not in crosshairs - angle {angleToTimothy:F1}° exceeds threshold {shootAngleThreshold}°");
                        return; // Don't shoot if not looking at Timothy
                    }
                }
                else
                {
                    Debug.Log("Timothy found but not active yet");
                }
            }
            else
            {
                Debug.Log("Hit object has no TimothyAI component");
            }
        }
    }

    private IEnumerator MuzzleFlashEffect()
    {
        if (muzzleFlashLight == null) yield break;

        // Turn light on
        muzzleFlashLight.enabled = true;
        muzzleFlashLight.range = 0f;

        // Rise quickly
        float elapsed = 0f;
        while (elapsed < muzzleFlashRiseTime)
        {
            elapsed += Time.deltaTime;
            muzzleFlashLight.range = Mathf.Lerp(0f, muzzleFlashMaxRange, elapsed / muzzleFlashRiseTime);
            yield return null;
        }
        muzzleFlashLight.range = muzzleFlashMaxRange;

        // Hold at max
        yield return new WaitForSeconds(muzzleFlashHoldTime);

        // Fade out
        elapsed = 0f;
        while (elapsed < muzzleFlashFadeTime)
        {
            elapsed += Time.deltaTime;
            muzzleFlashLight.range = Mathf.Lerp(muzzleFlashMaxRange, 0f, elapsed / muzzleFlashFadeTime);
            yield return null;
        }

        // Turn off
        muzzleFlashLight.range = 0f;
        muzzleFlashLight.enabled = false;
    }

    private IEnumerator ShootTimothySequence(TimothyAI timothy)
    {
        // Play gun sound
        if (Interactable.StoredGunShootSound != null)
        {
            Debug.Log("Playing gun sound (hit Timothy!)");
            AudioSource.PlayClipAtPoint(Interactable.StoredGunShootSound, transform.position, 1f);
        }
        else
        {
            Debug.LogWarning("StoredGunShootSound is NULL - gun sound was not stored properly on pickup");
        }

        // Trigger shot ending flag
        Interactable.shotEndingTriggered = true;

        // Disable player controls immediately
        PlayerController pc = playerBody.GetComponent<PlayerController>();
        MouseLook mouseLook = GetComponent<MouseLook>();

        if (pc != null) pc.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;

        // Wait for gun sound to play (adjust time based on your sound clip length)
        float gunSoundLength = Interactable.StoredGunShootSound != null ? Interactable.StoredGunShootSound.length : 1f;
        yield return new WaitForSeconds(gunSoundLength + 0.5f); // Add 0.5s buffer

        // Destroy Timothy
        Destroy(timothy.gameObject);

        Debug.Log("Timothy shot! Loading PaddedRoom scene.");

        // Load the PaddedRoom scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("PaddedRoom");
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

        // 5. Play final door audio when door starts opening
        if (doorInteractable.finalDoorAudio != null)
        {
            audioSource.clip = doorInteractable.finalDoorAudio;
            audioSource.Play();
        }

        // 6. Rotate door on left edge (horizontal swing)
        StartCoroutine(RotateDoorHorizontal(doorInteractable));

        // 7. Activate Timothy and move him forward slowly (coming out of door)
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

        // 8. Turn player camera 180 degrees (after Timothy emerges)
        // yield return StartCoroutine(RotatePlayer180(playerBody));

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

        // Start screen shake
        StartScreenShake();

        currentTarget = null;
        isInteracting = false;
    }

    private void StartScreenShake()
    {
        if (!isScreenShaking)
        {
            isScreenShaking = true;
            screenShakeCoroutine = StartCoroutine(ScreenShake());
        }
    }

    private void StopScreenShake()
    {
        if (isScreenShaking && screenShakeCoroutine != null)
        {
            StopCoroutine(screenShakeCoroutine);
            isScreenShaking = false;

            // Reset camera position to zero local position
            transform.localPosition = Vector3.zero;
        }
    }

    private IEnumerator ScreenShake()
    {
        Vector3 originalLocalPos = transform.localPosition;

        while (isScreenShaking)
        {
            // Generate random shake offset
            float offsetX = Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * 2f - 1f;
            float offsetY = Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * 2f - 1f;

            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0f) * shakeIntensity;
            transform.localPosition = originalLocalPos + shakeOffset;

            yield return null;
        }

        // Reset to original position when done
        transform.localPosition = originalLocalPos;
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
            AudioSource.PlayClipAtPoint(photoInteractable.photoDisplaySound, transform.position, 2f);

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