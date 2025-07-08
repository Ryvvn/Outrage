// Assets/Scripts/Core/EnemyMovement.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;
    private List<Vector3> pathPoints;
    private int currentPathIndex = 0;

    /// <summary>
    /// Receives a path from the spawner.
    /// </summary>
    public void SetPath(List<Vector3> newPath)
    {
        pathPoints = newPath;
        if (pathPoints != null && pathPoints.Count > 0)
        {
            // Start at the first point, ready to move towards the second.
            transform.position = pathPoints[0];
            currentPathIndex = 1;
        }
    }

    void Update()
    {
        if (pathPoints == null || currentPathIndex >= pathPoints.Count) return;
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Vector3 targetPosition = pathPoints[currentPathIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentPathIndex++;
        }

        // Check if the enemy has reached the end of the path
        if (currentPathIndex >= pathPoints.Count)
        {
            GameManager.Instance.TakeDamage(1);
            FindObjectOfType<WaveManager>().EnemyDefeated(); // Notify WaveManager
            Destroy(gameObject);
        }
    }
}