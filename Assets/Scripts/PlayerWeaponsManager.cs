using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Inventory")]
    [SerializeField] private List<WeaponData> ownedWeapons = new List<WeaponData>();
    [SerializeField] private WeaponData startingWeapon; // Assign basic blaster
    private int currentWeaponIndex = 0;
    
    [Header("Current Weapon State")]
    private Dictionary<WeaponData, int> weaponAmmo = new Dictionary<WeaponData, int>();
    private float lastFireTime = 0f;
    
    [Header("References")]
    private PlayerStatsManager statsManager;
    [SerializeField] private Transform firePoint; // Where projectiles spawn
    [SerializeField] private AudioSource audioSource;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer weaponSpriteRenderer; // Visual representation of weapon
    
    void Start()
    {
        statsManager = GetComponent<PlayerStatsManager>();
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Add starting weapon
        if (startingWeapon != null)
        {
            AddWeapon(startingWeapon);
        }
    }
    
    void Update()
    {
        HandleWeaponSwitching();
        HandleShooting();
    }
    
    private void HandleWeaponSwitching()
    {
        if (ownedWeapons.Count <= 1) return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon(-1); // Cycle left
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            CycleWeapon(1); // Cycle right
        }
    }
    
    private void CycleWeapon(int direction)
    {
        currentWeaponIndex += direction;
        
        if (currentWeaponIndex < 0)
            currentWeaponIndex = ownedWeapons.Count - 1;
        else if (currentWeaponIndex >= ownedWeapons.Count)
            currentWeaponIndex = 0;
            
        UpdateWeaponVisual();
    }
    
    private void HandleShooting()
    {
        if (ownedWeapons.Count == 0) return;
        
        WeaponData currentWeapon = ownedWeapons[currentWeaponIndex];
        
        // Check fire rate
        float modifiedFireRate = currentWeapon.fireRate * statsManager.fireRateMultiplier;
        float timeBetweenShots = 1f / modifiedFireRate;
        
        if (Input.GetMouseButton(0) && Time.time >= lastFireTime + timeBetweenShots)
        {
            TryShoot();
        }
    }
    
    private void TryShoot()
    {
        WeaponData currentWeapon = ownedWeapons[currentWeaponIndex];
        
        // Check ammo
        if (!currentWeapon.infiniteAmmo)
        {
            if (!weaponAmmo.ContainsKey(currentWeapon) || weaponAmmo[currentWeapon] <= 0)
            {
                PlayEmptySound(currentWeapon);
                return;
            }
            
            weaponAmmo[currentWeapon]--;
        }
        
        lastFireTime = Time.time;
        Shoot(currentWeapon);
    }
    
    private void Shoot(WeaponData weapon)
    {
        // Spawn projectile
        if (weapon.projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(weapon.projectilePrefab, firePoint.position, transform.rotation);
            // Removed call to Initialize as NormalPlayerBullet handles its own setup in Awake
        }
        
        // Play sound
        if (weapon.fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weapon.fireSound);
        }
    }
    
    private void PlayEmptySound(WeaponData weapon)
    {
        if (weapon.emptySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weapon.emptySound);
        }
    }
    
    public void AddWeapon(WeaponData weapon)
    {
        if (ownedWeapons.Contains(weapon))
        {
            // Already own this weapon, just refill ammo
            if (!weapon.infiniteAmmo)
            {
                int maxAmmo = Mathf.RoundToInt(weapon.maxAmmo * statsManager.maxAmmoMultiplier);
                weaponAmmo[weapon] = maxAmmo;
            }
            return;
        }
        
        ownedWeapons.Add(weapon);
        
        // Initialize ammo
        if (!weapon.infiniteAmmo)
        {
            int maxAmmo = Mathf.RoundToInt(weapon.maxAmmo * statsManager.maxAmmoMultiplier);
            weaponAmmo[weapon] = maxAmmo;
        }
        
        // If this is the first weapon, select it
        if (ownedWeapons.Count == 1)
        {
            currentWeaponIndex = 0;
            UpdateWeaponVisual();
        }
    }
    
    public bool OwnsWeapon(WeaponData weapon)
    {
        return ownedWeapons.Contains(weapon);
    }
    
    public WeaponData GetCurrentWeapon()
    {
        if (ownedWeapons.Count == 0) return null;
        return ownedWeapons[currentWeaponIndex];
    }
    
    public int GetCurrentAmmo()
    {
        WeaponData current = GetCurrentWeapon();
        if (current == null) return 0;
        if (current.infiniteAmmo) return -1; // -1 indicates infinite
        
        if (weaponAmmo.ContainsKey(current))
            return weaponAmmo[current];
        return 0;
    }
    
    private void UpdateWeaponVisual()
    {
        if (weaponSpriteRenderer != null && ownedWeapons.Count > 0)
        {
            WeaponData current = ownedWeapons[currentWeaponIndex];
            weaponSpriteRenderer.sprite = current.weaponSprite;
        }
    }
}
