using UnityEngine;

public class LookAwayDissapear : MonoBehaviour
{
    [Header("Objects to Dissapear")]
    public GameObject[] objectsToDissapear;

    [Header("Detection Settings")]
    public Transform playerCamera;
    public float lookAwayAngle = 60f;
    public float triggerDistance = 3f;

    [Header("Debug")]
    public bool debugMode = true;

    private bool hasTriggered = false;
    private bool playerInRange = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (objectsToDissapear == null || objectsToDissapear.Length == 0)
        {
            Debug.LogError("No objects assigned to dissapear!", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTriggered) return;

        float distance = Vector3.Distance(playerCamera.position, transform.position);
        playerInRange = distance <= triggerDistance;

        if (playerInRange)
        {
            if (IsPlayerLookingAway())
            {
                DissapearObjects();
            }
        }
    }

    bool IsPlayerLookingAway()
    {
        if (playerCamera == null) return false;

        Vector3 directionTrigger = (transform.position - playerCamera.position).normalized;

        float angle = Vector3.Angle(playerCamera.forward, directionTrigger);

        if (debugMode)
        {
            Debug.DrawRay(playerCamera.position, playerCamera.forward * triggerDistance, angle <= lookAwayAngle ? Color.green : Color.red);
            Debug.DrawRay(playerCamera.position, directionTrigger * triggerDistance, Color.yellow);
        }

        return angle > lookAwayAngle;
    }
    void DissapearObjects()
    {
        hasTriggered = true;

        foreach (GameObject obj in objectsToDissapear)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                if (debugMode)
                {
                    Debug.Log($"Dissapeared: {obj.name}", this);
                }
            }
        }
    }
}
