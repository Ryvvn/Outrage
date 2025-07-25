// Scripts/Spawning/WaveManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages the sequence of enemy waves, including timing and enemy composition.
/// It no longer handles spawning directly but fires an event to request spawns.
/// </summary>
public class WaveManager : MonoBehaviour
{
    /// <summary>
    /// Event fired when the game logic determines an enemy should be spawned.
    /// The PerimeterSpawnManager subscribes to this.
    /// </summary>
    public event Action<EnemyData> OnWaveSpawnRequest;

    /// <summary>
    /// Event fired when a wave is fully completed (all enemies defeated).
    /// The GameManager subscribes to this.
    /// </summary>
    public event Action<int> OnWaveCompleted;

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public EnemyData enemyData;
        public int enemyCount;
        public float spawnInterval;
    }

    [Header("Wave Configuration")]
    public List<Wave> waves;

    [Header("References")]
    [Tooltip("The final destination for all enemies.")]
    public Transform endPoint; // Target for enemies (Base Core)

    public int currentWaveIndex { get; private set; } = -1;
    private int enemiesRemainingInWave;

    /// <summary>
    /// Starts the next wave in the sequence if one is available.
    /// </summary>
    public void StartNextWave()
    {
        if (GameManager.Instance.currentState != GameState.Build) return;

        if (currentWaveIndex + 1 >= waves.Count)
        {
            Debug.Log("All waves completed.");
            GameManager.Instance.ChangeState(GameState.Victory);
            return;
        }

        GameManager.Instance.StartWave();
        currentWaveIndex++;
        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    /// <summary>
    /// Coroutine that handles the spawning logic for a single wave over time.
    /// </summary>
    private IEnumerator SpawnWave(Wave wave)
    {
        enemiesRemainingInWave = wave.enemyCount;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            // Fire the event to request a spawn. Another manager will handle the actual instantiation.
            OnWaveSpawnRequest?.Invoke(wave.enemyData);

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    /// <summary>
    /// Called by an enemy when it is defeated. Checks for wave completion.
    /// </summary>
    public void EnemyDefeated()
    {
        enemiesRemainingInWave--;
        if (enemiesRemainingInWave <= 0 && GameManager.Instance.currentState == GameState.WaveInProgress)
        {
            OnWaveCompleted?.Invoke(currentWaveIndex + 1);
        }
    }

    /// <summary>
    /// Checks if the current wave is the last one in the list.
    /// </summary>
    /// <returns>True if this is the final wave.</returns>
    public bool IsLastWave()
    {
        return currentWaveIndex >= waves.Count - 1;
    }
}