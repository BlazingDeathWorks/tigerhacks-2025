using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamager : MonoBehaviour
{
    [SerializeField] private int damageAmount = 3;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerStatsManager playerStats))
        {
            playerStats.TakeDamage(damageAmount);
        }
    }
}
