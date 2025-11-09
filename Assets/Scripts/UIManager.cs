using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to import TextMeshPro

public class UIManager : MonoBehaviour
{
    // Singleton instance. This is a pattern that allows this script to be accessed from anywhere.
    public static UIManager Instance { get; private set; }

    [Header("Player UI Elements")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image weaponIconImage;

    void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Updates the health text on the UI.
    /// </summary>
    /// <param name="currentHealth">Player's current health.</param>
    /// <param name="maxHealth">Player's maximum health.</param>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    /// <summary>
    /// Updates the ammo text on the UI.
    /// Handles special cases for infinite ammo or no ammo.
    /// </summary>
    /// <param name="currentAmmo">Weapon's current ammo. Use -1 for infinite.</param>
    public void UpdateAmmo(int currentAmmo)
    {
        if (ammoText != null)
        {
            if (currentAmmo == -1) // Convention for infinite ammo
            {
                ammoText.text = "∞";
            }
            else if (currentAmmo == -2) // Convention for weapons with no ammo system (like a sword)
            {
                ammoText.text = ""; // Hide ammo text
            }
            else
            {
                ammoText.text = $"{currentAmmo}";
            }
        }
    }

    /// <summary>
    /// Updates the weapon icon on the UI.
    /// </summary>
    /// <param name="newIcon">The sprite for the current weapon.</param>
    public void UpdateWeaponIcon(Sprite newIcon)
    {
        if (weaponIconImage != null)
        {
            if (newIcon != null)
            {
                weaponIconImage.sprite = newIcon;
                weaponIconImage.enabled = true;
            }
            else
            {
                // Hide the icon if no sprite is provided
                weaponIconImage.enabled = false;
            }
        }
    }
}
