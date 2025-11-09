using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemType itemData;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Particle")]
    [SerializeField] private GameObject pickupParticlePrefab;
    
    private Vector3 startPosition;
    private ItemChoiceManager choiceManager;
    private bool isBeingDestroyed = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (itemData != null && iconRenderer != null)
        {
            iconRenderer.sprite = itemData.itemIcon;
            
            if (glowRenderer != null)
            {
                glowRenderer.color = itemData.itemGlowColor;
            }
        }
    }
    
    void Update()
    {
        // Floating animation
        if (!isBeingDestroyed)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    public void Initialize(ItemType item, ItemChoiceManager manager)
    {
        itemData = item;
        choiceManager = manager;
        
        if (iconRenderer != null)
        {
            iconRenderer.sprite = itemData.itemIcon;
        }
        
        if (glowRenderer != null)
        {
            glowRenderer.color = itemData.itemGlowColor;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isBeingDestroyed) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerStatsManager statsManager = other.GetComponent<PlayerStatsManager>();
            
            if (statsManager != null)
            {
                ApplyItem(statsManager);
                
                // Notify choice manager
                if (choiceManager != null)
                {
                    choiceManager.OnItemPicked(this);
                }
                
                // Play pickup sound
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Spawn particle effect
                if (pickupParticlePrefab != null)
                {
                    Instantiate(pickupParticlePrefab, transform.position, Quaternion.identity);
                }
                
                isBeingDestroyed = true;
                Destroy(gameObject);
            }
        }
    }
    
    private void ApplyItem(PlayerStatsManager stats)
    {
        switch (itemData.category)
        {
            case ItemCategory.StatUpgrade:
                stats.ApplyStatUpgrade(itemData.statType, itemData.statValue, itemData.isPercentage);
                Debug.Log($"Applied {itemData.itemName}: {itemData.statType} + {itemData.statValue}");
                break;
                
            case ItemCategory.Consumable:
                // Handle consumables (like health potions)
                if (itemData.statType == StatType.Health)
                {
                    stats.Heal(itemData.statValue);
                }
                break;
        }
    }
    
    public ItemType GetItemData()
    {
        return itemData;
    }
    
    public void FadeAndDestroy()
    {
        isBeingDestroyed = true;
        // TODO: Add fade animation
        Destroy(gameObject, 0.5f);
    }
}