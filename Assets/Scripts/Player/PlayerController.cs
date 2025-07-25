// Scripts/Player/PlayerController.cs
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages player character movement, input, and interactions.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dependencies")]
    [Tooltip("Tilemap used for indicating buildable ground visually.")]
    [SerializeField] private Tilemap buildableAreaIndicator; // Optional: For visual feedback

    // Cached Components
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    // For physics-based movement in FixedUpdate
    private Vector2 movementVelocity;

    void Awake()
    {
        // Cache component references for performance
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Configure Rigidbody for top-down 2D movement
        rb.isKinematic = false;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Process Input in Update() for responsiveness
        // Use GetAxisRaw for immediate, non-smoothed input, ideal for character movement.
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Normalize the input vector to prevent faster diagonal movement.
        if (moveInput.sqrMagnitude > 1)
        {
            moveInput.Normalize();
        }

        movementVelocity = moveInput * moveSpeed;

        // Update animator parameters
        if (animator != null)
        {
            animator.SetBool("IsMoving", moveInput.sqrMagnitude > 0.01f);
            if (moveInput.x != 0) // Prioritize horizontal flip
            {
                // Flip the sprite based on movement direction
                transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);
            }
        }
    }

    void FixedUpdate()
    {
        // 2. Apply Physics in FixedUpdate() for consistency
        // By directly setting the velocity, we get responsive and predictable movement
        // that still respects collisions handled by the physics engine.
        rb.velocity = movementVelocity;
    }

    /// <summary>
    /// Handles collision with triggers, perfect for resource gathering.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Resource"))
        {
            // Example: Logic for picking up a resource
            Debug.Log($"Collected a resource: {other.name}");
            // Assuming the resource has a script to handle its collection
            // other.GetComponent<ResourceNode>()?.Collect();
            Destroy(other.gameObject);
        }
    }
}