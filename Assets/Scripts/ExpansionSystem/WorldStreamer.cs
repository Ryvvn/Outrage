// Scripts/ExpansionSystem/WorldStreamer.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldStreamer : MonoBehaviour
{
    [Header("World & Streaming")]
    public Transform target;
    public Vector2Int worldSizeInChunks = new Vector2Int(10, 10);
    public int loadRadius = 3;
    public int unloadRadius = 4;
    public float checkInterval = 0.5f;
    public bool preBakeWorld = false;

    [Header("Dependencies")]
    public ProceduralChunkGenerator chunkGenerator;
    public BiomeGenerationRules defaultBiome;

    private Dictionary<ChunkAddress, GameObject> activeChunks = new Dictionary<ChunkAddress, GameObject>();
    private ChunkAddress currentTargetAddress;
    private float timer;
    private bool isGenerating = false;

    void Start()
    {
        if (target == null || chunkGenerator == null || defaultBiome == null)
        {
            Debug.LogError("WorldStreamer is missing critical references! Disabling.", this);
            this.enabled = false;
            return;
        }
        StartCoroutine(GenerateWorldCoroutine());
    }

    private IEnumerator GenerateWorldCoroutine()
    {
        isGenerating = true;
        if (preBakeWorld)
        {
            yield return StartCoroutine(PreBakeWorldCoroutine());
        }
        currentTargetAddress = GetAddressFromPosition(target.position);
        yield return StartCoroutine(UpdateChunksCoroutine());
        isGenerating = false;
    }

    void Update()
    {
        if (isGenerating) return;
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            ChunkAddress newAddress = GetAddressFromPosition(target.position);
            if (newAddress.x != currentTargetAddress.x || newAddress.y != currentTargetAddress.y)
            {
                currentTargetAddress = newAddress;
                StartCoroutine(UpdateChunksCoroutine());
            }
        }
    }

    private IEnumerator PreBakeWorldCoroutine()
    {
        for (int x = 0; x < worldSizeInChunks.x; x++)
        {
            for (int y = 0; y < worldSizeInChunks.y; y++)
            {
                ChunkAddress addr = new ChunkAddress(x, y);
                if (!activeChunks.ContainsKey(addr))
                {
                    yield return StartCoroutine(LoadChunkCoroutine(addr));
                }
            }
        }
    }

    private IEnumerator UpdateChunksCoroutine()
    {
        var chunksToKeep = new HashSet<ChunkAddress>();
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                ChunkAddress addr = new ChunkAddress(currentTargetAddress.x + x, currentTargetAddress.y + y);
                if (!addr.IsInside(worldSizeInChunks)) continue;
                chunksToKeep.Add(addr);
                if (!activeChunks.ContainsKey(addr))
                {
                    yield return StartCoroutine(LoadChunkCoroutine(addr));
                }
            }
        }
        var chunksToUnload = new List<ChunkAddress>();
        foreach (var chunk in activeChunks.Keys)
        {
            if (!chunksToKeep.Contains(chunk)) chunksToUnload.Add(chunk);
        }
        foreach (var addr in chunksToUnload) UnloadChunk(addr);
    }

    private IEnumerator LoadChunkCoroutine(ChunkAddress addr)
    {
        if (activeChunks.ContainsKey(addr)) yield break;
        GameObject chunkObject = null;
        yield return StartCoroutine(chunkGenerator.GenerateChunkCoroutine(addr, defaultBiome, (result) => {
            chunkObject = result;
        }));
        if (chunkObject != null)
        {
            activeChunks.Add(addr, chunkObject);
        }
    }

    private void UnloadChunk(ChunkAddress addr)
    {
        if (activeChunks.TryGetValue(addr, out GameObject chunkObject))
        {
            Destroy(chunkObject);
            activeChunks.Remove(addr);
        }
    }

    private ChunkAddress GetAddressFromPosition(Vector3 position)
    {
        // CORRECTED: Must account for cell size to get the true world size of a chunk
        float worldChunkWidth = chunkGenerator.chunkSize.x * chunkGenerator.cellSize.x;
        float worldChunkHeight = chunkGenerator.chunkSize.y * chunkGenerator.cellSize.y;

        if (worldChunkWidth <= 0 || worldChunkHeight <= 0) return new ChunkAddress(0, 0);

        int x = Mathf.FloorToInt(position.x / worldChunkWidth);
        int y = Mathf.FloorToInt(position.y / worldChunkHeight);
        return new ChunkAddress(x, y);
    }
}