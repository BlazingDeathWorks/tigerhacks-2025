using UnityEngine;

// This is a base class that all weapon scripts will inherit from.
// It ensures that every weapon has a place to store its UI icon.
public abstract class WeaponBase : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The icon to display on the UI for this weapon.")]
    public Sprite weaponIcon;
}
