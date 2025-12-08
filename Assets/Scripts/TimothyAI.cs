using UnityEngine;

public class TimothyAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform player;
    public float chaseSpeed = 5f;
    public float activationDistance = 10f;

    [Header("Audio")]
    public AudioClip killSound;

    private bool isActive = false;
    private bool hasKilled = false;

    public bool IsActive => isActive;

    public void Activate()
    {
        isActive = true;
    }

    void Update()
    {
        if (!isActive || hasKilled || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is far enough away to start chasing
        if (distanceToPlayer <= activationDistance)
        {
            // Move towards player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;

            // Look at player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
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

            Debug.Log("Timothy caught the player! Caught ending triggered.");
        }
    }
}