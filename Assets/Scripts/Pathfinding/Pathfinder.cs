// Scripts/Pathfinding/Pathfinder.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }

    private GridManager gridManager;

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

        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("Pathfinder could not find an instance of GridManager!", this);
        }
    }

    /// <summary>
    /// Finds a path from a start to a target position.
    /// </summary>
    /// <param name="ignoreWalkability">If true, the pathfinder will ignore the isWalkable flag on nodes. Used for level generation.</param>
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, bool ignoreWalkability = false)
    {
        if (gridManager == null) return null;

        PathNode startNode = gridManager.GetNodeFromWorldPoint(startPos);
        PathNode targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        // Target node must always be valid, but start node might be inside an obstacle before carving.
        if (startNode == null || targetNode == null)
        {
            return null;
        }

        gridManager.ResetAllNodeCosts();

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
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (PathNode neighbour in gridManager.GetNeighbours(currentNode))
            {
                // --- MODIFIED LINE ---
                // If we are NOT ignoring walkability, check if the node is unwalkable.
                // Also, always skip nodes in the closed set.
                if (!ignoreWalkability && !neighbour.isWalkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                // --- END MODIFICATION ---

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetManhattanDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }

        return null; // No path found
    }

    private List<Vector3> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path.Select(node => node.worldPosition).ToList();
    }

    private int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return (dstX > dstY) ? 14 * dstY + 10 * (dstX - dstY) : 14 * dstX + 10 * (dstY - dstX);
    }

    private int GetManhattanDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return 10 * (dstX + dstY);
    }
}