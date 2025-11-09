using UnityEngine;

public enum RoomType
{
    ItemRoom,      // Just items, no enemies
    CombatRoom,    // Enemies that spawn items on clear
    StartRoom,     // No items or enemies
    BossRoom       // Boss fight
}

public class RoomController : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private RoomType roomType;
    [SerializeField] private ItemChoiceManager itemChoiceManager;
    
    [Header("Combat Room Settings")]
    [SerializeField] private GameObject[] enemySpawnPoints;
    private int enemiesRemaining = 0;
    private bool roomCleared = false;
    
    void Start()
    {
        // If item room, spawn items immediately
        if (roomType == RoomType.ItemRoom)
        {
            SpawnItems();
        }
        // If combat room, spawn enemies
        else if (roomType == RoomType.CombatRoom)
        {
            SpawnEnemies();
        }
    }
    
    private void SpawnItems()
    {
        if (itemChoiceManager != null)
        {
            itemChoiceManager.SpawnItemChoice();
        }
        else
        {
            Debug.LogError("ItemChoiceManager not assigned to RoomController!");
        }
    }
    
    private void SpawnEnemies()
    {
        // TODO: Implement enemy spawning
        // For now, simulate enemies for testing
        enemiesRemaining = enemySpawnPoints?.Length ?? 0;
        Debug.Log($"Spawned {enemiesRemaining} enemies");
    }
    
    public void OnEnemyKilled()
    {
        if (roomType != RoomType.CombatRoom) return;
        
        enemiesRemaining--;
        
        if (enemiesRemaining <= 0 && !roomCleared)
        {
            OnRoomCleared();
        }
    }
    
    private void OnRoomCleared()
    {
        roomCleared = true;
        Debug.Log("Room cleared! Spawning items...");
        SpawnItems();
    }
    
    // For testing - call this to simulate room clear
    public void TestClearRoom()
    {
        if (roomType == RoomType.CombatRoom && !roomCleared)
        {
            OnRoomCleared();
        }
    }
}