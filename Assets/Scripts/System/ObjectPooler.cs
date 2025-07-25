// Assets/Scripts/System/ObjectPooler.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic object pooling system for efficiently reusing GameObjects.
/// SRS Requirement: 5. Performance & Pooling
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private Transform poolContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if(poolDictionary == null)
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
        }

        poolContainer = new GameObject("ObjectPoolContainer").transform;

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolContainer);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// Spawns an object from the pool with a specific position, rotation, and parent.
    /// </summary>
    /// <param name="parent">The transform to parent the spawned object to. Can be null.</param>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist.");
            return null;
        }

        Queue<GameObject> poolQueue = poolDictionary[tag];

        if (poolQueue.Count == 0)
        {
            Pool p = pools.Find(pool => pool.tag == tag);
            if (p != null)
            {
                GameObject newObj = Instantiate(p.prefab, poolContainer);
                newObj.SetActive(false);
                poolQueue.Enqueue(newObj);
            }
            else return null;
        }

        GameObject objectToSpawn = poolQueue.Dequeue();

        objectToSpawn.transform.SetParent(parent); // Set the parent
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        pooledObj?.OnObjectSpawn();

        return objectToSpawn;
    }

    /// <summary>
    /// Returns an object to its pool.
    /// </summary>
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist. Destroying object.");
            Destroy(objectToReturn);
            return;
        }

        // Return to the central container to avoid being destroyed with a chunk
        objectToReturn.transform.SetParent(poolContainer);
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}

/// <summary>
/// Interface for objects that can be pooled.
/// </summary>
public interface IPooledObject
{
    void OnObjectSpawn();
}