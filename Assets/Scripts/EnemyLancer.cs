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
    
    // State management for proper wall reaching behavior
    private enum LancerState { MovingToWall, AtWall, ProbingEdge }
    private LancerState _currentState = LancerState.MovingToWall;
    
    // Sweeping variables
    private Vector2 _sweepStartPos;
    private Vector2 _sweepEndPos;
    private bool _isSweeping = false;
    [SerializeField] private float _sweepSpeed = 1.5f;
    [SerializeField] private float _sweepLaserRate = 0.4f; // Fire laser every 0.4 seconds while sweeping
    private float _lastSweepLaserTime;
    
    // Current wall tracking for adjacent-only movement
    private EdgeDirection _currentWall;

    private void Start()
    {
        // Initialize with a random starting wall
        _currentWall = (EdgeDirection)Random.Range(0, 4);
        
        // Pick initial direction (will be adjacent to starting position)
        PickNewWallDirection();
    }

    private void FixedUpdate()
    {
        switch (_currentState)
        {
            case LancerState.MovingToWall:
                HandleMovingToWall();
                break;
                
            case LancerState.AtWall:
                HandleAtWall();
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
            // Update current wall based on movement direction
            _currentWall = moveDirection switch
            {
                var dir when dir == Vector2.up => EdgeDirection.Up,
                var dir when dir == Vector2.right => EdgeDirection.Right,
                var dir when dir == Vector2.down => EdgeDirection.Down,
                var dir when dir == Vector2.left => EdgeDirection.Left,
                _ => EdgeDirection.Up
            };
            
            // We've reached the wall - rotate based on which wall we approached
            RotateBasedOnWallDirection();
            ObjectPool.Pool(this); // Fire the laser
            _currentState = LancerState.AtWall;
            Debug.Log($"Reached wall: {_currentWall}, firing laser");
        }
        else
        {
            // Continue moving towards the wall
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }
    
    private void HandleAtWall()
    {
        // Set up sweeping along the edge
        SetupEdgeSweeping();
        _currentState = LancerState.ProbingEdge;
        Debug.Log("Starting edge probe");
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
            
            // Check if sweep is complete
            if (Vector2.Distance(transform.position, _sweepEndPos) < 0.1f)
            {
                // Finished sweeping, pick a new wall to go to
                _isSweeping = false;
                PickNewWallDirection();
                _currentState = LancerState.MovingToWall;
                Debug.Log("Finished sweeping, picking new wall");
            }
        }
    }
    
    private void SetupEdgeSweeping()
    {
        Vector2 currentPos = transform.position;
        
        // Use transform.right for sweeping perpendicular to the laser direction
        // This will always be correct regardless of rotation
        Vector2 rightDirection = transform.right;
        Vector2 leftDirection = -transform.right;
        
        // Find the sweep boundaries by raycasting in both perpendicular directions
        RaycastHit2D hit1 = Physics2D.Raycast(currentPos, leftDirection, 20f, _laserMask);
        RaycastHit2D hit2 = Physics2D.Raycast(currentPos, rightDirection, 20f, _laserMask);
        
        float leftDistance = hit1.collider ? hit1.distance - _wallDetectionDistance : 20f - _wallDetectionDistance;
        float rightDistance = hit2.collider ? hit2.distance - _wallDetectionDistance : 20f - _wallDetectionDistance;
        
        Vector2 leftExtreme = currentPos + leftDirection * leftDistance;
        Vector2 rightExtreme = currentPos + rightDirection * rightDistance;
        
        // Always start from the direction with MORE distance (more room to explore)
        if (leftDistance >= rightDistance)
        {
            // More room to the left, so start from the right extreme and sweep left
            _sweepStartPos = rightExtreme;
            _sweepEndPos = leftExtreme;
            Debug.Log($"More room left ({leftDistance} vs {rightDistance}), sweeping right to left");
        }
        else
        {
            // More room to the right, so start from the left extreme and sweep right
            _sweepStartPos = leftExtreme;
            _sweepEndPos = rightExtreme;
            Debug.Log($"More room right ({rightDistance} vs {leftDistance}), sweeping left to right");
        }
        
        _isSweeping = false;
        
        Debug.Log($"Setup sweep from {_sweepStartPos} to {_sweepEndPos}");
    }
    
    private void PickNewWallDirection()
    {
        // Get only adjacent walls (neighbors) based on current wall
        EdgeDirection[] adjacentWalls = GetAdjacentWalls(_currentWall);
        
        // Raycast only in adjacent directions
        Vector2[] directions = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        float[] distances = new float[adjacentWalls.Length];
        
        for (int i = 0; i < adjacentWalls.Length; i++)
        {
            int dirIndex = (int)adjacentWalls[i];
            RaycastHit2D hit = Physics2D.Raycast(_spawnPoint.position, directions[dirIndex], 100f, _laserMask);
            distances[i] = hit.collider ? hit.distance : 100f;
        }
        
        // Pick from adjacent walls based on distance weighting
        EdgeDirection chosen = PickAdjacentWallByDistance(adjacentWalls, distances);
        _currentWall = chosen; // Update current wall
        
        // Convert enum to Vector2
        moveDirection = chosen switch
        {
            EdgeDirection.Up => Vector2.up,
            EdgeDirection.Right => Vector2.right,
            EdgeDirection.Down => Vector2.down,
            EdgeDirection.Left => Vector2.left,
            _ => Vector2.zero
        };
        
        Debug.Log($"Current wall: {_currentWall}, chose adjacent wall: {chosen}");
    }
    
    private EdgeDirection[] GetAdjacentWalls(EdgeDirection currentWall)
    {
        // Return only the two adjacent walls (no diagonal movement)
        return currentWall switch
        {
            EdgeDirection.Up => new EdgeDirection[] { EdgeDirection.Left, EdgeDirection.Right },
            EdgeDirection.Right => new EdgeDirection[] { EdgeDirection.Up, EdgeDirection.Down },
            EdgeDirection.Down => new EdgeDirection[] { EdgeDirection.Left, EdgeDirection.Right },
            EdgeDirection.Left => new EdgeDirection[] { EdgeDirection.Up, EdgeDirection.Down },
            _ => new EdgeDirection[] { EdgeDirection.Up, EdgeDirection.Right } // fallback
        };
    }
    
    private EdgeDirection PickAdjacentWallByDistance(EdgeDirection[] adjacentWalls, float[] distances)
    {
        // Weighted random selection from adjacent walls only
        float totalDistance = 0f;
        foreach (float d in distances) 
            totalDistance += d;
        
        if (totalDistance > 0f)
        {
            float rand = Random.Range(0f, totalDistance);
            float sum = 0f;
            
            for (int i = 0; i < distances.Length; i++)
            {
                sum += distances[i];
                if (rand <= sum)
                {
                    return adjacentWalls[i];
                }
            }
        }
        
        // Fallback: return first adjacent wall
        return adjacentWalls[0];
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
