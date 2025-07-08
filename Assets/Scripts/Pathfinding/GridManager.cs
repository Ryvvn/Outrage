// Scripts/Pathfinding/GridManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Tilemap baseTilemap;
    public Tilemap obstacleTilemap;
    private Grid unityGrid;
    private PathNode[,] nodes;
    private BoundsInt tilemapBounds;

    public int GridSizeX { get; private set; }
    public int GridSizeY { get; private set; }

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (baseTilemap == null || obstacleTilemap == null)
        {
            Debug.LogError("GridManager is missing one or more tilemap references!");
            return;
        }
        unityGrid = baseTilemap.layoutGrid;
        CreateGrid();
    }

    /// <summary>
    /// Expands the pathfinding grid to encompass new map areas.
    /// </summary>
    public void ExpandGrid()
    {
        Debug.Log("Expanding grid...");
        PathNode[,] oldNodes = nodes;
        BoundsInt oldBounds = tilemapBounds;

        // Recalculate the totals bounds of the map 
        baseTilemap.CompressBounds();
        tilemapBounds = baseTilemap.cellBounds;

        GridSizeX = tilemapBounds.size.x;
        GridSizeY = tilemapBounds.size.y;
        nodes = new PathNode[GridSizeX, GridSizeY];

        // Calculate the offset of the old grid within the new, larger grid
        Vector3Int offset = oldBounds.min - tilemapBounds.min;

        // Iterate through the new and larger grid space
        for(int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                // Check if this position corresponds to a node from the old grid
                int oldX = x - offset.x;
                int oldY = y - offset.y;

                if(oldX >= 0 && oldX < oldBounds.size.x && oldY >= 0 && oldY < oldBounds.size.y)
                {
                    // This was part of the old grid, copy the node over
                    nodes[x, y] = oldNodes[oldX, oldY];
                    nodes[x, y].gridX = x; // Update grid coordinates
                    nodes[x, y].gridY = y;
                }
                else
                {
                    // This is a new node in the expansion area, create it
                    Vector3Int tilemapCellPos = new Vector3Int(tilemapBounds.xMin + x, tilemapBounds.yMin + y, tilemapBounds.position.z);
                    nodes[x, y] = CreateNodeAt(tilemapCellPos, x, y);
                }
            }
        }
        Debug.Log($"Grid expanded to size {GridSizeX}x{GridSizeY}. New MinBounds: ({tilemapBounds.xMin}, {tilemapBounds.yMin})");
    }

    private PathNode CreateNodeAt(Vector3Int cellPos, int gridX, int gridY)
    {
        Vector3 worldPoint = unityGrid.GetCellCenterWorld(cellPos);
        // A node is walkable if it's on the base tilemap AND NOT on the obstacle tilemap
        bool isBaseTile = baseTilemap.HasTile(cellPos);
        bool isObstacleTile = obstacleTilemap.HasTile(cellPos);
        return new PathNode(isBaseTile && !isObstacleTile, worldPoint, gridX, gridY);
    }



    public void CreateGrid()
    {
        baseTilemap.CompressBounds();
        tilemapBounds = baseTilemap.cellBounds;

        GridSizeX = tilemapBounds.size.x;
        GridSizeY = tilemapBounds.size.y;
        nodes = new PathNode[GridSizeX, GridSizeY];

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                Vector3Int tilemapCellPos = new Vector3Int(tilemapBounds.xMin + x, tilemapBounds.yMin + y, tilemapBounds.position.z);
                Vector3 worldPoint = unityGrid.GetCellCenterWorld(tilemapCellPos);
                bool walkable = baseTilemap.HasTile(tilemapCellPos);

                nodes[x, y] = new PathNode(walkable, worldPoint, x, y);
            }
        }
        Debug.Log($"Grid created with size: {GridSizeX}x{GridSizeY}. MinBounds: ({tilemapBounds.xMin}, {tilemapBounds.yMin})");
    }

    public List<PathNode> GetAllNodes()
    {
        List<PathNode> allNodes = new List<PathNode>();
        if (nodes == null) return allNodes;

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                allNodes.Add(nodes[x, y]);
            }
        }
        return allNodes;
    }

    public void UpdateNodeWalkability(PathNode node, bool isWalkable)
    {
        if (node != null)
        {
            node.isWalkable = isWalkable;
        }
    }

    public PathNode GetNode(int x, int y)
    {
        if (x >= 0 && x < GridSizeX && y >= 0 && y < GridSizeY)
        {
            return nodes[x, y];
        }
        return null;
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return unityGrid.WorldToCell(worldPosition);
    }

    public PathNode GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        if (nodes == null || unityGrid == null) return null;
        Vector3Int cellPosition = unityGrid.WorldToCell(worldPosition);
        int x = cellPosition.x - tilemapBounds.xMin;
        int y = cellPosition.y - tilemapBounds.yMin;

        return GetNode(x, y);
    }

    public List<PathNode> GetNeighbours(PathNode node)
    {
        List<PathNode> neighbours = new List<PathNode>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                // Enforce 4-directional movement (no diagonals) for clearer turns.
                if (Mathf.Abs(x) == Mathf.Abs(y)) continue;

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

    void OnDrawGizmos()
    {
        if (nodes != null)
        {
            float nodeDiameter = unityGrid != null ? unityGrid.cellSize.x * 0.1f : 0.5f;
            foreach (PathNode n in nodes)
            {
                Gizmos.color = (n.isWalkable) ? Color.white : Color.red;
                if (Pathfinder.Instance != null && Pathfinder.Instance.FinalPath != null && Pathfinder.Instance.FinalPath.Contains(n))
                {
                    Gizmos.color = Color.black;
                }
                Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter);
            }
        }
    }

    public void ResetAllNodeCosts()
    {
        if (nodes == null) return;
        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                nodes[x, y].ResetCosts();
            }
        }
    }
}