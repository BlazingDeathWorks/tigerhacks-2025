using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float BASE_SPEED = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float DASH_SPEED_MULTIPLIER = 5f;
    [SerializeField] private float DASH_DURATION = 2f;
    [SerializeField] private float DASH_COOLDOWN = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dashSound;

    // Private variables
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Camera mainCamera;

    // Dash state
    private bool isDashing = false;
    private float dashTimeRemaining = 0f;
    private float dashCooldownRemaining = 0f;
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Configure Rigidbody2D for precise movement
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    void Update()
    {
        // Get input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // Get mouse position in world space
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Handle rotation - always point towards mouse
        RotateTowardsMouse();

        // Handle dash input
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownRemaining <= 0f && !isDashing)
        {
            StartDash();
        }

        // Update dash cooldown
        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            HandleNormalMovement();
        }
    }

    private void RotateTowardsMouse()
    {
        // Calculate direction to mouse
        Vector2 direction = mousePosition - (Vector2)transform.position;

        // Calculate angle and apply rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 assumes sprite faces up
    }

    private void HandleNormalMovement()
    {
        // Instant, snappy movement - no acceleration
        rb.linearVelocity = moveInput * BASE_SPEED;
    }

    private void StartDash()
    {
        // Use current movement direction, or face direction if not moving
        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = moveInput.normalized;
        }
        else
        {
            // If not moving, dash toward mouse
            dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        }

        isDashing = true;
        dashTimeRemaining = DASH_DURATION;
        dashCooldownRemaining = DASH_COOLDOWN;

        // Play dash sound if available
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }

    private void HandleDash()
    {
        dashTimeRemaining -= Time.fixedDeltaTime;

        if (dashTimeRemaining <= 0f)
        {
            // Dash complete
            isDashing = false;
            dashTimeRemaining = 0f;
            return;
        }

        // Calculate current dash speed using ease-out
        // Start at max speed, ramp down smoothly
        float normalizedTime = 1f - (dashTimeRemaining / DASH_DURATION); // 0 to 1
        float easeOutFactor = 1f - Mathf.Pow(1f - normalizedTime, 3f); // Cubic ease-out
        float currentDashSpeed = Mathf.Lerp(BASE_SPEED * DASH_SPEED_MULTIPLIER, BASE_SPEED, easeOutFactor);

        // Apply velocity in locked dash direction
        rb.linearVelocity = dashDirection * currentDashSpeed;
    }

    // Public getter for dash state (useful for animation controller)
    public bool IsDashing()
    {
        return isDashing;
    }

    // Public getter for dash progress (useful for tilt animation - 0 to 1)
    public float GetDashProgress()
    {
        if (!isDashing) return 0f;
        return 1f - (dashTimeRemaining / DASH_DURATION);
    }

    // Public getter for dash direction (useful for tilt animation)
    public Vector2 GetDashDirection()
    {
        return dashDirection;
    }

    // Public getter for dash availability (useful for UI)
    public bool CanDash()
    {
        return dashCooldownRemaining <= 0f && !isDashing;
    }

    // Public getter for cooldown progress (useful for UI - 0 to 1)
    public float GetDashCooldownProgress()
    {
        return 1f - Mathf.Clamp01(dashCooldownRemaining / DASH_COOLDOWN);
    }
}