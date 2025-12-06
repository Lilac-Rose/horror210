using UnityEngine;

public class WindowTeleport : MonoBehaviour
{
    public Transform player;              
    public float firstNumber = -66.75f;   // The first value
    public float secondNumber = 41f;      // The second value
    public bool teleportOnce = true;      // Prevent repeat teleports

    private bool hasTeleported = false;

    void Update()
    {
        // Only run when all windows are locked
        if (Interactable.AllWindowsLocked)
        {
            if (!hasTeleported || !teleportOnce)
            {
                float zOffset = firstNumber - secondNumber;

                // Teleport player relative along Z
                Vector3 pos = player.position;
                pos.z += zOffset;
                player.position = pos;

                hasTeleported = true;
            }
        }
    }
}
