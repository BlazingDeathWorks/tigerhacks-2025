using UnityEngine;
using System.Collections.Generic;

public class PlayerShooter : WeaponBase, IObjectPooler<NormalPoolableObject>
{
    public NormalPoolableObject Prefab => _prefabReference;
    [SerializeField] private NormalPoolableObject _prefabReference;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _shootInterval = 0.2f;
    private float _timeSinceLastShot = 0f;
    public Queue<NormalPoolableObject> Pool { get; private set; } = new Queue<NormalPoolableObject>();

    void OnEnable()
    {
        if (UIManager.Instance != null)
        {
            // Use -1 as a convention for infinite ammo
            UIManager.Instance.UpdateAmmo(-1);
        }
    }

    private void Awake()
    {
        _timeSinceLastShot = _shootInterval;
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceLastShot += Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            if (_timeSinceLastShot >= _shootInterval)
            {
                ObjectPool.Pool(this);
                _timeSinceLastShot = 0f;
            }
        }
    }

    public void OnPooled(NormalPoolableObject instance)
    {
        instance.transform.position = _shootPoint.position;
        instance.transform.localEulerAngles = transform.localEulerAngles;
        instance.gameObject.SetActive(true);
    }
}
