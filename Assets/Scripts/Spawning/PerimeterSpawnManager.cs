// Scripts/Spawning/PerimeterSpawnManager.cs
using UnityEngine;

/// <summary>
/// Manages the spawning of enemies in a perimeter around a central point.
/// Listens to requests from the WaveManager.
/// </summary>
public class PerimeterSpawnManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The WaveManager that will request spawns.")]
    [SerializeField] private WaveManager waveManager;
    [Tooltip("The central point (e.g., the player's base) to spawn around.")]
    [SerializeField] private Transform baseCore;

    [Header("Spawning Parameters")]
    [Tooltip("The radius from the base core at which enemies will be spawned.")]
    [SerializeField] private float spawnRadius = 40f;

    /// <summary>
    /// Ensures all dependencies are assigned and subscribes to the spawn request event.
    /// </summary>
    void Awake()
    {
        // Auto-find dependencies if they are not assigned in the inspector.
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogError("PerimeterSpawnManager could not find a WaveManager in the scene.", this);
                enabled = false;
                return;
            }
        }

        if (baseCore == null)
        {
            GameObject baseCoreObject = GameObject.FindGameObjectWithTag("BaseCore");
            if (baseCoreObject != null)
            {
                baseCore = baseCoreObject.transform;
            }
            else
            {
                Debug.LogError("PerimeterSpawnManager requires a Transform with the 'BaseCore' tag.", this);
                enabled = false;
                return;
            }
        }

        // Subscribe the local spawn method to the WaveManager's event.
        waveManager.OnWaveSpawnRequest += SpawnEnemyFromWave;
    }

    /// <summary>
    /// Unsubscribes from the event when this object is destroyed to prevent memory leaks.
    /// </summary>
    void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveSpawnRequest -= SpawnEnemyFromWave;
        }
    }

    /// <summary>
    /// The core logic for spawning an enemy. This method is triggered by the WaveManager's event.
    /// </summary>
    /// <param name="data">The EnemyData for the enemy to be spawned.</param>
    private void SpawnEnemyFromWave(EnemyData data)
    {
        if (data == null || data.enemyPrefab == null)
        {
            Debug.LogWarning("SpawnEnemyFromWave called with null EnemyData or prefab.", this);
            return;
        }

        // 1. Get a random direction on the unit circle.
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // 2. Calculate the spawn position based on the base core's position, the random direction, and the spawn radius.
        Vector3 spawnPosition = baseCore.position + (Vector3)(randomDirection * spawnRadius);

        // 3. Request the EnemySpawner to spawn the enemy at the calculated position.
        // This assumes EnemySpawner is a singleton and has the required overload.
        EnemySpawner.Instance.SpawnEnemy(data.enemyPrefab, spawnPosition);
    }

    /// <summary>
    /// Draws a helpful gizmo in the editor to visualize the spawn radius.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (baseCore != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(baseCore.position, spawnRadius);
        }
    }
}