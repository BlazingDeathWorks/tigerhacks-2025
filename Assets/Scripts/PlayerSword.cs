using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSword : WeaponBase
{
    [Header("References")]
    [Tooltip("The parent object that will be rotated to create the swing. The sword sprite should be a child of this object.")]
    public GameObject swordPivotObject; 
    [Tooltip("An empty GameObject, child of the sword sprite, marking the area to check for hits.")]
    public Transform attackPoint;
    [Tooltip("Drag the TrailRenderer component here from your AttackPoint GameObject.")]
    public TrailRenderer swordTrail;

    [Header("Attack Settings")]
    public int maxAmmo = 30;
    public int currentAmmo;
    public float attackDamage = 25f;
    public float attackRange = 0.8f;
    public float attackRate = 2f; // Attacks per second
    [Tooltip("Set this to include layers for both Enemies and Enemy Projectiles.")]
    public LayerMask hitMask;

    [Header("Animation Settings")]
    [Tooltip("How long the swing animation takes. Set to a very small value for an almost instant swing.")]
    public float swingDuration = 0.05f;
    [Tooltip("The total angle of the sword's arc.")]
    public float swingAngle = 120f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private bool swingToLeft = true; // Determines the direction of the next swing

    void Awake()
    {
        currentAmmo = maxAmmo;
        if (swordPivotObject != null)
        {
            swordPivotObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (swordPivotObject != null)
        {
            swordPivotObject.SetActive(true);
            Quaternion restingRotation = Quaternion.Euler(0, 0, (swingToLeft ? 1 : -1) * (swingAngle / 2));
            swordPivotObject.transform.localRotation = restingRotation;
        }
        if (swordTrail != null)
        {
            swordTrail.emitting = false;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(currentAmmo);
        }
    }

    void OnDisable()
    {
        if (swordPivotObject != null)
        {
            swordPivotObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking && currentAmmo > 0)
            {
                nextAttackTime = Time.time + 1f / attackRate;
                StartCoroutine(SwingAttack());
            }
        }
    }

    IEnumerator SwingAttack()
    {
        isAttacking = true;
        currentAmmo--;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(currentAmmo);
        }

        List<Collider2D> hitThisSwing = new List<Collider2D>();

        if (swordTrail != null)
        {
            swordTrail.Clear();
            swordTrail.emitting = true;
        }

        float startAngleVal = (swingToLeft ? 1 : -1) * (swingAngle / 2);
        float endAngleVal = -startAngleVal;
        Quaternion startRotation = Quaternion.Euler(0, 0, startAngleVal);
        Quaternion endRotation = Quaternion.Euler(0, 0, endAngleVal);

        float elapsedTime = 0f;
        while (elapsedTime < swingDuration)
        {
            float progress = elapsedTime / swingDuration;
            swordPivotObject.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, progress);

            // --- Continuous Hit Detection ---
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, hitMask);
            foreach (Collider2D collider in hitColliders)
            {
                if (!hitThisSwing.Contains(collider))
                {
                    // Check if it's an enemy projectile and destroy it
                    if (collider.CompareTag("EnemyProjectile"))
                    {
                        hitThisSwing.Add(collider); // Add to list so we don't process it again
                        Destroy(collider.gameObject);
                        Debug.Log("Sword destroyed an enemy projectile.");
                    }
                    // Check if it's an enemy and damage it
                    else if (collider.CompareTag("Enemy"))
                    {
                        hitThisSwing.Add(collider);
                        Debug.Log($"Sword hit {collider.name}");
                        // TODO: Replace this with your actual damage logic.
                        // For example: enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
                    }
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        swordPivotObject.transform.localRotation = endRotation;
        
        if (swordTrail != null)
        {
            swordTrail.emitting = false;
        }
        
        swingToLeft = !swingToLeft;
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
