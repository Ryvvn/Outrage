// Scripts/Spawning/EnemySpawner.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages object pools for enemies to improve performance by reusing GameObjects.
/// Acts as a Singleton for easy global access.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Tooltip("Parent transform for pooled objects to keep the hierarchy clean.")]
    public Transform poolParent;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
    }

    /// <summary>
    /// Creates a new object pool for a given prefab.
    /// </summary>
    /// <param name="prefab">The enemy prefab to pool.</param>
    /// <param name="initialSize">The number of enemies to pre-instantiate.</param>
    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null) return;
        string poolKey = prefab.name;

        if (poolDictionary.ContainsKey(poolKey)) return;

        poolDictionary[poolKey] = new Queue<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent);
            obj.name = poolKey; // Ensure consistent naming for re-pooling
            obj.SetActive(false);
            poolDictionary[poolKey].Enqueue(obj);
        }
    }

    /// <summary>
    /// Spawns an enemy from the pool at a specific position.
    /// This is the primary method used by the new perimeter spawning system.
    /// </summary>
    /// <param name="enemyPrefab">The prefab of the enemy to spawn.</param>
    /// <param name="position">The world position to spawn the enemy at.</param>
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnEnemy was called with a null prefab.");
            return;
        }

        string poolKey = enemyPrefab.name;

        // Ensure a pool for this prefab exists. If not, create a small one.
        if (!poolDictionary.ContainsKey(poolKey))
        {
            Debug.LogWarning($"Pool for {poolKey} not found. Creating a new one on-the-fly.");
            CreatePool(enemyPrefab, 5);
        }

        Queue<GameObject> queue = poolDictionary[poolKey];
        GameObject enemyToSpawn;

        // If the pool is empty, instantiate a new object and add it to the pool.
        if (queue.Count == 0)
        {
            enemyToSpawn = Instantiate(enemyPrefab, poolParent);
            enemyToSpawn.name = poolKey;
        }
        else // Otherwise, reuse an existing object.
        {
            enemyToSpawn = queue.Dequeue();
        }

        // ** CRITICAL STEP **: Set position and rotation *before* activating the object.
        // This prevents OnEnable() from running at the wrong spot and avoids extra physics calculations.
        enemyToSpawn.transform.position = position;
        enemyToSpawn.transform.rotation = Quaternion.identity;
        enemyToSpawn.SetActive(true);
    }

    /// <summary>
    /// Returns an enemy to its corresponding pool.
    /// </summary>
    /// <param name="enemy">The GameObject of the enemy to return.</param>
    public void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        string poolKey = enemy.name;

        // Deactivate the object and add it back to the queue.
        enemy.SetActive(false);

        if (poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary[poolKey].Enqueue(enemy);
        }
        else
        {
            // If the pool somehow doesn't exist, just destroy the object to prevent errors.
            Debug.LogWarning($"Pool with key '{poolKey}' does not exist. Destroying object instead.");
            Destroy(enemy);
        }
    }
}