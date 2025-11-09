using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int Health = 10;

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
