// Scripts/System/HealthSystem.cs
using UnityEngine;
using System;

/// <summary>
/// A reusable health component for any game object that can take damage.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Tooltip("The maximum health of this entity.")]
    [SerializeField] private int maxHealth = 100;

    [Tooltip("Minimum time between taking damage instances. Prevents rapid multi-hits.")]
    [SerializeField] private float damageCooldown = 0.1f;

    [Header("Effects")]
    [Tooltip("Particle effect to instantiate when damage is taken.")]
    [SerializeField] private GameObject bloodEffectPrefab;

    // Public event that fires when health changes, passing current and max health.
    public event Action<float, float> OnHealthChanged;
    // Public event that fires when the entity's health reaches zero.
    public event Action OnDied;

    private int currentHealth;
    private bool isDead = false;
    private float lastDamageTime;

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
        lastDamageTime = -damageCooldown; // Allow immediate damage on enable.
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Update health bar on spawn
    }

    /// <summary>
    /// Reduces the entity's health by a specified amount, respecting the cooldown.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take.</param>
    public void TakeDamage(int damageAmount)
    {
        // Ignore damage if dead or if the cooldown has not elapsed.
        if (isDead || Time.time < lastDamageTime + damageCooldown) return;

        lastDamageTime = Time.time;
        currentHealth -= damageAmount;

        // Trigger particle effect
        if (bloodEffectPrefab != null)
        {
            Instantiate(bloodEffectPrefab, transform.position, Quaternion.identity);
        }

        // Notify listeners that health has changed.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

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
        OnDied?.Invoke();
    }
}