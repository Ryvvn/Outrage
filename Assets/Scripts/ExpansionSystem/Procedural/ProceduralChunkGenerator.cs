// Scripts/ExpansionSystem/Procedural/ProceduralChunkGenerator.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Central orchestrator for creating procedural map chunks.
/// </summary>
public class ProceduralChunkGenerator : MonoBehaviour
{
    public static ProceduralChunkGenerator Instance { get; private set; }

    [Header("Generation Settings")]
    [Tooltip("The standard size for a generated chunk.")]
    public Vector2Int standardChunkSize = new Vector2Int(16, 16);
    [Tooltip("A global seed for the entire run for deterministic results. 0 means random.")]
    public int globalSeed;

    [Header("Validation Settings")]
    [Tooltip("How many times to try generating a chunk before falling back to a simpler one.")]
    public int maxGenerationAttempts = 5;
    [Tooltip("The minimum number of buildable tiles required for a chunk to be valid.")]
    public int requiredBuildableSpaces = 10;

    private TerrainGenerator terrainGenerator;
    private PathGenerator pathGenerator;
    private ObstacleGenerator obstacleGenerator;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        terrainGenerator = new TerrainGenerator();
        pathGenerator = new PathGenerator();
        obstacleGenerator = new ObstacleGenerator();

        if (globalSeed == 0)
        {
            globalSeed = Random.Range(1, 999999);
        }
    }

    /// <summary>
    /// Generates a procedural chunk based on the given rules, with retries and a fallback.
    /// </summary>
    public ProceduralExpansionChunk GenerateChunk(BiomeGenerationRules rules, List<Vector2Int> existingConnectionPoints, int expansionIndex)
    {
        int chunkSeed = globalSeed + expansionIndex;

        for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
        {
            var chunk = GenerateChunkAttempt(rules, existingConnectionPoints, chunkSeed + attempt);
            if (ValidateChunk(chunk))
            {
                Debug.Log($"Chunk '{rules.biomeName}' generated successfully on attempt {attempt + 1}.");
                return chunk;
            }
        }

        Debug.LogWarning($"Failed to generate a valid chunk for '{rules.biomeName}' after {maxGenerationAttempts} attempts. Generating fallback chunk.");
        return GenerateFallbackChunk(rules, existingConnectionPoints, chunkSeed);
    }

    private ProceduralExpansionChunk GenerateChunkAttempt(BiomeGenerationRules rules, List<Vector2Int> connectionPoints, int seed)
    {
        var chunk = new ProceduralExpansionChunk
        {
            chunkName = rules.biomeName,
            chunkSize = standardChunkSize,
            biomeRules = rules,
            connectionPoints = new List<Vector2Int> { new Vector2Int(0, standardChunkSize.y / 2) }
        };

        int arraySize = chunk.chunkSize.x * chunk.chunkSize.y;
        chunk.generatedBackgroundTileData = new TileBase[arraySize];
        chunk.generatedObstacleTileData = new TileBase[arraySize];

        terrainGenerator.GenerateBaseTerrain(chunk, seed);
        var pathData = pathGenerator.GeneratePathNetwork(chunk, connectionPoints, seed);
        obstacleGenerator.PlaceObstacles(chunk, pathData, seed);
        CalculateBuildableAreas(chunk);

        return chunk;
    }

    private bool ValidateChunk(ProceduralExpansionChunk chunk)
    {
        if (chunk.generatedBuildableAreas.Count < requiredBuildableSpaces)
        {
            return false;
        }
        return true;
    }

    private void CalculateBuildableAreas(ProceduralExpansionChunk chunk)
    {
        chunk.generatedBuildableAreas.Clear();
        for (int y = 0; y < chunk.chunkSize.y; y++)
        {
            for (int x = 0; x < chunk.chunkSize.x; x++)
            {
                int index = y * chunk.chunkSize.x + x;
                if (chunk.generatedBackgroundTileData[index] != null && chunk.generatedObstacleTileData[index] == null)
                {
                    chunk.generatedBuildableAreas.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    private ProceduralExpansionChunk GenerateFallbackChunk(BiomeGenerationRules rules, List<Vector2Int> connectionPoints, int seed)
    {
        var chunk = new ProceduralExpansionChunk
        {
            chunkName = $"{rules.biomeName} (Fallback)",
            chunkSize = standardChunkSize,
            biomeRules = rules,
            connectionPoints = new List<Vector2Int> { new Vector2Int(0, standardChunkSize.y / 2) }
        };

        int arraySize = chunk.chunkSize.x * chunk.chunkSize.y;
        chunk.generatedBackgroundTileData = new TileBase[arraySize];
        chunk.generatedObstacleTileData = new TileBase[arraySize];

        if (rules.tilePalette.backgroundTiles.Length > 0)
        {
            TileBase backgroundTile = rules.tilePalette.backgroundTiles[0];
            for (int i = 0; i < arraySize; i++)
            {
                chunk.generatedBackgroundTileData[i] = backgroundTile;
            }
        }

        CalculateBuildableAreas(chunk);
        return chunk;
    }
}

#region Sub-Generators

/// <summary>
/// A job-based terrain generator that uses Perlin noise.
/// </summary>
public class TerrainGenerator
{
    // A job for calculating Perlin noise in parallel.
    [BurstCompile]
    private struct GenerateNoiseJob : IJobParallelFor
    {
        [ReadOnly] public int width;
        [ReadOnly] public float scale;
        [ReadOnly] public float offsetX;
        [ReadOnly] public float offsetY;

        [WriteOnly] public NativeArray<float> noiseMap;

        public void Execute(int index)
        {
            int x = index % width;
            int y = index / width;

            float sampleX = (float)x / width * scale + offsetX;
            float sampleY = (float)y / width * scale + offsetY;

            noiseMap[index] = Mathf.PerlinNoise(sampleX, sampleY);
        }
    }

    public void GenerateBaseTerrain(ProceduralExpansionChunk chunk, int seed)
    {
        if (chunk.biomeRules.tilePalette.backgroundTiles.Length == 0) return;

        Random.InitState(seed);
        float offsetX = Random.value * 1000f;
        float offsetY = Random.value * 1000f;

        int totalTiles = chunk.chunkSize.x * chunk.chunkSize.y;
        var noiseMap = new NativeArray<float>(totalTiles, Allocator.TempJob);

        var job = new GenerateNoiseJob
        {
            width = chunk.chunkSize.x,
            scale = chunk.biomeRules.noiseScale,
            offsetX = offsetX,
            offsetY = offsetY,
            noiseMap = noiseMap
        };

        JobHandle handle = job.Schedule(totalTiles, 64);
        handle.Complete();

        for (int i = 0; i < totalTiles; i++)
        {
            // Simple example: Use noise to pick between the first two background tiles.
            // A more complex system would use weights and more thresholds.
            if (noiseMap[i] > chunk.biomeRules.noiseThreshold && chunk.biomeRules.tilePalette.backgroundTiles.Length > 1)
            {
                chunk.generatedBackgroundTileData[i] = chunk.biomeRules.tilePalette.backgroundTiles[1];
            }
            else
            {
                chunk.generatedBackgroundTileData[i] = chunk.biomeRules.tilePalette.backgroundTiles[0];
            }
        }

        noiseMap.Dispose();
    }
}

public class PathGenerator
{
    public List<Vector2Int> GeneratePathNetwork(ProceduralExpansionChunk chunk, List<Vector2Int> connectionPoints, int seed)
    {
        var path = new List<Vector2Int>();
        // Assume first connection point is the entry on the left edge.
        Vector2Int entry = chunk.connectionPoints[0];
        Vector2Int exit = new Vector2Int(chunk.chunkSize.x - 1, chunk.chunkSize.y / 2);

        // This is a placeholder for a more complex pathing algorithm (e.g., A*, drunkard's walk).
        // For now, it creates a simple L-shaped path.
        Vector2Int current = entry;
        path.Add(current);

        // Move horizontally
        while (current.x < exit.x)
        {
            current.x++;
            path.Add(current);
        }

        // Move vertically
        while (current.y != exit.y)
        {
            if (current.y < exit.y) current.y++;
            else current.y--;
            path.Add(current);
        }

        return path;
    }
}

public class ObstacleGenerator
{
    public void PlaceObstacles(ProceduralExpansionChunk chunk, List<Vector2Int> pathTiles, int seed)
    {
        if (chunk.biomeRules.tilePalette.obstacleTiles.Length == 0) return;

        Random.InitState(seed);
        var pathSet = new HashSet<Vector2Int>(pathTiles);
        int obstacleCount = (int)(chunk.chunkSize.x * chunk.chunkSize.y * chunk.biomeRules.obstacleDensity);

        for (int i = 0; i < obstacleCount; i++)
        {
            int x = Random.Range(0, chunk.chunkSize.x);
            int y = Random.Range(0, chunk.chunkSize.y);
            var pos = new Vector2Int(x, y);

            // Do not place obstacles on path tiles or near connection points.
            if (pathSet.Contains(pos) || chunk.connectionPoints.Contains(pos))
            {
                i--; // Retry this placement.
                continue;
            }

            int index = y * chunk.chunkSize.x + x;
            if (chunk.generatedObstacleTileData[index] == null)
            {
                chunk.generatedObstacleTileData[index] = chunk.biomeRules.tilePalette.obstacleTiles[0];
            }
            else
            {
                i--;
            }
        }
    }
}
#endregion