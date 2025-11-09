using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyCrossfire : MonoBehaviour, IObjectPooler<LargeEnemyBullet>, IObjectPooler<SmallEnemyBullet>
{
    // Explicit interface Prefabs
    LargeEnemyBullet IObjectPooler<LargeEnemyBullet>.Prefab => _largePrefab;
    SmallEnemyBullet IObjectPooler<SmallEnemyBullet>.Prefab => _smallPrefab;

    [SerializeField] private LargeEnemyBullet _largePrefab;
    [SerializeField] private SmallEnemyBullet _smallPrefab;

    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _normalBulletSpawnRate = 0.2f;
    [SerializeField] private float _spiralRate = 3f;
    [SerializeField] private float _spiralTime = 5f;
    [SerializeField] private float _tertiarySpiralTime = 3f;
    
    [Header("Smart Probing Movement")]
    [SerializeField] private float _moveSpeed = 1.5f;        // Movement speed
    [SerializeField] private float _probeDistance = 2f;      // How far to probe each step
    [SerializeField] private float _pauseTime = 0.8f;       // Delay between moves
    [SerializeField] private LayerMask _obstacleMask = -1;   // Layers considered as walls
    [SerializeField] private float _raycastLength = 4f;     // How far the enemy "senses"

    private float _timeSinceLastSpiralTime = 0;
    private float _timeSinceLastNormalBulletSpawn = 0;
    private float _originalNormalBulletSpawnRate;
    private bool _isSpiral = false;
    private bool _isTertiarySpiral = false;
    
    // Smart probing movement variables
    private Vector2 _moveDirection;
    private bool _isMoving = false;
    private Vector2 _startPosition;
    private float _moveStartTime;
    
    private readonly Vector2[] _directions = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        new Vector2(1, 1).normalized,
        new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(-1, -1).normalized
    };

    // Independent pools for both bullet types
    Queue<LargeEnemyBullet> IObjectPooler<LargeEnemyBullet>.Pool { get; } = new Queue<LargeEnemyBullet>();
    Queue<SmallEnemyBullet> IObjectPooler<SmallEnemyBullet>.Pool { get; } = new Queue<SmallEnemyBullet>();

    private int _spawnIndex = 0;

    private void Awake()
    {
        _originalNormalBulletSpawnRate = _normalBulletSpawnRate;
        
        // Initialize smart probing movement
        Invoke(nameof(ChooseNewDirection), Random.Range(0f, _pauseTime));
    }

    private void Update()
    {
        if (_isTertiarySpiral)
        {
            // Fastest spiral firing
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);
            _normalBulletSpawnRate = 0.05f;
        }
        else if (_isSpiral)
        {
            // Normal spiral firing
            transform.Rotate(0f, 0f, 180f * Time.deltaTime);
            _normalBulletSpawnRate = 0.2f;
        }
        else
        {
            // Regular pattern
            _timeSinceLastSpiralTime += Time.deltaTime;
        }
        
        // Handle smart probing movement
        if (_isMoving)
        {
            HandleMovement();
        }

        _timeSinceLastNormalBulletSpawn += Time.deltaTime;

        // Bullet spawning logic
        if (_timeSinceLastNormalBulletSpawn >= _normalBulletSpawnRate)
        {
            _timeSinceLastNormalBulletSpawn = 0;

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_isTertiarySpiral)
                    ObjectPool.Pool((IObjectPooler<SmallEnemyBullet>)this);
                else if (_isSpiral)
                    ObjectPool.Pool((IObjectPooler<SmallEnemyBullet>)this);
                else
                    ObjectPool.Pool((IObjectPooler<LargeEnemyBullet>)this);
            }
        }

        // Switch to spiral mode after delay
        if (_timeSinceLastSpiralTime >= _spiralRate && !_isSpiral && !_isTertiarySpiral)
        {
            _timeSinceLastSpiralTime = 0;
            _isSpiral = true;
            StartCoroutine(HandleSpiralModes());
        }
    }

    private void HandleMovement()
    {
        Vector2 currentPos = transform.position;
        Vector2 newPos = currentPos + _moveDirection * _moveSpeed * Time.deltaTime;

        // Check for obstacles directly in front
        RaycastHit2D hit = Physics2D.Raycast(currentPos, _moveDirection, _moveSpeed * Time.deltaTime + 0.1f, _obstacleMask);
        if (hit.collider != null)
        {
            _isMoving = false;
            Invoke(nameof(ChooseNewDirection), _pauseTime);
            return;
        }

        transform.position = newPos;

        // Stop after traveling probeDistance
        if (Vector2.Distance(_startPosition, newPos) >= _probeDistance)
        {
            _isMoving = false;
            Invoke(nameof(ChooseNewDirection), _pauseTime);
        }
    }

    private void ChooseNewDirection()
    {
        // Cast rays in all 8 directions to check distances to walls
        List<float> distances = new List<float>();
        Vector2 currentPos = transform.position;
        
        foreach (Vector2 dir in _directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, dir, _raycastLength, _obstacleMask);
            distances.Add(hit.collider ? hit.distance : _raycastLength);
        }

        // Pick a random direction weighted by distance to walls
        float totalDistance = 0f;
        foreach (float d in distances) 
            totalDistance += d;

        if (totalDistance > 0f)
        {
            float rand = Random.Range(0f, totalDistance);
            float sum = 0f;
            int chosenIndex = 0;
            
            for (int i = 0; i < distances.Count; i++)
            {
                sum += distances[i];
                if (rand <= sum)
                {
                    chosenIndex = i;
                    break;
                }
            }

            _moveDirection = _directions[chosenIndex].normalized;
        }
        else
        {
            // Fallback: choose random direction if all directions are blocked
            _moveDirection = _directions[Random.Range(0, _directions.Length)];
        }

        _startPosition = transform.position;
        _moveStartTime = Time.time;
        _isMoving = true;
    }

    private IEnumerator HandleSpiralModes()
    {
        // First: normal spiral
        yield return new WaitForSecondsRealtime(_spiralTime);
        _isSpiral = false;
        _isTertiarySpiral = true;

        // Then: tertiary spiral (fast bullets)
        yield return new WaitForSecondsRealtime(_tertiarySpiralTime);
        _isTertiarySpiral = false;
        _normalBulletSpawnRate = _originalNormalBulletSpawnRate;
    }

    // --- Explicit interface implementations ---
    void IObjectPooler<LargeEnemyBullet>.OnPooled(LargeEnemyBullet instance)
    {
        instance.transform.position = _spawnPoints[_spawnIndex].position;
        instance.transform.rotation = _spawnPoints[_spawnIndex].rotation;
        instance.gameObject.SetActive(true);
        _spawnIndex = (_spawnIndex + 1) % _spawnPoints.Length;
    }

    void IObjectPooler<SmallEnemyBullet>.OnPooled(SmallEnemyBullet instance)
    {
        instance.transform.position = _spawnPoints[_spawnIndex].position;
        instance.transform.rotation = _spawnPoints[_spawnIndex].rotation;
        instance.gameObject.SetActive(true);
        _spawnIndex = (_spawnIndex + 1) % _spawnPoints.Length;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw raycast directions for debugging
        Gizmos.color = Color.yellow;
        foreach (Vector2 dir in _directions)
        {
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * _raycastLength);
        }
        
        // Draw current movement direction
        if (_isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + _moveDirection * _probeDistance);
        }
    }
}
