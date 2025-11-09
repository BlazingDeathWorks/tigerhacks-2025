using UnityEngine;

public class LargeEnemyBullet : MonoBehaviour, IObjectPoolable<LargeEnemyBullet>
{
    [SerializeField] private float _speed = 1;
    private Rigidbody2D _rb;
    public IObjectPooler<LargeEnemyBullet> ParentObjectPooler { get; set; }
    private LargeEnemyBullet _instance = null;

    private float _lifetime = 3;
    private float _timeSinceAlive = 0;

    public void OnReturn() => gameObject.SetActive(false);

    public LargeEnemyBullet ReturnComponent() => _instance;

    private void Awake()
    {
        _instance = this;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _timeSinceAlive += Time.deltaTime;
        if (_timeSinceAlive >= _lifetime)
        {
            _timeSinceAlive = 0;
            ObjectPool.Return(this);
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = transform.up * _speed;
    }
}
