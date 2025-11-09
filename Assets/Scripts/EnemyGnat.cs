using UnityEngine;
using System.Collections.Generic;

public class EnemyGnat : MonoBehaviour, IObjectPooler<SmallEnemyBullet>
{
    public SmallEnemyBullet Prefab => _smallBulletPrefab;
    [SerializeField] private SmallEnemyBullet _smallBulletPrefab;
    public Queue<SmallEnemyBullet> Pool { get; private set; } = new Queue<SmallEnemyBullet>();
    [SerializeField] private float _bulletSpawnRate = 0.3f;
    private float _timeSinceLastSpawn = 0;
    [SerializeField] private Transform _spawnPoint = null;
    
    [Header("Targeting Settings")]
    [SerializeField] private float _targetUpdateRate = 3f; // Update target every 3 seconds
    [SerializeField] private float _bulletSpreadAngle = 30f; // 30 degree variation
    [SerializeField] private float _rotationSpeed = 2f; // How fast to rotate toward player
    
    [Header("Probing Movement")]
    [SerializeField] private float _probeSpeed = 2f; // Movement speed during probing
    [SerializeField] private float _probeDuration = 1.5f; // How long to move in one direction
    [SerializeField] private float _pauseDuration = 0.5f; // How long to pause between probes
    [SerializeField] private float _wallDetectionDistance = 1f; // How far to check for walls
    [SerializeField] private LayerMask _wallLayerMask = -1; // Wall layer mask
    
    private GameObject _player;
    private Vector2 _targetPosition;
    private float _timeSinceLastTargetUpdate = 0;
    private float _baseRotationAngle = 0f;
    private float _targetRotationAngle = 0f;
    
    // Probing movement variables
    private Vector2 _probeDirection;
    private float _probeTimer;
    private float _pauseTimer;
    private bool _isProbing; // True when moving, false when pausing
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _player = GameObject.Find("Player");
        _rigidbody = GetComponent<Rigidbody2D>();
        
        if (_player == null)
        {
            Debug.LogError("Player not found! Make sure there's a GameObject named 'Player' in the scene.");
        }
        
        if (_rigidbody == null)
        {
            Debug.LogError("Rigidbody2D not found! EnemyGnat needs a Rigidbody2D component for probing movement.");
        }
        
        // Initialize probing movement
        InitializeProbing();
    }

    private void Update()
    {
        if (_spawnPoint == null) return;

        // Timer to update target rotation toward player every few seconds
        _timeSinceLastTargetUpdate += Time.deltaTime;
        if (_timeSinceLastTargetUpdate >= _targetUpdateRate && _player != null)
        {
            _timeSinceLastTargetUpdate = 0;
            
            // Calculate new target rotation toward player
            Vector2 directionToPlayer = (_player.transform.position - transform.position).normalized;
            _targetRotationAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
        }

        // Smoothly lerp base rotation toward target rotation
        _baseRotationAngle = Mathf.LerpAngle(_baseRotationAngle, _targetRotationAngle, _rotationSpeed * Time.deltaTime);

        // Apply oscillation on top of smoothly rotating base rotation
        float oscillationAngle = Mathf.Sin(Time.time * 2f) * 30f; // 30 degrees left/right, speed = 2
        transform.rotation = Quaternion.Euler(0f, 0f, _baseRotationAngle + oscillationAngle);

        // Handle probing movement
        HandleProbingMovement();

        _timeSinceLastSpawn += Time.deltaTime;
        if (_timeSinceLastSpawn >= _bulletSpawnRate)
        {
            _timeSinceLastSpawn = 0;
            ObjectPool.Pool(this);
        }
    }

    private void HandleProbingMovement()
    {
        if (_rigidbody == null) return;

        if (_isProbing)
        {
            // Currently moving - check for walls and move
            _probeTimer -= Time.deltaTime;
            
            // Check for wall collision in current direction
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _probeDirection, _wallDetectionDistance, _wallLayerMask);
            
            if (hit.collider != null)
            {
                // Hit a wall - reflect direction and continue probing
                Vector2 reflectedDirection = Vector2.Reflect(_probeDirection, hit.normal);
                _probeDirection = reflectedDirection.normalized;
            }
            
            // Move in the current probe direction
            _rigidbody.linearVelocity = _probeDirection * _probeSpeed;
            
            // Check if probe duration is over
            if (_probeTimer <= 0f)
            {
                _isProbing = false;
                _pauseTimer = _pauseDuration;
                _rigidbody.linearVelocity = Vector2.zero; // Stop movement
            }
        }
        else
        {
            // Currently pausing
            _pauseTimer -= Time.deltaTime;
            
            if (_pauseTimer <= 0f)
            {
                // Start new probe
                _isProbing = true;
                _probeTimer = _probeDuration;
                
                // Choose a semi-random direction with some intelligence
                ChooseNewProbeDirection();
            }
        }
    }
    
    private void ChooseNewProbeDirection()
    {
        // Generate a few random directions and pick the one with the most open space
        Vector2 bestDirection = Vector2.right;
        float maxDistance = 0f;
        
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f; // Check 8 directions around the enemy
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _wallDetectionDistance * 2f, _wallLayerMask);
            float distance = hit.collider != null ? hit.distance : _wallDetectionDistance * 2f;
            
            if (distance > maxDistance)
            {
                maxDistance = distance;
                bestDirection = direction;
            }
        }
        
        // Add some randomness to the best direction
        float randomAngle = Random.Range(-30f, 30f);
        float finalAngle = Mathf.Atan2(bestDirection.y, bestDirection.x) * Mathf.Rad2Deg + randomAngle;
        _probeDirection = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;
    }
    
    public void InitializeProbing()
    {
        // Initialize probing movement when enemy becomes active
        _isProbing = false;
        _pauseTimer = Random.Range(0f, _pauseDuration); // Start with random pause
        _probeTimer = 0f;
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    public void OnPooled(SmallEnemyBullet instance)
    {
        instance.transform.position = _spawnPoint.position;
        instance.transform.localEulerAngles = transform.localEulerAngles;
        instance.gameObject.SetActive(true);
    }
}
