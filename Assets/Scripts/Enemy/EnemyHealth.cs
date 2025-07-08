// Assets/Scripts/Core/EnemyHealth.cs
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int moneyOnDeath = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= (int)amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.Instance.AddMoney(moneyOnDeath);
        // Let WaveManager know this enemy is defeated
        FindObjectOfType<WaveManager>().EnemyDefeated();
        Destroy(gameObject);
    }
}