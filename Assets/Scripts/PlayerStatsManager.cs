using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStatsManager : MonoBehaviour
{
    [Header("Health")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    
    [Header("Movement (references PlayerController)")]
    private PlayerController playerController;
    
    [Header("Stat Multipliers")]
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    public float dashCooldownMultiplier = 1f;
    public float maxAmmoMultiplier = 1f;

    [Header("Map Information")]
    public GameObject MapGenerator;
    public string nextLevelName;


    private bool isDead = false;
    private bool finishedCharFade = false;
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        // Set initial health on the UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (finishedCharFade)
        {
            SceneManager.LoadScene("TitleSene");
        }
    }
    
    public void ApplyStatUpgrade(StatType statType, float value, bool isPercentage)
    {
        switch (statType)
        {
            case StatType.Health:
                if (isPercentage)
                    currentHealth += maxHealth * value;
                else
                    currentHealth += value;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
                break;
                
            case StatType.MaxHealth:
                if (isPercentage)
                    maxHealth += maxHealth * value;
                else
                    maxHealth += value;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
                break;
                
            case StatType.MaxAmmo:
                if (isPercentage)
                    maxAmmoMultiplier += value;
                else
                    maxAmmoMultiplier += value / 100f; // Convert flat to percentage
                break;
                
            case StatType.Damage:
                if (isPercentage)
                    damageMultiplier += value;
                else
                    damageMultiplier += value / 100f;
                break;
                
            case StatType.FireRate:
                if (isPercentage)
                    fireRateMultiplier += value;
                else
                    fireRateMultiplier += value / 100f;
                break;
                
            case StatType.MoveSpeed:
                if (isPercentage)
                    moveSpeedMultiplier += value;
                else
                    moveSpeedMultiplier += value / 100f;
                    
                // Update PlayerController speed
                if (playerController != null)
                {
                    playerController.UpdateMoveSpeed(moveSpeedMultiplier);
                }
                break;
                
            case StatType.DashCooldown:
                // Negative value = faster cooldown
                if (isPercentage)
                    dashCooldownMultiplier -= value;
                else
                    dashCooldownMultiplier -= value / 100f;
                    
                dashCooldownMultiplier = Mathf.Max(0.1f, dashCooldownMultiplier);
                
                if (playerController != null)
                {
                    playerController.UpdateDashCooldown(dashCooldownMultiplier);
                }
                break;
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }
    
    private IEnumerator FadeOut()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float timer = 0f;
        // Ensure the image starts at the correct color and alpha
        Color currentColor = sprite.color;
        currentColor.a = 1;
        sprite.color = currentColor;

        while (timer < 1)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1, 0, timer / 1);
            currentColor.a = newAlpha;
            sprite.color = currentColor;
            yield return null; // Wait for the next frame
        }

        // Ensure the final alpha value is set precisely
        currentColor.a = 0;
        sprite.color = currentColor;
        finishedCharFade = true;
    }

    private void Die()
    {
        if (!isDead)
        {
            isDead = true;
            UnityEngine.Debug.Log("Player died!");
            //TODO black out everything except the player
            GetComponent<PlayerController>().LockMovement();
            MapGenerator.GetComponent<MapGeneratorScript>().FadeOutInstant();

            //TODO fade the player to black
            StartCoroutine(FadeOut());


            
        }
    }
}