using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float gravity = -9.81f;

    [Header("Audio")]
    public AudioClip footstepSound;
    public float footstepVolume = 0.5f;

    [Header("Debug")]
    public bool showDebugInfo = false;

    [HideInInspector] public bool movementLocked = false;

    private CharacterController controller;
    private Vector3 velocity;
    private AudioSource audioSource;
    private bool isPlayingFootsteps = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Get audio source component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = footstepSound;
        audioSource.loop = true;
        audioSource.volume = footstepVolume;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

    }

    void Update()
    {
        if (!movementLocked)
        {
            HandleMovement();
        }

        ApplyGravity();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);

        bool isMoving = move.magnitude > 0.1f;

        if (isMoving && !isPlayingFootsteps)
        {
            // Start playing footsteps
            audioSource.Play();
            isPlayingFootsteps = true;
        }
        else if (!isMoving && isPlayingFootsteps)
        {
            // Stop playing footsteps
            audioSource.Stop();
            isPlayingFootsteps = false;
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (showDebugInfo)
        {
            Debug.Log($"Grounded: {controller.isGrounded}, Velocity.y: {velocity.y:F2}");
        }
    }
}
