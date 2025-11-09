using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCrusher : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeAcceleration = 100f; // Very fast acceleration
    [SerializeField] private float maxChargeSpeed = 50f; // High max speed
    [SerializeField] private float rotationSpeed = 10f; // Faster rotation
    
    [Header("Behavior Settings")]
    [SerializeField] private float chargeDelay = 1f; // Time to look at player before charging
    [SerializeField] private float predictionDistance = 3f; // How far ahead to target
    [SerializeField] private float decelerationRate = 50f; // How fast to slow down after overshooting
    
    [Header("Bezier Curve Settings")]
    [SerializeField] private float curveHeight = 12f; // How high the curve goes (increased for more intensity)
    [SerializeField] private float curveSpeed = 2f; // Speed along the curve
    [SerializeField] private float curveHeightVariation = 0.5f; // Random variation in curve height (0-1)
    
    [Header("Wall Collision Settings")]
    [SerializeField] private LayerMask wallLayerMask = -1; // Layer mask for walls
    [SerializeField] private float wallDetectionDistance = 2f; // How far ahead to check for walls
    [SerializeField] private float enemyRadius = 0.5f; // Radius of the enemy for collision checking
    
    private GameObject player;
    private Rigidbody2D rb;
    private Rigidbody2D playerRb;
    private Vector2 chargeDirection;
    private Vector2 targetPosition;
    private float currentSpeed = 0f;
    private bool isCharging = false;
    private bool isDecelerating = false;
    private float chargeTimer = 0f;
    
    // Bezier curve variables
    private int chargeCount = 0;
    private bool isCurveCharge = false;
    private Vector2 curveStart;
    private Vector2 curveEnd;
    private Vector2 curveControl;
    private float curveProgress = 0f;

    void Awake()
    {
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody2D>();
        
        // Configure Rigidbody2D to prevent falling/gravity issues
        rb.gravityScale = 0f; // Disable gravity
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Only allow position movement
        rb.linearDamping = 0f; // No linear damping to prevent slowing down
        rb.angularDamping = 0f; // No angular damping
        
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure there's a GameObject named 'Player' in the scene.");
        }
        else
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isCharging && !isDecelerating)
        {
            // Look toward player and prepare to charge
            LookAtPlayer();
            chargeTimer += Time.deltaTime;
            
            if (chargeTimer >= chargeDelay)
            {
                StartCharge();
            }
        }
        else if (isCharging)
        {
            if (isCurveCharge)
            {
                // Follow bezier curve path
                ChargeBezierCurve();
            }
            else
            {
                // Continue charging and check if we've passed the target
                ChargeAtPlayer();
                CheckIfPassedTarget();
            }
        }
        else if (isDecelerating)
        {
            // Smoothly decelerate
            DecelerateSmooth();
        }
    }

    private void LookAtPlayer()
    {
        Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
        
        // Smoothly rotate toward the player
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    private void StartCharge()
    {
        isCharging = true;
        isDecelerating = false;
        chargeCount++;
        
        // Unfreeze rigidbody for movement
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Every third charge is a curve charge
        isCurveCharge = (chargeCount % 3 == 0);
        
        // Calculate predicted player position
        Vector2 playerVelocity = playerRb != null ? playerRb.linearVelocity : Vector2.zero;
        Vector2 playerPosition = player.transform.position;
        Vector2 predictedPosition = playerPosition + playerVelocity.normalized * predictionDistance;
        
        targetPosition = predictedPosition;
        
        if (isCurveCharge)
        {
            SetupBezierCurve();
        }
        else
        {
            chargeDirection = (targetPosition - (Vector2)transform.position).normalized;
            currentSpeed = 0f;
        }
    }

    private void ChargeAtPlayer()
    {
        // Accelerate toward the player
        currentSpeed += chargeAcceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxChargeSpeed);
        
        // Apply velocity in the charge direction
        rb.linearVelocity = chargeDirection * currentSpeed;
    }

    private void CheckIfPassedTarget()
    {
        // Calculate if we've passed the target position
        Vector2 currentPos = transform.position;
        Vector2 directionToTarget = (targetPosition - currentPos).normalized;
        
        // If the dot product is negative, we've passed the target
        if (Vector2.Dot(chargeDirection, directionToTarget) < 0f)
        {
            StartDeceleration();
        }
    }

    private void SetupBezierCurve()
    {
        curveStart = transform.position;
        curveEnd = targetPosition;
        
        // Create a control point that's perpendicular to the direct path
        Vector2 midPoint = (curveStart + curveEnd) / 2f;
        Vector2 perpendicular = Vector2.Perpendicular(curveEnd - curveStart).normalized;
        
        // More explicit random side selection with better distribution
        int randomChoice = Random.Range(0, 2); // 0 or 1
        float side = randomChoice == 0 ? -1f : 1f; // Left (-1) or Right (+1)
        
        // Add some angle variation to the perpendicular for even more variety
        float angleVariation = Random.Range(-30f, 30f); // ±30 degrees
        float radians = angleVariation * Mathf.Deg2Rad;
        Vector2 rotatedPerpendicular = new Vector2(
            perpendicular.x * Mathf.Cos(radians) - perpendicular.y * Mathf.Sin(radians),
            perpendicular.x * Mathf.Sin(radians) + perpendicular.y * Mathf.Cos(radians)
        );
        
        // Add random variation to curve height for more intensity
        float randomVariation = Random.Range(1f - curveHeightVariation, 1f + curveHeightVariation);
        float finalCurveHeight = curveHeight * randomVariation;
        
        Vector2 proposedControl = midPoint + rotatedPerpendicular * finalCurveHeight * side;
        
        // Validate that the curve control point doesn't create a path through walls
        if (ValidateBezierPath(curveStart, proposedControl, curveEnd))
        {
            curveControl = proposedControl;
        }
        else
        {
            // Fallback: use a smaller, safer curve or straight path
            curveControl = midPoint + rotatedPerpendicular * (finalCurveHeight * 0.3f) * side;
            
            // If even the smaller curve is blocked, fall back to straight charge
            if (!ValidateBezierPath(curveStart, curveControl, curveEnd))
            {
                // Convert to straight charge
                isCurveCharge = false;
                chargeDirection = (targetPosition - (Vector2)transform.position).normalized;
                currentSpeed = 0f;
                return;
            }
        }
        
        curveProgress = 0f;
        currentSpeed = 0f;
    }

    private void ChargeBezierCurve()
    {
        // Move along the bezier curve
        curveProgress += curveSpeed * Time.deltaTime;
        
        if (curveProgress >= 1f)
        {
            // Reached the end of the curve
            curveProgress = 1f;
            StartDeceleration();
            return;
        }
        
        // Calculate position on bezier curve
        Vector2 newPosition = CalculateBezierPoint(curveProgress, curveStart, curveControl, curveEnd);
        
        // Calculate direction for smooth rotation
        Vector2 nextPosition = CalculateBezierPoint(Mathf.Min(curveProgress + 0.01f, 1f), curveStart, curveControl, curveEnd);
        Vector2 direction = (nextPosition - newPosition).normalized;
        
        // Smooth rotation to follow the curve
        if (direction != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        
        // Move to the new position
        rb.MovePosition(newPosition);
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        // Quadratic Bezier formula: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector2 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        
        return point;
    }

    private bool ValidateBezierPath(Vector2 start, Vector2 control, Vector2 end)
    {
        // Sample points along the bezier curve to check for wall collisions
        int sampleCount = 10;
        
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector2 point = CalculateBezierPoint(t, start, control, end);
            
            // Check if this point would collide with a wall
            Collider2D hit = Physics2D.OverlapCircle(point, enemyRadius, wallLayerMask);
            if (hit != null)
            {
                return false; // Path blocked by wall
            }
        }
        
        return true; // Path is clear
    }

    private void StartDeceleration()
    {
        isCharging = false;
        isDecelerating = true;
    }


    private void DecelerateSmooth()
    {
        // Gradually reduce speed
        currentSpeed -= decelerationRate * Time.deltaTime;
        
        if (currentSpeed <= 0f)
        {
            // Fully stopped, reset for next charge
            ResetCharge();
        }
        else
        {
            // Continue moving but slower
            rb.linearVelocity = chargeDirection * currentSpeed;
        }
    }

    private void ResetCharge()
    {
        isCharging = false;
        isDecelerating = false;
        isCurveCharge = false;
        chargeTimer = 0f;
        currentSpeed = 0f;
        curveProgress = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if ((isCharging || isDecelerating) && player != null)
        {
            if (isCurveCharge)
            {
                // Draw bezier curve
                Gizmos.color = Color.magenta;
                Vector2 previousPoint = curveStart;
                
                for (int i = 1; i <= 20; i++)
                {
                    float t = i / 20f;
                    Vector2 point = CalculateBezierPoint(t, curveStart, curveControl, curveEnd);
                    Gizmos.DrawLine(previousPoint, point);
                    previousPoint = point;
                }
                
                // Draw control point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(curveControl, 0.3f);
                Gizmos.DrawLine(curveStart, curveControl);
                Gizmos.DrawLine(curveControl, curveEnd);
            }
            else
            {
                // Draw straight charge direction
                Gizmos.color = isCharging ? Color.red : Color.orange;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(chargeDirection * 3f));
            }
            
            // Draw target position (predicted player position)
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            
            // Draw player's current position for comparison
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.transform.position, 0.3f);
        }
        
        // Always draw wall collision detection when selected
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);
        
        // Draw ahead detection when charging
        if (isCharging)
        {
            Vector2 movementDirection;
            if (isCurveCharge)
            {
                Vector2 nextPosition = CalculateBezierPoint(Mathf.Min(curveProgress + 0.1f, 1f), curveStart, curveControl, curveEnd);
                movementDirection = (nextPosition - (Vector2)transform.position).normalized;
            }
            else
            {
                movementDirection = chargeDirection;
            }
            
            // Draw ahead detection raycast
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + movementDirection * wallDetectionDistance);
        }
    }
}
