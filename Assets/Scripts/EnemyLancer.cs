using UnityEngine;
using System.Collections.Generic;

public class EnemyLancer : MonoBehaviour, IObjectPooler<EnemyLazerIndicator>
{
    public EnemyLazerIndicator Prefab => _prefab;
    [SerializeField] private EnemyLazerIndicator _prefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private LayerMask _laserMask; // what layers the laser can hit

    public Queue<EnemyLazerIndicator> Pool { get; } = new Queue<EnemyLazerIndicator>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ObjectPool.Pool(this);
        }
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
