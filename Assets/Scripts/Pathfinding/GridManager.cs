// Scripts/Pathfinding/GridManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates and manages the pathfinding grid for the entire finite world.
/// The grid's walkability is now updated by the world generator.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("World Setup")]
    [Tooltip("The total size of the world in tiles (e.g., 200x200).")]
    [SerializeField] private Vector2Int worldSize = new Vector2Int(200, 200);
    [Tooltip("The physical size of each cell in the grid.")]
    [SerializeField] private Vector3 cellSize = Vector3.one;
    [Tooltip("The world-space position corresponding to the grid's bottom-left corner.")]
    [SerializeField] private Vector3 worldOriginPosition = new Vector3(-100, -100, 0);

    private PathNode[,] nodes;
    public int GridSizeX { get; private set; }
    public int GridSizeY { get; private set; }

    // Public accessor for cell size for other systems
    public Vector3 CellSize => cellSize;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // Create the grid structure on Awake to ensure it's ready for other services.
        CreateGridStructure();
    }

    /// <summary>
    /// Creates the grid data structure with all nodes initially set to unwalkable.
    /// The actual walkability will be set by the world generator.
    /// </summary>
    public void CreateGridStructure()
    {
        GridSizeX = worldSize.x;
        GridSizeY = worldSize.y;
        nodes = new PathNode[GridSizeX, GridSizeY];

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                Vector3 worldPoint = worldOriginPosition + new Vector3(x * cellSize.x + cellSize.x / 2, y * cellSize.y + cellSize.y / 2, 0);
                // All nodes start as unwalkable until a generator explicitly makes them walkable.
                nodes[x, y] = new PathNode(false, worldPoint, x, y);
            }
        }
        Debug.Log($"Grid structure created with size: {GridSizeX}x{GridSizeY}. Awaiting generation data.");
    }

    /// <summary>
    /// Updates the walkability of a specific node. Called by world generators.
    /// </summary>
    public void UpdateNodeWalkability(Vector3 worldPosition, bool isWalkable)
    {
        PathNode node = GetNodeFromWorldPoint(worldPosition);
        if (node != null)
        {
            node.isWalkable = isWalkable;
        }
    }

    public PathNode GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        if (nodes == null) return null;

        float percentX = (worldPosition.x - worldOriginPosition.x) / (GridSizeX * cellSize.x);
        float percentY = (worldPosition.y - worldOriginPosition.y) / (GridSizeY * cellSize.y);

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.FloorToInt((GridSizeX) * percentX);
        int y = Mathf.FloorToInt((GridSizeY) * percentY);

        // Boundary check
        x = Mathf.Clamp(x, 0, GridSizeX - 1);
        y = Mathf.Clamp(y, 0, GridSizeY - 1);

        return nodes[x, y];
    }

    public PathNode GetNode(int x, int y)
    {
        if (x >= 0 && x < GridSizeX && y >= 0 && y < GridSizeY)
        {
            return nodes[x, y];
        }
        return null;
    }

    public List<PathNode> GetNeighbours(PathNode node)
    {
        List<PathNode> neighbours = new List<PathNode>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                PathNode neighbourNode = GetNode(checkX, checkY);
                if (neighbourNode != null)
                {
                    neighbours.Add(neighbourNode);
                }
            }
        }
        return neighbours;
    }

    public void ResetAllNodeCosts()
    {
        if (nodes == null) return;
        foreach (PathNode node in nodes)
        {
            node.ResetCosts();
        }
    }
}