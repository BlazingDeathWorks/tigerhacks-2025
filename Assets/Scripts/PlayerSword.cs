using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSword : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The parent object that will be rotated to create the swing. The sword sprite should be a child of this object.")]
    public GameObject swordPivotObject; 
    [Tooltip("An empty GameObject, child of the sword sprite, marking the area to check for hits.")]
    public Transform attackPoint;
    [Tooltip("Drag the TrailRenderer component here from your AttackPoint GameObject.")]
    public TrailRenderer swordTrail;

    [Header("Attack Settings")]
    public float attackDamage = 25f;
    public float attackRange = 0.8f;
    public float attackRate = 2f; // Attacks per second
    public LayerMask enemyLayers;

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
        // Ensure the trail is not emitting when we switch to this weapon.
        if (swordTrail != null)
        {
            swordTrail.emitting = false;
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
            if (Input.GetMouseButtonDown(0) && !isAttacking)
            {
                nextAttackTime = Time.time + 1f / attackRate;
                StartCoroutine(SwingAttack());
            }
        }
    }

    IEnumerator SwingAttack()
    {
        isAttacking = true;
        List<Collider2D> hitThisSwing = new List<Collider2D>();

        if (swordTrail != null)
        {
            swordTrail.Clear(); // Clear any old trail segments.
            swordTrail.emitting = true; // Start emitting the trail.
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

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (!hitThisSwing.Contains(enemy))
                {
                    hitThisSwing.Add(enemy);
                    Debug.Log($"Sword hit {enemy.name}");
                    // TODO: Replace this with your actual damage logic.
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        swordPivotObject.transform.localRotation = endRotation;
        
        if (swordTrail != null)
        {
            swordTrail.emitting = false; // Stop emitting the trail.
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