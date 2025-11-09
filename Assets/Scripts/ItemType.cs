using UnityEngine;

[System.Serializable]
public enum ItemCategory
{
    StatUpgrade,
    Weapon,
    Consumable
}

[System.Serializable]
public enum StatType
{
    Health,
    MaxHealth,
    MaxAmmo,
    Damage,
    FireRate,
    MoveSpeed,
    DashCooldown
}

[CreateAssetMenu(fileName = "New Item", menuName = "sop/Item")]
public class ItemType : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public Sprite itemIcon;
    public ItemCategory category;
    
    [Header("Stat Upgrade Settings (if StatUpgrade)")]
    public StatType statType;
    public float statValue; // For percentage: 0.05 = 5%, for flat: just the number
    public bool isPercentage = true;
    
    [Header("Weapon Settings (if Weapon)")]
    public WeaponData weaponData;
    
    [Header("Rarity")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f; // Higher = more common
    
    [Header("Visual")]
    public Color itemGlowColor = Color.white;
}