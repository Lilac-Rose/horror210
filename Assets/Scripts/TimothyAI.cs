using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimothyAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform player;
    public float chaseSpeed = 5f;
    public float activationDistance = 10f;

    [Header("Movement Constraints")]
    [Tooltip("Lock Timothy's Y position to prevent floating/sinking")]
    public bool lockYPosition = true;
    private float lockedYPosition;

    [Header("Audio")]
    public AudioClip killSound;

    private bool isActive = false;
    private bool hasKilled = false;

    public bool IsActive => isActive;

    void Start()
    {
        // Store the initial Y position
        lockedYPosition = transform.position.y;
    }

    public void Activate()
    {
        isActive = true;
        // Update locked Y position when activated
        lockedYPosition = transform.position.y;
    }

    void Update()
    {
        if (!isActive || hasKilled || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within activation distance to start chasing
        if (distanceToPlayer <= activationDistance)
        {
            // Calculate direction only on X and Z axes (horizontal plane)
            Vector3 playerPosFlat = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 direction = (playerPosFlat - transform.position).normalized;

            // Move towards player
            Vector3 newPosition = transform.position + direction * chaseSpeed * Time.deltaTime;

            // Lock Y position if enabled
            if (lockYPosition)
            {
                newPosition.y = lockedYPosition;
            }

            transform.position = newPosition;

            // Rotate to look at player (only horizontal rotation - Y axis only)
            Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 lookDirection = lookTarget - transform.position;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                // Only use the Y rotation, keep X and Z at 0
                transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasKilled || !isActive) return;

        if (other.CompareTag("Player"))
        {
            hasKilled = true;
            Interactable.caughtEndingTriggered = true;

            if (killSound != null)
                AudioSource.PlayClipAtPoint(killSound, transform.position);

            Debug.Log("Timothy caught the player! Loading Hospital scene.");

            // Disable player controls
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = false;
            }

            // Find and disable MouseLook on the camera
            MouseLook mouseLook = player.GetComponentInChildren<MouseLook>();
            if (mouseLook != null)
            {
                mouseLook.enabled = false;
            }

            // Load Hospital scene immediately
            SceneManager.LoadScene("Hospital");
        }
    }
}