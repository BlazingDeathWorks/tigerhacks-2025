using UnityEngine;
using System.Collections.Generic;

public enum EdgeDirection
{
    Up,
    Right,
    Down,
    Left
}

public class EnemyLancer : MonoBehaviour, IObjectPooler<EnemyLazerIndicator>
{
    public EnemyLazerIndicator Prefab => _prefab;
    [SerializeField] private EnemyLazerIndicator _prefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private LayerMask _laserMask; // what layers the laser can hit

    public Queue<EnemyLazerIndicator> Pool { get; } = new Queue<EnemyLazerIndicator>();

    private Vector2 moveDirection;
    [SerializeField] private float moveSpeed = 2f;
    
    [SerializeField] private float _wallDetectionDistance = 0.5f; // Smaller raycast to detect when near wall
    
    // Simplified state management - only two states needed
    private enum LancerState { MovingToWall, ProbingEdge }
    private LancerState _currentState = LancerState.MovingToWall;
    
    // Sweeping variables
    private Vector2 _sweepStartPos;
    private Vector2 _sweepEndPos;
    private bool _isSweeping = false;
    [SerializeField] private float _sweepSpeed = 1.5f;
    [SerializeField] private float _sweepLaserRate = 0.4f; // Fire laser every 0.4 seconds while sweeping
    private float _lastSweepLaserTime;
    
    // Single edge assignment - never changes
    private EdgeDirection _assignedWall;

    private void Start()
    {
        // Pick one wall and stick with it forever
        _assignedWall = (EdgeDirection)Random.Range(0, 4);
        
        // Set movement direction to reach assigned wall
        moveDirection = _assignedWall switch
        {
            EdgeDirection.Up => Vector2.up,
            EdgeDirection.Right => Vector2.right,
            EdgeDirection.Down => Vector2.down,
            EdgeDirection.Left => Vector2.left,
            _ => Vector2.up
        };
        
        Debug.Log($"Lancer assigned to {_assignedWall} wall forever");
    }

    private void FixedUpdate()
    {
        switch (_currentState)
        {
            case LancerState.MovingToWall:
                HandleMovingToWall();
                break;
                
            case LancerState.ProbingEdge:
                HandleProbingEdge();
                break;
        }
    }
    
    private void HandleMovingToWall()
    {
        // Check if we're close to a wall in our movement direction
        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, moveDirection, _wallDetectionDistance, _laserMask);
        if (wallCheck.collider != null)
        {
            // We've reached the wall - rotate and start probing immediately
            RotateBasedOnWallDirection();
            ObjectPool.Pool(this); // Fire the laser
            SetupEdgeSweeping();
            _currentState = LancerState.ProbingEdge;
            Debug.Log($"Reached {_assignedWall} wall, starting eternal edge probing");
        }
        else
        {
            // Continue moving towards the wall
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }
    
    private void HandleProbingEdge()
    {
        if (!_isSweeping)
        {
            // Move to sweep start position
            transform.position = Vector2.MoveTowards(transform.position, _sweepStartPos, _sweepSpeed * Time.fixedDeltaTime);
            
            if (Vector2.Distance(transform.position, _sweepStartPos) < 0.1f)
            {
                _isSweeping = true;
                _lastSweepLaserTime = Time.time;
                Debug.Log("Started sweeping");
            }
        }
        else
        {
            // Sweep along the edge
            transform.position = Vector2.MoveTowards(transform.position, _sweepEndPos, _sweepSpeed * Time.fixedDeltaTime);
            
            // Fire laser at intervals while sweeping
            if (Time.time - _lastSweepLaserTime >= _sweepLaserRate)
            {
                _lastSweepLaserTime = Time.time;
                ObjectPool.Pool(this);
                Debug.Log("Firing laser while sweeping");
            }
            
            // Check if sweep is complete - just reverse direction and keep going forever
            if (Vector2.Distance(transform.position, _sweepEndPos) < 0.1f)
            {
                // Reverse sweep direction and continue forever on this edge
                Vector2 temp = _sweepStartPos;
                _sweepStartPos = _sweepEndPos;
                _sweepEndPos = temp;
                Debug.Log("Reached end of sweep, reversing direction - staying on this edge forever");
            }
        }
    }
    
