// Scripts/Pathfinding/Pathfinder.cs
using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }
    public List<PathNode> FinalPath { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<PathNode> FindPath(Vector3 startPos, Vector3 targetPos, Dictionary<PathNode, int> costPenalties = null, float randomFactor = 0f, bool useDijkstra = false)
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("GridManager instance not found!");
            return null;
        }

        PathNode startNode = GridManager.Instance.GetNodeFromWorldPoint(startPos);
        PathNode targetNode = GridManager.Instance.GetNodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.isWalkable || !targetNode.isWalkable)
        {
            Debug.LogWarning($"Pathfinding: Invalid start or target node. Start walkable: {startNode?.isWalkable}, Target walkable: {targetNode?.isWalkable}.");
            return null;
        }

        GridManager.Instance.ResetAllNodeCosts();

        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        openSet.Add(startNode);

        startNode.gCost = 0;
        startNode.hCost = GetManhattanDistance(startNode, targetNode);

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                FinalPath = RetracePath(startNode, targetNode);
                return FinalPath;
            }

            foreach (PathNode neighbour in GridManager.Instance.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetManhattanDistance(currentNode, neighbour);
                if (costPenalties != null && costPenalties.ContainsKey(neighbour))
                {
                    newMovementCostToNeighbour += costPenalties[neighbour];
                }

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;

                    int hCost = GetManhattanDistance(neighbour, targetNode);
                    if (useDijkstra)
                    {
                        hCost = 0; // A* with hCost=0 is Dijkstra's algorithm
                    }
                    else if (randomFactor > 0)
                    {
                        hCost = (int)(hCost * (1 + Random.Range(-randomFactor, randomFactor)));
                    }
                    neighbour.hCost = hCost;

                    neighbour.parentNode = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return null; // Path not found
    }

    List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }

    public int GetManhattanDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        // Standard Manhattan distance for a 4-directional grid.
        // Cost for straight move is 10.
        return 10 * (dstX + dstY);
    }
}