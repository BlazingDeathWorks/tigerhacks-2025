using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float BASE_SPEED = 8f;
    
    [Header("Dash Settings")]
    [SerializeField] private float DASH_SPEED_MULTIPLIER = 3f;
    [SerializeField] private float DASH_DURATION = 0.5f;
    [SerializeField] private float DASH_COOLDOWN = 1f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dashSound;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject dashTrailPrefab;
    [SerializeField] private float dashTrailAnimationLength = 0.5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private Camera mainCamera;
    
    private bool isDashing = false;
    private float dashTimeRemaining = 0f;
    private float dashCooldownRemaining = 0f;
    private Vector2 dashDirection;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RotateTowardsMouse();
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownRemaining <= 0f && !isDashing)
        {
            StartDash();
        }
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
        Vector2 direction = mousePosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
    
    private void HandleNormalMovement()
    {
        rb.linearVelocity = moveInput * BASE_SPEED;
    }
    
    private void StartDash()
    {
        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = moveInput.normalized;
        }
        else
        {
            dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        }
        isDashing = true;
        dashTimeRemaining = DASH_DURATION;
        dashCooldownRemaining = DASH_COOLDOWN;
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
        if (dashTrailPrefab != null)
        {
            float explosionAngle = Mathf.Atan2(-dashDirection.y, -dashDirection.x) * Mathf.Rad2Deg;
            Quaternion trailRotation = Quaternion.Euler(0f, 0f, explosionAngle - 90f);
            GameObject trail = Instantiate(
                dashTrailPrefab,
                transform.position,
                trailRotation
            );
            Destroy(trail, dashTrailAnimationLength);
        }
    }
    
    private void HandleDash()
    {
        dashTimeRemaining -= Time.fixedDeltaTime;
        if (dashTimeRemaining <= 0f)
        {
            isDashing = false;
            dashTimeRemaining = 0f;
            return;
        }
        float normalizedTime = 1f - (dashTimeRemaining / DASH_DURATION);
        float angleToMouse = Mathf.Atan2(mousePosition.y - transform.position.y, mousePosition.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion dashOriginalRotation = Quaternion.Euler(0f, 0f, angleToMouse - 90f);
        Quaternion dashFlippedRotation = dashOriginalRotation * Quaternion.Euler(0, 0, 180);
        if (normalizedTime < 0.5f)
        {
            transform.rotation = dashFlippedRotation;
        }
        else
        {
            float lerpT = Mathf.InverseLerp(0.5f, 1.0f, normalizedTime);
            transform.rotation = Quaternion.Lerp(dashFlippedRotation, dashOriginalRotation, lerpT);
        }
        float currentDashSpeed;
        if (normalizedTime < 0.667f)
        {
            currentDashSpeed = BASE_SPEED * DASH_SPEED_MULTIPLIER;
        }
        else
        {
            float easeProgress = (normalizedTime - 0.667f) / 0.333f;
            float easeOutFactor = 1f - Mathf.Pow(1f - easeProgress, 3f);
            currentDashSpeed = Mathf.Lerp(BASE_SPEED * DASH_SPEED_MULTIPLIER, BASE_SPEED, easeOutFactor);
        }
        rb.linearVelocity = dashDirection * currentDashSpeed;
    }
    
    public bool IsDashing()
    {
        return isDashing;
    }
    
    public float GetDashProgress()
    {
        if (!isDashing) return 0f;
        return 1f - (dashTimeRemaining / DASH_DURATION);
    }
    
    public Vector2 GetDashDirection()
    {
        return dashDirection;
    }
    
    public bool CanDash()
    {
        return dashCooldownRemaining <= 0f && !isDashing;
    }
    
    public float GetDashCooldownProgress()
    {
        return 1f - Mathf.Clamp01(dashCooldownRemaining / DASH_COOLDOWN);
    }
}