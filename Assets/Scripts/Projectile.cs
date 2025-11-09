using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Rigidbody2D rb;
    
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void Initialize(float damage, float speed, float size)
    {
        this.damage = damage;
        this.speed = speed;
        transform.localScale = Vector3.one * size;
        
        // Move in the direction it's facing
        if (rb != null)
        {
            rb.linearVelocity = transform.up * speed;
        }
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit enemy
        if (other.CompareTag("Enemy"))
        {
            // TODO: Apply damage to enemy
            // Enemy enemy = other.GetComponent<Enemy>();
            // if (enemy != null) enemy.TakeDamage(damage);
            
            Destroy(gameObject);
        }
        // Check if hit wall
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}