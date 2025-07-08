
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages object pools for enemies to improve performance by reusing GameObjects.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // Singleton instance for easy access.
    public static EnemySpawner Instance { get; private set; }

    [Tooltip("Parent transform for pooled objects to keep the hierarchy clean.")]
    public Transform poolParent;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        // Setup Singleton pattern.
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
    /// Spawns an enemy from the pool or creates a new one if the pool is empty.
    /// It then assigns the necessary path data.
    /// </summary>
    public void SpawnEnemy(GameObject enemyPrefab, List<Vector3> path)
    {
        string poolKey = enemyPrefab.name;

        // Ensure a pool for this prefab exists.
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary[poolKey] = new Queue<GameObject>();
        }

        GameObject enemyToSpawn;

        // If the pool has an inactive object, reuse it.
        if (poolDictionary[poolKey].Count > 0)
        {
            enemyToSpawn = poolDictionary[poolKey].Dequeue();
        }
        else // Otherwise, create a new one.
        {
            enemyToSpawn = Instantiate(enemyPrefab, poolParent);
            enemyToSpawn.name = poolKey; // To keep names consistent.
        }

        // Setup the enemy's components.
        enemyToSpawn.transform.position = path[0];

        EnemyMovement movement = enemyToSpawn.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.SetPath(path);
        }

        // Activate the enemy and make it visible.
        enemyToSpawn.SetActive(true);
    }

    /// <summary>
    /// Returns an enemy to its corresponding pool.
    /// </summary>
    public void ReturnEnemyToPool(GameObject enemy)
    {
        string poolKey = enemy.name;

        // Deactivate the object and add it back to the queue.
        enemy.SetActive(false);

        // Ensure the pool exists before trying to enqueue.
        if (poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary[poolKey].Enqueue(enemy);
        }
        else
        {
            Debug.LogWarning($"Pool with key '{poolKey}' does not exist. Destroying object instead.");
            Destroy(enemy);
        }
    }
}