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

    private float _timeSinceLastSpiralTime = 0;
    private float _timeSinceLastNormalBulletSpawn = 0;
    private float _originalNormalBulletSpawnRate;
    private bool _isSpiral = false;
    private bool _isTertiarySpiral = false;

    // Independent pools for both bullet types
    Queue<LargeEnemyBullet> IObjectPooler<LargeEnemyBullet>.Pool { get; } = new Queue<LargeEnemyBullet>();
    Queue<SmallEnemyBullet> IObjectPooler<SmallEnemyBullet>.Pool { get; } = new Queue<SmallEnemyBullet>();

    private int _spawnIndex = 0;

    private void Awake()
    {
        _originalNormalBulletSpawnRate = _normalBulletSpawnRate;
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
}
