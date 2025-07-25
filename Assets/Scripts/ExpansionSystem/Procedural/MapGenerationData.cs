// "Assets/Scripts/ExpansionSystem/Procedural/MapGenerationData.cs"
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#region Enums
/// <summary>
/// Defines the overall pattern for generated paths.
/// SRS Requirement: 1. Architecture Overview (pathPattern)
/// </summary>
public enum PathPatternType
{
    Winding,
    Straight,
    Branching,
    Organic,
    Maze
}

/// <summary>
/// Defines how obstacles are grouped together.
/// </summary>
public enum ClusteringRule
{
    None,
    Scattered,
    Clustered,
    Linear
}

/// <summary>
/// Represents a type of resource that can be spawned.
/// SRS Requirement: 1. Architecture Overview (resourceSet)
/// </summary>
[System.Serializable]
public class ResourceType
{
    public string name;
    public GameObject prefab; // The resource prefab (e.g., a rock or tree with a collider)
    [Range(0f, 1f)]
    public float density; // The chance for this resource to be chosen
}
#endregion

/// <summary>
/// A ScriptableObject containing all the rules for generating a chunk for a specific biome.
/// SRS Requirement: 1. Architecture Overview, 6. Extensibility
/// </summary>
[CreateAssetMenu(fileName = "NewBiomeRules", menuName = "Tower Defense/Biome Generation Rules")]
public class BiomeGenerationRules : ScriptableObject
{
    [Header("Core Info")]
    public string biomeName;
    public TilePalette tilePalette;

    [Header("Noise Parameters")]
    [Tooltip("Controls the 'zoom' level of the Perlin noise pattern.")]
    public float noiseScale = 0.1f;
    public Vector2 noiseOffset = Vector2.zero;

    [Header("Density Controls")]
    [Range(0f, 1f)] public float obstacleDensity = 0.2f;
    [Range(0f, 1f)] public float decorativeDensity = 0.1f;
    [Range(0f, 1f)] public float resourceDensity = 0.05f;

    [Header("Content Sets")]
    public List<ResourceType> resourceSet;
    // public List<GameObject> decorativeSet; // Placeholder for future expansion

    [Header("Pattern Controls")]
    public PathPatternType pathPattern = PathPatternType.Winding;
    public int pathBranchCount = 1;
}

/// <summary>
/// Defines a palette of tiles for a specific biome.
/// </summary>
[System.Serializable]
public class TilePalette
{
    public string paletteName;
    public TileBase[] backgroundTiles;
    public TileBase[] obstacleTiles;
    public TileBase[] pathTiles;
    public TileBase[] decorativeTiles;
}