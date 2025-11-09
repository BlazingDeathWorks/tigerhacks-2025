using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "sop/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public Sprite weaponSprite;
    public Sprite weaponIcon; // For UI
    
    [Header("Ammo")]
    public int maxAmmo = 100;
    public bool infiniteAmmo = false; // For starter weapon
    
    [Header("Damage")]
    public float baseDamage = 10f;
    
    [Header("Fire Rate")]
    public float fireRate = 5f; // Shots per second
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float projectileSize = 1f;
    
    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip emptySound;
    
    // Calculate time between shots
    public float TimeBetweenShots => 1f / fireRate;
}