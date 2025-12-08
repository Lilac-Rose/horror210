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
    public bool showTriggerRange = true;
    public bool showFOVCone = true;
    public Color triggerRangeColor = new Color(1f, 1f, 0f, 0.3f); // Yellow
    public Color fovConeColor = new Color(0f, 1f, 0f, 0.2f); // Green

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
            if (debugMode)
            {
                Debug.Log($"Player in range. Checking FOV...", this);
            }

            // Check if ALL objects are completely outside the player's FOV
            if (AreAllObjectsOutsideFOV())
            {
                if (debugMode)
                {
                    Debug.Log("All objects outside FOV - triggering disappear!", this);
                }
                DisappearObjects();
            }
        }
        else if (debugMode)
        {
            Debug.Log($"Player out of range. Distance: {distance:F2}m (need: {triggerDistance}m)", this);
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

            if (debugMode)
            {
                Debug.Log($"{obj.name} has no collider, checking center. Angle: {angle:F1}° (FOV limit: {lookAwayAngle}°)", this);
            }

            return angle <= lookAwayAngle;
        }

        // Get the bounds of the collider
        Bounds bounds = collider.bounds;

        if (debugMode)
        {
            Debug.Log($"Checking {obj.name} collider bounds. Size: {bounds.size}", this);
        }

        int pointsInFOV = 0;
        int totalPoints = 0;

        // Check multiple points across the collider bounds
        int points = checkPointsPerAxis;
        for (int x = 0; x < points; x++)
        {
            for (int y = 0; y < points; y++)
            {
                for (int z = 0; z < points; z++)
                {
                    totalPoints++;

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
                        pointsInFOV++;
                        if (debugMode)
                        {
                            Debug.Log($"  Point {totalPoints} of {obj.name} IN FOV! Angle: {angle:F1}°", this);
                        }
                        return true;
                    }
                }
            }
        }

        if (debugMode)
        {
            Debug.Log($"{obj.name}: {pointsInFOV}/{totalPoints} points in FOV. Object is OUTSIDE FOV.", this);
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

    void OnDrawGizmos()
    {
        if (!debugMode) return;

        // Show trigger range sphere
        if (showTriggerRange)
        {
            Gizmos.color = triggerRangeColor;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }

        // Show FOV cone from player camera
        if (showFOVCone && playerCamera != null)
        {
            Gizmos.color = fovConeColor;

            // Draw FOV cone
            int segments = 32;
            float angleStep = 360f / segments;
            Vector3 forward = playerCamera.forward * triggerDistance;

            for (int i = 0; i < segments; i++)
            {
                float currentAngle = angleStep * i;
                float nextAngle = angleStep * (i + 1);

                // Create rotation around the forward axis
                Quaternion rot1 = Quaternion.AngleAxis(currentAngle, playerCamera.forward);
                Quaternion rot2 = Quaternion.AngleAxis(nextAngle, playerCamera.forward);

                // Calculate the direction at the FOV angle
                Vector3 up = playerCamera.up;
                Vector3 dir1 = Quaternion.AngleAxis(lookAwayAngle, up) * playerCamera.forward;
                Vector3 dir2 = dir1;

                // Rotate around forward axis
                dir1 = rot1 * dir1;
                dir2 = rot2 * dir2;

                Vector3 point1 = playerCamera.position + dir1 * triggerDistance;
                Vector3 point2 = playerCamera.position + dir2 * triggerDistance;

                // Draw cone edge segments
                Gizmos.DrawLine(point1, point2);

                // Draw lines from camera to cone edge
                if (i % 4 == 0)
                {
                    Gizmos.DrawLine(playerCamera.position, point1);
                }
            }

            // Draw center line
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerCamera.position, playerCamera.position + playerCamera.forward * triggerDistance);
        }

        // Draw lines to objects being monitored
        if (objectsToDisappear != null)
        {
            foreach (GameObject obj in objectsToDisappear)
            {
                if (obj != null)
                {
                    Gizmos.color = obj.activeSelf ? Color.cyan : Color.gray;
                    Gizmos.DrawLine(transform.position, obj.transform.position);

                    // Draw small sphere at object position
                    Gizmos.DrawWireSphere(obj.transform.position, 0.2f);
                }
            }
        }
    }
}