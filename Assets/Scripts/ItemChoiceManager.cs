using System.Collections.Generic;
using UnityEngine;

public class ItemChoiceManager : MonoBehaviour
{
    [Header("Item Pool")]
    [SerializeField] private List<ItemType> allItems = new List<ItemType>();
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;
    [SerializeField] private GameObject itemPickupPrefab;
    
    [Header("Visual Connection")]
    [SerializeField] private LineRenderer connectionLine;
    [SerializeField] private Color connectionColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    
    [Header("Weapon Rarity")]
    [Range(0f, 1f)]
    [SerializeField] private float weaponSpawnChance = 0.1f; // 10% chance for weapons
    
    private ItemPickup leftItem;
    private ItemPickup rightItem;
    private bool itemsSpawned = false;
    
    void Start()
    {
        SetupConnectionLine();
    }
    
    void Update()
    {
        UpdateConnectionLine();
    }
    
    private void SetupConnectionLine()
    {
        if (connectionLine == null)
        {
            connectionLine = gameObject.AddComponent<LineRenderer>();
        }
        
        connectionLine.startColor = connectionColor;
        connectionLine.endColor = connectionColor;
        connectionLine.startWidth = lineWidth;
        connectionLine.endWidth = lineWidth;
        connectionLine.positionCount = 2;
        connectionLine.enabled = false;
        
        // Make sure line renders behind items
        connectionLine.sortingOrder = -1;
    }
    
    public void SpawnItemChoice()
    {
        if (itemsSpawned) return;
        
        // Select two different items
        ItemType item1 = SelectRandomItem();
        ItemType item2 = SelectRandomItem(item1);
        
        // Spawn left item
        GameObject leftObj = Instantiate(itemPickupPrefab, leftSpawnPoint.position, Quaternion.identity);
        leftItem = leftObj.GetComponent<ItemPickup>();
        leftItem.Initialize(item1, this);
        
        // Spawn right item
        GameObject rightObj = Instantiate(itemPickupPrefab, rightSpawnPoint.position, Quaternion.identity);
        rightItem = rightObj.GetComponent<ItemPickup>();
        rightItem.Initialize(item2, this);
        
        itemsSpawned = true;
        connectionLine.enabled = true;
    }
    
    // No longer requires weaponManager
    private ItemType SelectRandomItem(ItemType excludeItem = null)
    {
        List<ItemType> availableItems = new List<ItemType>();
        
        // Determine if this should be a weapon roll
        bool rollWeapon = Random.value < weaponSpawnChance;
        
        foreach (ItemType item in allItems)
        {
            // Skip excluded item
            if (item == excludeItem) continue;
            
            // If you want to allow only stat upgrades, filter here:
            if (!rollWeapon && item.category == ItemCategory.StatUpgrade) availableItems.Add(item);
            else if (rollWeapon && item.category == ItemCategory.Weapon) availableItems.Add(item);
            else if (item.category == ItemCategory.Consumable) availableItems.Add(item); // Always let consumable fill if needed
        }
        
        // If no items available (e.g., all weapons owned), fall back to stat upgrades
        if (availableItems.Count == 0)
        {
            foreach (ItemType item in allItems)
            {
                if (item == excludeItem) continue;
                availableItems.Add(item);
            }
        }
        
        if (availableItems.Count == 0)
        {
            Debug.LogError("No available items to spawn!");
            return null;
        }
        
        // Weighted random selection
        float totalWeight = 0f;
        foreach (ItemType item in availableItems)
        {
            totalWeight += item.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (ItemType item in availableItems)
        {
            currentWeight += item.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }
        
        return availableItems[availableItems.Count - 1];
    }
    
    private void UpdateConnectionLine()
    {
        if (leftItem != null && rightItem != null && connectionLine.enabled)
        {
            connectionLine.SetPosition(0, leftItem.transform.position);
            connectionLine.SetPosition(1, rightItem.transform.position);
        }
    }
    
    public void OnItemPicked(ItemPickup pickedItem)
    {
        // Destroy the other item
        if (pickedItem == leftItem && rightItem != null)
        {
            rightItem.FadeAndDestroy();
        }
        else if (pickedItem == rightItem && leftItem != null)
        {
            leftItem.FadeAndDestroy();
        }
        
        // Hide connection line
        connectionLine.enabled = false;
        itemsSpawned = false;
    }
}