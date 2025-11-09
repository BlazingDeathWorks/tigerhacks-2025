using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerRocket : MonoBehaviour, IObjectPoolable<PlayerRocket>
{
    public IObjectPooler<PlayerRocket> ParentObjectPooler { get; set; }

    [Header("Rocket Settings")]
    [SerializeField] private float baseSpeed = 1f;
    [SerializeField] private float accelerationRate = 1.5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float maxLifetime = 3f;

    [Header("Explosion Settings")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody2D rb;
    private float timeSinceSpawned;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        timeSinceSpawned += Time.deltaTime;
        if (timeSinceSpawned >= maxLifetime)
        {
            Explode(); // Explode at end of life
        }
    }

    void FixedUpdate()
    {
        // Exponential growth: speed = base * e^(rate * t)
        float currentSpeed = baseSpeed * Mathf.Exp(accelerationRate * timeSinceSpawned);
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        rb.linearVelocity = transform.up * currentSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the rocket hit something on the enemy layer
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            Explode();
        }
        if (other.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(10);
            Explode();
        }
    }

    private void Explode()
    {
        // Create explosion visual effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Find all enemies in the radius
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Debug.Log($"Rocket explosion hit {enemy.name}");
            // TODO: Apply damage to the enemy
            // enemy.GetComponent<EnemyHealth>()?.TakeDamage(damage);
        }

        // Return the rocket to the pool
        ObjectPool.Return(this);
    }

    // --- IObjectPoolable Implementation ---
    public void OnReturn()
    {
        gameObject.SetActive(false);
        timeSinceSpawned = 0;
    }

    public PlayerRocket ReturnComponent() => this;

    // Draw the explosion radius in the editor for easy setup
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
