// Assets/Scripts/System/HealthSystem.cs
using UnityEngine;
using System;

/// <summary>
/// A reusable health component for any game object that can take damage.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Tooltip("The maximum health of this entity.")]
    [SerializeField] private int maxHealth = 100;

    // Public event that fires when the entity's health reaches zero.
    public event Action OnDied;

    private int currentHealth;
    private bool isDead = false;

    // Public property to access current health safely.
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    /// <summary>
    /// OnEnable is called when an object is activated (e.g., from an object pool).
    /// We use it to reset the entity's state.
    /// </summary>
    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    /// <summary>
    /// Reduces the entity's health by a specified amount.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take.</param>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    /// <summary>
    /// Instantly kills the entity. Useful for specific game mechanics.
    /// </summary>
    public void Kill()
    {
        TakeDamage(maxHealth);
    }

    /// <summary>
    /// Handles the death of the entity by invoking the OnDied event.
    /// </summary>
    private void Die()
    {
        isDead = true;

        // Broadcast that this entity has died. Other scripts can subscribe to this.
        OnDied?.Invoke();
    }
}