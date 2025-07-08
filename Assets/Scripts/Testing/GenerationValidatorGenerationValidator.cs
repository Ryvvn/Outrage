// Scripts/Testing/GenerationValidator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// A tool for testing the procedural generation system's performance and validity.
/// Place this on a GameObject in your scene and call RunGenerationTests() from a custom editor button or script.
/// </summary>
public class GenerationValidator : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The procedural generator to test.")]
    [SerializeField] private ProceduralChunkGenerator chunkGenerator;

    [Header("Test Configuration")]
    [Tooltip("The list of biome rules to run tests on.")]
    [SerializeField] private List<BiomeGenerationRules> biomesToTest;
    [Tooltip("The number of chunks to generate for each biome type.")]
    public int testGenerationCount = 100;

    [Header("Success Criteria")]
    [Tooltip("The minimum percentage of generated chunks that must pass validation.")]
    [Range(0f, 1f)] public float requiredPassRate = 0.95f;

    /// <summary>
    /// Runs a full suite of generation tests on all specified biomes.
    /// </summary>
    public void RunGenerationTests()
    {
        if (chunkGenerator == null)
        {
            UnityEngine.Debug.LogError("GenerationValidator: ProceduralChunkGenerator reference not set!");
            return;
        }
        if (biomesToTest == null || biomesToTest.Count == 0)
        {
            UnityEngine.Debug.LogError("GenerationValidator: No BiomeGenerationRules have been assigned for testing!");
            return;
        }

        UnityEngine.Debug.Log("--- STARTING PROCEDURAL GENERATION VALIDATION ---");

        foreach (var biome in biomesToTest)
        {
            RunTestForBiome(biome);
        }

        UnityEngine.Debug.Log("--- VALIDATION COMPLETE ---");
    }

    private void RunTestForBiome(BiomeGenerationRules biome)
    {
        UnityEngine.Debug.Log($"--- Testing Biome: {biome.biomeName} ---");

        int successCount = 0;
        long totalMilliseconds = 0;
        var stopwatch = new Stopwatch();

        for (int i = 0; i < testGenerationCount; i++)
        {
            stopwatch.Restart();
            // In a real test, connection points might be varied. For now, an empty list is sufficient.
            var chunk = chunkGenerator.GenerateChunk(biome, new List<Vector2Int>(), i);
            stopwatch.Stop();

            totalMilliseconds += stopwatch.ElapsedMilliseconds;

            // A simple validation check. This can be expanded.
            if (chunk.generatedBuildableAreas.Count >= chunkGenerator.requiredBuildableSpaces && !chunk.chunkName.Contains("Fallback"))
            {
                successCount++;
            }
        }

        float passRate = (float)successCount / testGenerationCount;
        long averageTime = totalMilliseconds / testGenerationCount;

        UnityEngine.Debug.Log($"Result: {successCount} / {testGenerationCount} chunks passed validation.");
        UnityEngine.Debug.Log($"Pass Rate: {passRate:P2}");
        UnityEngine.Debug.Log($"Average Generation Time: {averageTime} ms");

        if (passRate < requiredPassRate)
        {
            UnityEngine.Debug.LogError($"VALIDATION FAILED for {biome.name}. Pass rate {passRate:P2} is below required {requiredPassRate:P2}.");
        }
        else
        {
            UnityEngine.Debug.Log($"VALIDATION PASSED for {biome.name}.");
        }
    }
}