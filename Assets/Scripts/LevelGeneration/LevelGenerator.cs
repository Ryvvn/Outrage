//// Scripts/LevelGeneration/LevelGenerator.cs
//using UnityEngine;
//using UnityEngine.Tilemaps;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;

//public class LevelGenerator : MonoBehaviour
//{

//    [Header("Input Requirements")]
//    public Tilemap baseTilemap;
//    public Tilemap obstacleTilemap;
//    public Tile obstacleTile;
//    public Transform tower;
//    public List<Transform> spawnPoints;

//    [Header("Generation Parameters")]
//    [Range(0.05f, 0.4f)]
//    public float initialObstacleDensity = 0.25f;
//    public int pathsPerSpawn = 3;
//    public int minPathLength = 12;
//    [Range(0f, 1f)]
//    public float maxPathRedundancy = 0.3f;
//    public int clearanceRadius = 2;

//    [Header("Debug")]
//    public bool drawDebugPaths = true;

//    private GridManager gridManager;
//    private Pathfinder pathfinder;

//    public LevelPathData FinalLevelPaths { get; private set; }
//    private List<List<PathNode>> debugAllPaths;

//    public static LevelGenerator Instance { get; private set; }

//    void Awake()
//    {
//        if (Instance != null && Instance != this) Destroy(gameObject);
//        else Instance = this;
//    }


//    void Start()
//    {
//        gridManager = GridManager.Instance;
//        pathfinder = Pathfinder.Instance;
//        FinalLevelPaths = new LevelPathData();
//        debugAllPaths = new List<List<PathNode>>();

//        if (gridManager == null || pathfinder == null || tower == null || spawnPoints.Count == 0)
//        {
//            Debug.LogError("LevelGenerator is missing critical references. Aborting.");
//            return;
//        }

//        StartCoroutine(GenerateLevelCoroutine());
//    }


//    /// <summary>
//    /// Adds a procedurally generated chunk's data to the tilemaps.
//    /// </summary>
//    //public void AddChunk(ProceduralExpansionChunk chunk, Vector3Int origin)
//    //{
//    //    if (chunk == null)
//    //    {
//    //        Debug.LogError("Cannot add a null procedural chunk.");
//    //        return;
//    //    }

//    //    BoundsInt chunkBounds = new BoundsInt(origin.x, origin.y, origin.z, chunk.chunkSize.x, chunk.chunkSize.y, 1);

//    //    // Set the background and obstacle tiles from the generated data.
//    //    if (chunk.generatedBackgroundTileData != null)
//    //    {
//    //        baseTilemap.SetTilesBlock(chunkBounds, chunk.generatedBackgroundTileData);
//    //    }

//    //    if (chunk.generatedObstacleTileData != null)
//    //    {
//    //        obstacleTilemap.SetTilesBlock(chunkBounds, chunk.generatedObstacleTileData);
//    //    }

//    //    // Expand the pathfinding grid to include the new tiles.
//    //    GridManager.Instance.ExpandGrid();
//    //    Debug.Log($"Procedural chunk '{chunk.chunkName}' added at {origin}. Grid expanded. Path recalculation needed.");
//    //}

//    /// <summary>
//    /// Draws the final calculated paths onto the path tilemap for visualization.
//    /// </summary>
//    //public void DrawPaths()
//    //{
//    //    if (pathTilemap == null || pathTile == null || FinalLevelPaths == null)
//    //    {
//    //        Debug.LogWarning("Path Tilemap or Path Tile not set. Cannot draw paths.");
//    //        return;
//    //    }

//    //    pathTilemap.ClearAllTiles();

//    //    foreach (var spawnData in FinalLevelPaths.spawnPaths)
//    //    {
//    //        foreach (var path in spawnData.paths)
//    //        {
//    //            foreach (var point in path)
//    //            {
//    //                Vector3Int cellPosition = gridManager.WorldToCell(point.ToVector3());
//    //                if (obstacleTilemap.GetTile(cellPosition) == null)
//    //                {
//    //                    pathTilemap.SetTile(cellPosition, pathTile);
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    private IEnumerator GenerateLevelCoroutine()
//    {
//        Debug.Log("Starting level generation...");

//        float currentDensity = initialObstacleDensity;
//        int maxRetries = 3;

//        for (int i = 0; i < maxRetries; i++)
//        {
//            ResetGridState();
//            List<PathNode> candidateNodes = GetObstacleCandidateNodes();
//            int obstacleCount = PlaceObstacles(candidateNodes, currentDensity);
//            Debug.Log($"Attempt {i + 1}: Placing {obstacleCount} obstacles with density {currentDensity:P2}.");

//            if ((float)obstacleCount / candidateNodes.Count > 0.7f)
//            {
//                Debug.LogWarning("Obstacle coverage is over 70%. This may impact performance or pathability.");
//                currentDensity *= 0.85f;
//                continue;
//            }

//            yield break;

           
//        }

//        Debug.LogError("Failed to generate a valid level after all retries. The level may be unplayable.");
//    }

//    void ResetGridState()
//    {
//        //if (pathTilemap != null) pathTilemap.ClearAllTiles();

//        obstacleTilemap.ClearAllTiles();
//        GridManager.Instance.CreateGrid();
//    }

