using UnityEngine;

public class LookAwayDisappear : MonoBehaviour
{
    [Header("Objects to Disappear")]
    public GameObject[] objectsToDisappear;

    [Header("Detection Settings")]
    public Transform playerCamera;
    public float lookAwayAngle = 60f;
    public float triggerDistance = 3f;
    public float checkInterval = 0.1f; // How often to check (in seconds)

    [Header("Collider Check Settings")]
    [Tooltip("Number of points to check on each collider (more = more accurate but slower)")]
    public int checkPointsPerAxis = 3;

    [Header("Debug")]
    public bool debugMode = true;

    private bool hasTriggered = false;
    private bool playerInRange = false;
    private float nextCheckTime = 0f;

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (objectsToDisappear == null || objectsToDisappear.Length == 0)
        {
            Debug.LogError("No objects assigned to disappear!", this);
        }
    }

    void Update()
    {
        if (hasTriggered) return;

        // Only check at intervals for performance
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        float distance = Vector3.Distance(playerCamera.position, transform.position);
        playerInRange = distance <= triggerDistance;

        if (playerInRange)
        {
            // Check if ALL objects are completely outside the player's FOV
            if (AreAllObjectsOutsideFOV())
            {
                DisappearObjects();
            }
        }
    }

    bool AreAllObjectsOutsideFOV()
    {
        if (playerCamera == null) return false;

        // Check each object to see if ANY part of it is in the FOV
        foreach (GameObject obj in objectsToDisappear)
        {
            if (obj != null && obj.activeSelf)
            {
                if (IsAnyPartOfColliderInFOV(obj))
                {
                    if (debugMode)
                    {
                        Debug.Log($"{obj.name} has parts still in FOV", this);
                    }
                    return false; // At least one object is visible
                }
            }
        }

        // All objects are completely outside FOV
        return true;
    }

    bool IsAnyPartOfColliderInFOV(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();

        if (collider == null)
        {
            // Fallback to checking center point if no collider
            Vector3 directionToObject = (obj.transform.position - playerCamera.position).normalized;
            float angle = Vector3.Angle(playerCamera.forward, directionToObject);
            return angle <= lookAwayAngle;
        }

        // Get the bounds of the collider
        Bounds bounds = collider.bounds;

        // Check multiple points across the collider bounds
        int points = checkPointsPerAxis;
        for (int x = 0; x < points; x++)
        {
            for (int y = 0; y < points; y++)
            {
                for (int z = 0; z < points; z++)
                {
                    // Calculate point position within bounds
                    Vector3 point = new Vector3(
                        Mathf.Lerp(bounds.min.x, bounds.max.x, x / (float)(points - 1)),
                        Mathf.Lerp(bounds.min.y, bounds.max.y, y / (float)(points - 1)),
                        Mathf.Lerp(bounds.min.z, bounds.max.z, z / (float)(points - 1))
                    );

                    Vector3 directionToPoint = (point - playerCamera.position).normalized;
                    float angle = Vector3.Angle(playerCamera.forward, directionToPoint);

                    if (debugMode)
                    {
                        Debug.DrawRay(playerCamera.position, directionToPoint * triggerDistance * 0.5f,
                            angle <= lookAwayAngle ? Color.green : Color.red, checkInterval);
                    }

                    // If ANY point is in FOV, the object is visible
                    if (angle <= lookAwayAngle)
                    {
                        return true;
                    }
                }
            }
        }

        // No points were in FOV
        return false;
    }

    void DisappearObjects()
    {
        hasTriggered = true;

        foreach (GameObject obj in objectsToDisappear)
        {
            if (obj != null)
            {
                obj.SetActive(false);

                if (debugMode)
                {
                    Debug.Log($"Disappeared: {obj.name}", this);
                }
            }
        }
    }
}