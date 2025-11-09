using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int Health = 10;
    void OnStart()
    {
        Health = (int) (Health * ProgressScript.Instance.HealthMultiplier);
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
