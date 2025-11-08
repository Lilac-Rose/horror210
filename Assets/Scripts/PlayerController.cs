using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runMultiplier = 1.5f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayerMask = -1;
    public bool useGroundTag = false;
    public string groundTag = "Ground";

    [Header("Debug")]
    public bool showDebugInfo = true;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleJumping();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = walkSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleJumping()
    {
        bool isGrounded = IsGrounded();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (showDebugInfo)
                Debug.Log($"Jumping! Jump velocity: {velocity.y}");
        }

        if (showDebugInfo)
        {
            Debug.Log($"Grounded: {isGrounded}, Velocity.y: {velocity.y:F2}");
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        Vector3 bottomCenter = transform.position - Vector3.up * (controller.height * 0.5f);
        float checkRadius = controller.radius * 0.95f;
        float checkDistance = 0.2f;

        Collider[] overlapping = Physics.OverlapSphere(bottomCenter, checkRadius, groundLayerMask);
        bool hasOverlap = false;

        foreach (Collider col in overlapping)
        {
            if (col != controller && col.transform != transform)
            {
                hasOverlap = true;
                break;
            }
        }

        bool sphereCastHit = Physics.SphereCast(
            bottomCenter + Vector3.up * checkDistance,
            checkRadius,
            Vector3.down,
            out RaycastHit sphereHit,
            checkDistance * 1.5f,
            groundLayerMask
        );

        bool controllerGrounded = controller.isGrounded;
        bool isGrounded = hasOverlap || sphereCastHit || controllerGrounded;

        if (isGrounded && useGroundTag && sphereCastHit)
        {
            isGrounded = sphereHit.collider.CompareTag(groundTag);
        }
        else if (isGrounded && useGroundTag && hasOverlap)
        {
            bool hasGroundTag = false;
            foreach (Collider col in overlapping)
            {
                if (col != controller && col.transform != transform && col.CompareTag(groundTag))
                {
                    hasGroundTag = true;
                    break;
                }
            }
            isGrounded = hasGroundTag;
        }

        if (showDebugInfo)
        {
            Color debugColor = isGrounded ? Color.green : Color.red;
            Debug.DrawRay(bottomCenter + Vector3.up * checkDistance, Vector3.down * checkDistance * 1.5f, debugColor);
        }

        return isGrounded;
    }
}
