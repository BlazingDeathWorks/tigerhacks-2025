using UnityEngine;

public class NormalPlayerBullet : MonoBehaviour
{
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _lifetime = 4f;
    private float _timeAlive = 0f;
    private NormalPoolableObject _poolableObject;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _poolableObject = GetComponent<NormalPoolableObject>();
    }

    private void Update()
    {
        _timeAlive += Time.deltaTime;
        if (_timeAlive < _lifetime) return;
        _timeAlive = 0f;
        ObjectPool.Return(_poolableObject);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = transform.up * _bulletSpeed;
    }
}
