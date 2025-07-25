// "Assets/Scripts/Spawning/DayNightCycleManager.cs"
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the day-night cycle and drives enemy spawning based on it.
/// SRS Requirement: 4. Day-Night & Spawn Integration
/// </summary>
public class DayNightCycleManager : MonoBehaviour
{
    [Header("Cycle Timings")]
    [Tooltip("Total duration of one full day-night cycle in seconds.")]
    public float cycleDurationSeconds = 600f; // 10 minutes

    [Header("Spawning Parameters")]
    [Tooltip("Base spawn rate (average spawns per second at normal intensity).")]
    public float baseSpawnRate = 0.5f;
    [Tooltip("Curve controlling spawn intensity over the day-night cycle (Time 0 to 1).")]
    public AnimationCurve spawnRateMultiplier;
    [Tooltip("The enemy prefab to spawn. In a full game, this would come from a biome or wave data object.")]
    public GameObject enemyPrefab; // For simplicity, using one prefab.

    // Private State
    private float cycleTimer;
    private int dayCount = 1;
    private float timeUntilNextSpawn;

    void Update()
    {
        // Advance the cycle timer
        cycleTimer += Time.deltaTime;
        if (cycleTimer >= cycleDurationSeconds)
        {
            cycleTimer -= cycleDurationSeconds;
            dayCount++;
        }

        // Handle Spawning
        timeUntilNextSpawn -= Time.deltaTime;
        if (timeUntilNextSpawn <= 0)
        {
            SpawnEnemy();
            // Calculate the time until the next spawn using Poisson distribution
            timeUntilNextSpawn = GetPoissonRandom(GetCurrentLambda());
        }
    }

    /// <summary>
    /// Calculates the current spawn rate parameter (lambda) for the Poisson process.
    /// λ(t) = baseRate × s(t) × (1 + dayCount/10)
    /// </summary>
    private float GetCurrentLambda()
    {
        float normalizedTime = cycleTimer / cycleDurationSeconds;
        float intensity = spawnRateMultiplier.Evaluate(normalizedTime);
        float dayModifier = 1f + (dayCount / 10f);
        return baseSpawnRate * intensity * dayModifier;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || ObjectPooler.Instance == null) return;

        // As per SRS, ensure chunk is generated before spawning.
        // This logic will be more complex and integrated with the player's position.
        // For now, we'll spawn at a fixed radius around the base core.
        Transform baseCore = GameManager.Instance.waveManager.endPoint; // Assuming this is the base
        if (baseCore == null) return;

        float spawnRingRadius = 20f; // Example radius
        float randomAngle = Random.Range(0, 2 * Mathf.PI);
        Vector3 spawnPos = baseCore.position + new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0) * spawnRingRadius;

        ObjectPooler.Instance.SpawnFromPool(enemyPrefab.name, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Returns a random value from a Poisson distribution.
    /// This determines the delay until the next event (spawn).
    /// </summary>
    /// <param name="lambda">The average rate of events.</param>
    private float GetPoissonRandom(float lambda)
    {
        // An approximation using the relationship between Poisson and Exponential distributions.
        // The time between events in a Poisson process follows an exponential distribution.
        if (lambda <= 0) return float.MaxValue; // Avoid division by zero and infinite loops
        return -Mathf.Log(1.0f - Random.value) / lambda;
    }
}