//    List<PathNode> GetObstacleCandidateNodes()
//    {
//        List<PathNode> candidates = new List<PathNode>();
//        List<PathNode> allNodes = gridManager.GetAllNodes();

//        HashSet<PathNode> protectedNodes = new HashSet<PathNode>();
//        List<Vector3> criticalPositions = spawnPoints.Select(s => s.position).ToList();
//        criticalPositions.Add(tower.position);

//        foreach (var pos in criticalPositions)
//        {
//            PathNode centerNode = gridManager.GetNodeFromWorldPoint(pos);
//            if (centerNode == null) continue;
//            for (int x = -clearanceRadius; x <= clearanceRadius; x++)
//            {
//                for (int y = -clearanceRadius; y <= clearanceRadius; y++)
//                {
//                    PathNode node = gridManager.GetNode(centerNode.gridX + x, centerNode.gridY + y);
//                    if (node != null) protectedNodes.Add(node);
//                }
//            }
//        }

//        foreach (var node in allNodes)
//        {
//            if (node.isWalkable && !protectedNodes.Contains(node))
//            {
//                candidates.Add(node);
//            }
//        }
//        return candidates;
//    }

//    int PlaceObstacles(List<PathNode> candidates, float density)
//    {
//        int obstaclesToPlace = Mathf.FloorToInt(candidates.Count * density);
//        var shuffledCandidates = candidates.OrderBy(a => Random.value).ToList();

//        for (int i = 0; i < obstaclesToPlace; i++)
//        {
//            PathNode node = shuffledCandidates[i];
//            gridManager.UpdateNodeWalkability(node, false);
//            obstacleTilemap.SetTile(gridManager.WorldToCell(node.worldPosition), obstacleTile);
//        }
//        return obstaclesToPlace;
//    }

//    //public bool RecalculateAllRoutes()
//    //{
//    //    FinalLevelPaths.spawnPaths.Clear();
//    //    if (drawDebugPaths) debugAllPaths.Clear();

//    //    var penalties = new Dictionary<PathNode, int>();

//    //    foreach (var spawn in spawnPoints)
//    //    {
//    //        var spawnData = new SpawnPathData { spawnPoint = new SerializableVector3(spawn.position) };
//    //        var generatedPathsForSpawn = new List<List<PathNode>>();

//    //        for (int i = 0; i < pathsPerSpawn; i++)
//    //        {
//    //            var newPath = pathfinder.FindPath(spawn.position, tower.position, penalties, 0.2f);

//    //            if (newPath == null || newPath.Count < minPathLength)
//    //            {
//    //                Debug.LogWarning($"Could not generate path {i + 1} for spawn {spawn.position}. Trying fallback.");
//    //                newPath = pathfinder.FindPath(spawn.position, tower.position, penalties, 0f, true);
//    //                if (newPath == null || newPath.Count < minPathLength)
//    //                {
//    //                    Debug.LogError($"Fatal: Could not find any path for spawn {spawn.position}, even with Dijkstra's.");
//    //                    return false;
//    //                }
//    //            }

//    //            bool isTooSimilar = false;
//    //            foreach (var existingPath in generatedPathsForSpawn)
//    //            {
//    //                if (CalculateRedundancy(newPath, existingPath) > maxPathRedundancy)
//    //                {
//    //                    isTooSimilar = true;
//    //                    break;
//    //                }
//    //            }

//    //            if (isTooSimilar)
//    //            {
//    //                i--;
//    //                Debug.Log("Generated path was too similar, retrying.");
//    //                continue;
//    //            }

//    //            generatedPathsForSpawn.Add(newPath);
//    //            if (drawDebugPaths) debugAllPaths.Add(newPath);

//    //            int basePenalty = pathfinder.GetManhattanDistance(newPath[0], newPath[1]) * 2;
//    //            foreach (var node in newPath)
//    //            {
//    //                if (!penalties.ContainsKey(node)) penalties.Add(node, 0);
//    //                penalties[node] += basePenalty;
//    //            }
//    //        }
//    //        foreach (var nodePath in generatedPathsForSpawn)
//    //        {
//    //            spawnData.paths.Add(nodePath.Select(n => new SerializableVector3(n.worldPosition)).ToList());
//    //        }
//    //        FinalLevelPaths.spawnPaths.Add(spawnData);
//    //    }

//    //    return true;
//    //}

//    float CalculateRedundancy(List<PathNode> pathA, List<PathNode> pathB)
//    {
//        if (pathA.Count == 0 || pathB.Count == 0) return 0;
//        var setA = new HashSet<PathNode>(pathA);
//        int sharedCount = pathB.Count(node => setA.Contains(node));
//        return (float)sharedCount / Mathf.Min(pathA.Count, pathB.Count);
//    }

//    void OnDrawGizmos()
//    {
//        if (!drawDebugPaths || debugAllPaths == null) return;

//        Color[] colors = { Color.cyan, Color.green, Color.magenta, Color.yellow, Color.white };
//        int colorIndex = 0;

//        foreach (var path in debugAllPaths)
//        {
//            if (path == null || path.Count < 2) continue;

//            Gizmos.color = colors[colorIndex % colors.Length];
//            for (int i = 0; i < path.Count - 1; i++)
//            {
//                Gizmos.DrawLine(path[i].worldPosition, path[i + 1].worldPosition);
//            }
//            colorIndex++;
//        }
//    }
//}