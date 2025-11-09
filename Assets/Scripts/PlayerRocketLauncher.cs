using UnityEngine;
using System.Collections.Generic;

public class PlayerRocketLauncher : WeaponBase, IObjectPooler<PlayerRocket>
{
    [Header("Launcher Settings")]
    [SerializeField] private PlayerRocket rocketPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float fireRate = 1f; // Rockets per second

    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 10;
    private int currentAmmo;

    private float nextFireTime = 0f;

    // --- IObjectPooler Implementation ---
    public PlayerRocket Prefab => rocketPrefab;
    public Queue<PlayerRocket> Pool { get; private set; } = new Queue<PlayerRocket>();

    public void OnPooled(PlayerRocket instance)
    {
        instance.transform.position = shootPoint.position;
        instance.transform.rotation = shootPoint.rotation;
        instance.gameObject.SetActive(true);
    }
    // ------------------------------------

    void Awake()
    {
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(currentAmmo);
        }
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            if (Input.GetMouseButtonDown(0) && currentAmmo > 0)
            {
                nextFireTime = Time.time + 1f / fireRate;
                
                currentAmmo--;
                ObjectPool.Pool(this);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateAmmo(currentAmmo);
                }
            }
        }
    }
}
