using UnityEngine;

public class PlayerWeaponsManager : MonoBehaviour
{
    [Tooltip("Add all weapon components (scripts) you want to cycle through here. The first element is the starting weapon.")]
    [SerializeField] private WeaponBase[] weapons; 
    
    private int currentWeaponIndex = 0;

    void Start()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("PlayerWeaponsManager: No weapons assigned in the inspector!");
            return;
        }

        // Disable all weapons except the first one.
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].enabled = (i == 0);
            }
        }
        currentWeaponIndex = 0;

        // Set the initial weapon icon
        if (weapons[currentWeaponIndex] != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWeaponIcon(weapons[currentWeaponIndex].weaponIcon);
        }
    }

    void Update()
    {
        if (weapons.Length <= 1) return; // Can't switch if there's only one weapon.

        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleWeapon(-1);
        }
    }

    private void CycleWeapon(int direction)
    {
        // Disable the current weapon before switching
        if (weapons[currentWeaponIndex] != null)
        {
            weapons[currentWeaponIndex].enabled = false;
        }

        currentWeaponIndex += direction;

        if (currentWeaponIndex >= weapons.Length)
        {
            currentWeaponIndex = 0;
        }
        else if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = weapons.Length - 1;
        }

        // Enable the new current weapon
        if (weapons[currentWeaponIndex] != null)
        {
            weapons[currentWeaponIndex].enabled = true;

            // Update the UI with the new weapon's icon
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateWeaponIcon(weapons[currentWeaponIndex].weaponIcon);
            }
        }
    }
}
