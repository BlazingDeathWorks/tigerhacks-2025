using UnityEngine;

public class LaserCollider : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(3); // Assuming the laser deals 1 damage
        }
    }
}