    private void SetupEdgeSweeping()
    {
        Vector2 currentPos = transform.position;
        
        // Use transform.right for sweeping perpendicular to the laser direction
        Vector2 rightDirection = transform.right;
        Vector2 leftDirection = -transform.right;
        
        // First, move to the actual wall edge we're supposed to sweep along
        Vector2 wallDirection = -transform.up; // Direction toward the wall we just approached
        RaycastHit2D wallHit = Physics2D.Raycast(currentPos, wallDirection, 5f, _laserMask);
        Vector2 wallEdgePos = wallHit.collider ? 
            (Vector2)wallHit.point + wallHit.normal * _wallDetectionDistance : 
            currentPos + wallDirection * _wallDetectionDistance;
        
        // Now find the TRUE extremes of this wall edge by raycasting from the wall edge position
        RaycastHit2D leftCorner = Physics2D.Raycast(wallEdgePos, leftDirection, 50f, _laserMask);
        RaycastHit2D rightCorner = Physics2D.Raycast(wallEdgePos, rightDirection, 50f, _laserMask);
        
        // Calculate the actual corner positions with safe distance from walls
        Vector2 leftExtreme = leftCorner.collider ? 
            (Vector2)leftCorner.point - leftDirection * _wallDetectionDistance : 
            wallEdgePos + leftDirection * 50f;
        Vector2 rightExtreme = rightCorner.collider ? 
            (Vector2)rightCorner.point - rightDirection * _wallDetectionDistance : 
            wallEdgePos + rightDirection * 50f;
        
        // Calculate distances to determine which direction has more room
        float leftDistance = Vector2.Distance(wallEdgePos, leftExtreme);
        float rightDistance = Vector2.Distance(wallEdgePos, rightExtreme);
        
        // Always start from the direction with MORE distance (more room to explore)
        if (leftDistance >= rightDistance)
        {
            _sweepStartPos = rightExtreme;
            _sweepEndPos = leftExtreme;
            Debug.Log($"Full wall sweep: right to left ({rightDistance:F1} to {leftDistance:F1} units)");
        }
        else
        {
            _sweepStartPos = leftExtreme;
            _sweepEndPos = rightExtreme;
            Debug.Log($"Full wall sweep: left to right ({leftDistance:F1} to {rightDistance:F1} units)");
        }
        
        _isSweeping = false;
        
        Debug.Log($"Full edge sweep from {_sweepStartPos} to {_sweepEndPos} (total distance: {Vector2.Distance(_sweepStartPos, _sweepEndPos):F1})");
    }

    private void RotateBasedOnWallDirection()
    {
        // Rotate based on which wall we approached (not the opposite direction)
        float targetRotation = 0f;
        
        if (moveDirection == Vector2.up)
            targetRotation = 0f;   // Top wall - no rotation
        else if (moveDirection == Vector2.left)
            targetRotation = 90f;  // Left wall - 90 degrees
        else if (moveDirection == Vector2.down)
            targetRotation = 180f; // Bottom wall - 180 degrees
        else if (moveDirection == Vector2.right)
            targetRotation = 270f; // Right wall - 270 degrees
        
        // Apply rotation to the transform
        transform.rotation = Quaternion.Euler(0f, 0f, targetRotation);
    }

    public void OnPooled(EnemyLazerIndicator instance)
    {
        // Raycast from the spawn point forward
        RaycastHit2D hit = Physics2D.Raycast(_spawnPoint.position, _spawnPoint.up, Mathf.Infinity, _laserMask);
        if (!hit.collider)
        {
            instance.gameObject.SetActive(false);
            return;
        }

        // Get SpriteRenderer height in world units
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("EnemyLazerIndicator prefab missing SpriteRenderer!");
            return;
        }

        float spriteHeight = sr.sprite.bounds.size.y; // local sprite height
        float distance = Vector2.Distance(_spawnPoint.position, hit.point);

        // Adjust scale based on distance
        Vector3 newScale = instance.transform.localScale;
        newScale.y = distance / spriteHeight;
        instance.transform.localScale = newScale;

        // Position halfway between spawn and hit
        instance.transform.position = (_spawnPoint.position + (Vector3)hit.point) / 2f;

        // Match rotation to spawn
        instance.transform.rotation = _spawnPoint.rotation;

        instance.gameObject.SetActive(true);
        instance.transform.parent = transform;
    }

    private void OnDrawGizmos()
    {
        RaycastHit2D hit = Physics2D.Raycast(_spawnPoint.position, _spawnPoint.up, 100f, _laserMask);
        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_spawnPoint.position, hit.point);
        }
    }
}
