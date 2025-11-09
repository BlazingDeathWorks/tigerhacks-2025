using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRocket : MonoBehaviour, IObjectPoolable<EnemyRocket>
{
    public IObjectPooler<EnemyRocket> ParentObjectPooler { get; set; }

    [Header("Rocket Settings")]
    [SerializeField] private float baseSpeed = 1f;          // Starting speed
    [SerializeField] private float accelerationRate = 1.5f; // Exponential growth rate
    [SerializeField] private float maxSpeed = 20f;          // Clamp to avoid infinite speed

    private Rigidbody2D rb;
    private EnemyRocket _instance = null;
    private float lifetime; // time since spawned
    private float _lifetime = 3;
    private float _timeSinceAlive;
    public void OnReturn() => gameObject.SetActive(false);

    public EnemyRocket ReturnComponent() => _instance;

    private void Awake()
    {
        _instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _timeSinceAlive += Time.deltaTime;
        if (_timeSinceAlive >= _lifetime)
        {
            _timeSinceAlive = 0;
            lifetime = 0;
            ObjectPool.Return(this);
        }
    }

    private void FixedUpdate()
    {
        // Increase lifetime each frame
        lifetime += Time.fixedDeltaTime;

        // Exponential growth: speed = base * e^(rate * t)
        float currentSpeed = baseSpeed * Mathf.Exp(accelerationRate * lifetime);

        // Clamp the speed for safety
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        // Move in the rocket's facing direction (up)
        rb.linearVelocity = transform.up * currentSpeed;
    }
}
