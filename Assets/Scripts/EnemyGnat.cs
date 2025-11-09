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

    private void Update()
    {
        if (_spawnPoint == null) return;

        // Oscillate EnemyGnat rotation left/right
        float angle = Mathf.Sin(Time.time * 2f) * 30f; // 30 degrees left/right, speed = 2
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        _timeSinceLastSpawn += Time.deltaTime;
        if (_timeSinceLastSpawn >= _bulletSpawnRate)
        {
            _timeSinceLastSpawn = 0;
            ObjectPool.Pool(this);
        }
    }


    public void OnPooled(SmallEnemyBullet instance)
    {
        instance.transform.position = _spawnPoint.position;
        instance.transform.localEulerAngles = transform.localEulerAngles;
        instance.gameObject.SetActive(true);
    }
}
