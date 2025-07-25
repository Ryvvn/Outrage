// Assets/Scripts/ExpansionSystem/Procedural/ProceduralChunkGenerator.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ProceduralChunkGenerator : MonoBehaviour
{
    public static ProceduralChunkGenerator Instance { get; private set; }

    [Header("Chunk Settings")]
    public Vector2Int chunkSize = new Vector2Int(32, 32);
    public Vector3 cellSize => gridManager != null ? gridManager.CellSize : Vector3.one;

    [Header("Generation Settings")]
    public int globalSeed = 12345;
    public int playerStartClearanceRadius = 10;

    private System.Random pseudoRandom;
    private GridManager gridManager;
    private Pathfinder pathfinder;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("ProceduralChunkGenerator requires a GridManager in the scene!");
        }

        pathfinder = Pathfinder.Instance;
        if (pathfinder == null)
        {
            Debug.LogError("ProceduralChunkGenerator requires a Pathfinder in the scene!");
        }
    }

    public IEnumerator GenerateChunkCoroutine(ChunkAddress addr, BiomeGenerationRules rules, System.Action<GameObject> callback)
    {
        GameObject chunkRoot = new GameObject($"Chunk_{addr.x}_{addr.y}");
        int chunkSeed = globalSeed + (addr.x * 1000) + addr.y;
        pseudoRandom = new System.Random(chunkSeed);
        Grid grid = chunkRoot.AddComponent<Grid>();
        grid.cellSize = this.cellSize;

        float chunkWorldWidth = chunkSize.x * cellSize.x;
        float chunkWorldHeight = chunkSize.y * cellSize.y;

        Vector3 worldOrigin = gridManager.transform.position;
        chunkRoot.transform.position = worldOrigin + new Vector3(addr.x * chunkWorldWidth, addr.y * chunkWorldHeight, 0);

        Tilemap backgroundTilemap = CreateTilemap(chunkRoot, "Tilemap_Base", "Background", 0);
        Tilemap pathTilemap = CreateTilemap(chunkRoot, "Tilemap_Path", "Paths", 0);
        Tilemap obstacleTilemap = CreateTilemap(chunkRoot, "Tilemap_Obstacle", "Obstacles", 0);

        var tilemapCollider = obstacleTilemap.gameObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.usedByComposite = true;
        obstacleTilemap.gameObject.layer = LayerMask.NameToLayer("Obstacles");

        var rb = obstacleTilemap.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var composite = obstacleTilemap.gameObject.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

        GenerateNoiseAndBaseTiles(chunkRoot.transform, backgroundTilemap, rules);
        yield return null;

        GenerateObstacles(chunkRoot.transform, obstacleTilemap, rules);
        yield return null;

        GeneratePaths(chunkRoot.transform, pathTilemap, obstacleTilemap, rules);
        yield return null;

        GenerateResources(chunkRoot.transform, rules);

        callback(chunkRoot);
    }

    private void GeneratePaths(Transform chunkTransform, Tilemap pathTilemap, Tilemap obstacleTilemap, BiomeGenerationRules rules)
    {
        if (pathfinder == null || rules.tilePalette.pathTiles.Length == 0) return;

        Vector3 left = chunkTransform.position + new Vector3(0, (chunkSize.y * cellSize.y) / 2f, 0);
        Vector3 right = chunkTransform.position + new Vector3(chunkSize.x * cellSize.x, (chunkSize.y * cellSize.y) / 2f, 0);
        Vector3 bottom = chunkTransform.position + new Vector3((chunkSize.x * cellSize.x) / 2f, 0, 0);
        Vector3 top = chunkTransform.position + new Vector3((chunkSize.x * cellSize.x) / 2f, chunkSize.y * cellSize.y, 0);

        // --- MODIFIED ---
        // Call FindPath with ignoreWalkability set to true.
        List<Vector3> pathHorizontal = pathfinder.FindPath(left, right, true);
        List<Vector3> pathVertical = pathfinder.FindPath(bottom, top, true);
        // --- END MODIFICATION ---

        HashSet<Vector3Int> pathPositions = new HashSet<Vector3Int>();

        if (pathHorizontal != null)
        {
            foreach (var worldPos in pathHorizontal)
            {
                pathPositions.Add(pathTilemap.WorldToCell(worldPos));
            }
        }
        else { Debug.LogError($"Could not generate horizontal path for chunk at {chunkTransform.position}"); }

        if (pathVertical != null)
        {
            foreach (var worldPos in pathVertical)
            {
                pathPositions.Add(pathTilemap.WorldToCell(worldPos));
            }
        }
        else { Debug.LogError($"Could not generate vertical path for chunk at {chunkTransform.position}"); }


        foreach (var tilePos in pathPositions)
        {
            pathTilemap.SetTile(tilePos, rules.tilePalette.pathTiles[0]);
            if (obstacleTilemap.HasTile(tilePos))
            {
                obstacleTilemap.SetTile(tilePos, null);
            }
            gridManager.UpdateNodeWalkability(pathTilemap.GetCellCenterWorld(tilePos), true);
        }
    }

    private void GenerateNoiseAndBaseTiles(Transform chunkTransform, Tilemap tilemap, BiomeGenerationRules rules)
    {
        if (rules.tilePalette.backgroundTiles.Length == 0) return;

        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePos, rules.tilePalette.backgroundTiles[0]);
                gridManager.UpdateNodeWalkability(tilemap.GetCellCenterWorld(tilePos), true);
            }
        }
    }

    private void GenerateObstacles(Transform chunkTransform, Tilemap tilemap, BiomeGenerationRules rules)
    {
        if (rules.tilePalette.obstacleTiles.Length == 0) return;

        float offsetX = pseudoRandom.Next(0, 10000) + rules.noiseOffset.x;
        float offsetY = pseudoRandom.Next(0, 10000) + rules.noiseOffset.y;

        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);

                if (Vector3.Distance(worldPos, Vector3.zero) < playerStartClearanceRadius) continue;

                float noiseValue = Mathf.PerlinNoise(
                    (worldPos.x) * rules.noiseScale + offsetX,
                    (worldPos.y) * rules.noiseScale + offsetY
                );

                if (noiseValue < rules.obstacleDensity)
                {
                    tilemap.SetTile(tilePos, rules.tilePalette.obstacleTiles[0]);
                    gridManager.UpdateNodeWalkability(worldPos, false);
                }
            }
        }
    }

    private void GenerateResources(Transform chunkTransform, BiomeGenerationRules rules)
    {
        if (ObjectPooler.Instance == null) return;
        int resourcesToPlace = Mathf.FloorToInt(chunkSize.x * chunkSize.y * rules.resourceDensity);

        for (int i = 0; i < resourcesToPlace; i++)
        {
            int x = pseudoRandom.Next(0, chunkSize.x);
            int y = pseudoRandom.Next(0, chunkSize.y);

            Vector3 worldPos = chunkTransform.TransformPoint(new Vector3(x * cellSize.x, y * cellSize.y, 0));

            if (Vector3.Distance(worldPos, Vector3.zero) < playerStartClearanceRadius) continue;

            PathNode node = gridManager.GetNodeFromWorldPoint(worldPos);
            if (node == null || !node.isWalkable) continue;

            ResourceType chosenType = ChooseResourceType(rules.resourceSet);
            if (chosenType != null && chosenType.prefab != null)
            {
                ObjectPooler.Instance.SpawnFromPool(chosenType.prefab.tag, worldPos, Quaternion.identity);
                gridManager.UpdateNodeWalkability(worldPos, false);
            }
        }
    }

    private Tilemap CreateTilemap(GameObject parent, string name, string sortingLayer, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Tilemap tilemap = go.AddComponent<Tilemap>();
        TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = order;
        return tilemap;
    }

    private ResourceType ChooseResourceType(List<ResourceType> resourceSet)
    {
        if (resourceSet == null || resourceSet.Count == 0) return null;
        float totalDensity = resourceSet.Sum(r => r.density);
        if (totalDensity == 0) return null;
        float randomPoint = (float)pseudoRandom.NextDouble() * totalDensity;
        foreach (var resource in resourceSet)
        {
            if (randomPoint < resource.density) return resource;
            randomPoint -= resource.density;
        }
        return resourceSet[0];
    }
}