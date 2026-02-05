using UnityEngine;

public class DummyEnemy : MonoBehaviour, IDamageable
{
    public float health = 5f;

    public void TakeDamage(float amount)
    {
        health -= amount;

        if(health <= 0f)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
