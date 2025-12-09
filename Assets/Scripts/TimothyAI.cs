using UnityEngine;
using UnityEngine.SceneManagement;

public class TimothyAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform player;
    public float chaseSpeed = 5f;

    [Header("Movement Constraints")]
    public bool lockYPosition = true;
    private float lockedYPosition;

    [Header("Audio")]
    public AudioClip killSound;

    [Header("Light Settings")]
    [Tooltip("Point light that activates when chase starts")]
    public Light timothyLight;

    private bool isActive = false;
    private bool hasKilled = false;

    public bool IsActive => isActive;

    void Start()
    {
        lockedYPosition = transform.position.y;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Get the light component if not assigned
        if (timothyLight == null)
        {
            timothyLight = GetComponentInChildren<Light>();
        }

        // Make sure light starts off
        if (timothyLight != null)
        {
            timothyLight.enabled = false;
        }
    }

    public void Activate()
    {
        isActive = true;
        lockedYPosition = transform.position.y;

        // Turn on the light when chase starts
        if (timothyLight != null)
        {
            timothyLight.enabled = true;
        }
    }

    void Update()
    {
        if (!isActive || hasKilled || player == null) return;

        // Move toward player (flat movement)
        Vector3 playerPosFlat = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 direction = (playerPosFlat - transform.position).normalized;
        Vector3 newPosition = transform.position + direction * chaseSpeed * Time.deltaTime;

        if (lockYPosition)
            newPosition.y = lockedYPosition;

        transform.position = newPosition;

        Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 lookDirection = lookTarget - transform.position;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Vector3 rot = new Vector3(90f, targetRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Euler(rot);
        }
        else
        {
            // Ensure X rotation stays locked even when not rotating
            Vector3 rot = transform.rotation.eulerAngles;
            rot.x = 90f;
            rot.z = 0f;
            transform.rotation = Quaternion.Euler(rot);
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

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;

            MouseLook mouseLook = player.GetComponentInChildren<MouseLook>();
            if (mouseLook != null)
                mouseLook.enabled = false;

            SceneManager.LoadScene("Hospital");
        }
    }
}