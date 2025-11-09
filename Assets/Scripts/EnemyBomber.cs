using System.Collections.Generic;
using UnityEngine;

public class EnemyBomber : MonoBehaviour, IObjectPooler<EnemyRocket>
{
    public EnemyRocket Prefab => _prefab;
    [SerializeField] private EnemyRocket _prefab;

    public Queue<EnemyRocket> Pool { get; } = new Queue<EnemyRocket>();
    [SerializeField] private float _bulletSpawnRate = 3;
    private float _timeSinceLastSpawn = 0;
    [SerializeField] private Transform _spawnPoint;
    
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    
    private GameObject _player;

    private void Awake()
    {
        _player = GameObject.Find("Player");
        
        if (_player == null)
        {
            Debug.LogError("Player not found! Make sure there's a GameObject named 'Player' in the scene.");
        }
    }

    private void Update()
    {
        if (_player != null)
        {
            // Move towards the player
            Vector3 targetPosition = _player.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
            
            // Rotate to face the player
            Vector2 directionToPlayer = (targetPosition - transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        _timeSinceLastSpawn += Time.deltaTime;
        if (_timeSinceLastSpawn >= _bulletSpawnRate)
        {
            _timeSinceLastSpawn = 0;
            ObjectPool.Pool(this);
        }
    }

    public void OnPooled(EnemyRocket instance)
    {
        instance.transform.position = _spawnPoint.position;
        instance.transform.rotation = transform.rotation; // Use bomber's world rotation
        instance.gameObject.SetActive(true);
    }
}
