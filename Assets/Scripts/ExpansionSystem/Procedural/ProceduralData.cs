// Scripts/ExpansionSystem/Procedural/ProceduralData.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#region Enums
/// <summary>
/// Defines the overall pattern for generated paths.
/// </summary>
public enum PathPatternType
{
    Winding,
    Straight,
    Branching,
    Organic, // Similar to winding but with more variation
    Maze
}

/// <summary>
/// Defines how obstacles are grouped together.
/// </summary>
public enum ClusteringRule
{
    None,
    Scattered, // Random placement
    Clustered, // Obstacles grouped together
    Linear     // Obstacles in lines or veins
}
#endregion

/// <summary>
/// Defines a palette of tiles for a specific biome.
/// </summary>
[System.Serializable]
public class TilePalette
{
    public string paletteName;
    [Header("Tile Categories")]
    public TileBase[] backgroundTiles; // Tiles for ground, water, etc.
    public TileBase[] obstacleTiles;   // Tiles for non-walkable objects like rocks, trees.
    public TileBase[] pathTiles;       // Optional tiles specifically for paths.
    public TileBase[] decorativeTiles; // Non-functional visual tiles.
}

/// <summary>
/// A ScriptableObject containing all the rules for generating a chunk for a specific biome.
/// </summary>
[CreateAssetMenu(fileName = "NewBiomeRules", menuName = "Tower Defense/Biome Generation Rules")]
public class BiomeGenerationRules : ScriptableObject
{
    public string biomeName;
    public TilePalette tilePalette;

    [Header("Density Controls")]
    [Range(0f, 1f)] public float obstacleDensity = 0.2f;
    [Range(0f, 1f)] public float decorativeDensity = 0.1f;

    [Header("Pattern Controls")]
    public PathPatternType pathPattern = PathPatternType.Winding;
    public ClusteringRule obstacleCluster = ClusteringRule.Scattered;
    [Min(1)] public int pathBranchCount = 1;

    [Header("Noise Parameters")]
    [Tooltip("Controls the 'zoom' level of the Perlin noise pattern.")]
    public float noiseScale = 0.1f;
    [Tooltip("The cutoff value for noise; values above this might be treated differently.")]
    public float noiseThreshold = 0.5f;
    public Vector2 noiseOffset = Vector2.zero;
}


/// <summary>
/// Represents a chunk of the map generated procedurally at runtime.
/// This is a plain C# class, not a ScriptableObject, as it's created dynamically.
/// </summary>
[System.Serializable]
public class ProceduralExpansionChunk
{
    public string chunkName;
    public Vector2Int chunkSize;
    public BiomeGenerationRules biomeRules;
    public List<Vector2Int> connectionPoints;
    public TerrainEffect[] environmentalEffects;
    public PathingModifier pathingData;

    // Data generated at runtime. Not serialized as it's transient.
    [System.NonSerialized] public TileBase[] generatedBackgroundTileData;
    [System.NonSerialized] public TileBase[] generatedObstacleTileData;
    [System.NonSerialized] public List<Vector2Int> generatedBuildableAreas;

    public ProceduralExpansionChunk()
    {
        connectionPoints = new List<Vector2Int>();
        generatedBuildableAreas = new List<Vector2Int>();
    }
}