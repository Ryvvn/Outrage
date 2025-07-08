// Assets/Scripts/Core/WaveManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WaveManager : MonoBehaviour
{
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
    public Transform spawnPoint; // The starting transform
    public Transform endPoint;   // The ending transform

    [Header("Dependencies")]
    public Pathfinder pathfinder; // Reference to the Pathfinder

    public int currentWaveIndex { get; private set; } = -1;
    private int enemiesRemainingInWave;

    public void StartNextWave()
    {
        currentWaveIndex++;
        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        enemiesRemainingInWave = wave.enemyCount;

        // Get the path from the Pathfinder using the start and end points
        List<PathNode> path = pathfinder.FindPath(spawnPoint.position, endPoint.position);

        if (path == null || path.Count == 0)
        {
            Debug.LogError("Cannot start wave: No path found by Pathfinder!");
            yield break;
        }

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject enemyGO = Instantiate(wave.enemyData.enemyPrefab, path[0].worldPosition, Quaternion.identity);

            EnemyMovement enemyMovement = enemyGO.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                List<Vector3> pathPositions = new List<Vector3>();
                foreach (PathNode node in path)
                {
                    pathPositions.Add(node.worldPosition);
                }
                enemyMovement.SetPath(pathPositions);
            }
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    public void EnemyDefeated()
    {
        enemiesRemainingInWave--;
        if (enemiesRemainingInWave <= 0 && GameManager.Instance.currentState == GameState.WaveInProgress)
        {
            OnWaveCompleted?.Invoke(currentWaveIndex + 1);
        }
    }

    public bool IsLastWave()
    {
        return currentWaveIndex >= waves.Count - 1;
    }
}