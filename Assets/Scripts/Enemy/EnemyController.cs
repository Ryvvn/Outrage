// Assets/Scripts/Enemy/EnemyController.cs
using UnityEngine;
using System;

/// <summary>
/// Controls the behavior of an enemy, linking its health, movement, and game-wide events.
/// </summary>
[RequireComponent(typeof(HealthSystem), typeof(EnemyMovement))]
public class EnemyController : MonoBehaviour
{
    // Static event to notify the WaveManager when any enemy is defeated (by death or reaching the goal).
    public static event Action<GameObject> OnEnemyDefeated;

    private HealthSystem healthSystem;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    void OnEnable()
    {
        // Subscribe the HandleDeath method to the OnDied event of the HealthSystem.
        healthSystem.OnDied += HandleDeath;
    }

    void OnDisable()
    {
        // Always unsubscribe from events to prevent memory leaks and errors.
        healthSystem.OnDied -= HandleDeath;
    }

    /// <summary>
    /// Called when the enemy's health reaches zero.
    /// </summary>
    private void HandleDeath()
    {
        // Notify the wave manager and return to the pool.
        Defeated();
    }

    /// <summary>
    /// Called by EnemyMovement when it reaches the destination.
    /// </summary>
    public void HandleReachedGoal()
    {
        // Notify the wave manager and return to the pool.
        Defeated();
    }

    /// <summary>
    /// Centralized method for when an enemy is defeated, either by death or reaching the goal.
    /// </summary>
    private void Defeated()
    {

        // Broadcast that this enemy is defeated.
        OnEnemyDefeated?.Invoke(gameObject);

        // Return this GameObject to the object pool via the spawner.
        EnemySpawner.Instance.ReturnEnemyToPool(gameObject);
    }
}