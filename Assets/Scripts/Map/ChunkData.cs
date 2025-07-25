// "Assets/Scripts/Map/ChunkData.cs"
using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a unique address for a chunk in the world grid.
/// </summary>
[System.Serializable]
public struct ChunkAddress
{
    public int x;
    public int y;

    public ChunkAddress(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Helper method to check if this address is within the defined world boundaries.
    /// </summary>
    /// <param name="worldSizeInChunks">The total size of the world in chunks.</param>
    /// <returns>True if the address is valid.</returns>
    public bool IsInside(Vector2Int worldSizeInChunks)
    {
        return x >= 0 && y >= 0 && x < worldSizeInChunks.x && y < worldSizeInChunks.y;
    }
}

// The rest of the file (ChunkData struct) remains unchanged, but is included for completeness.
[System.Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 160)]
public struct ChunkData
{
    // --- Header (16 bytes) ---
    public Vector2Int position;
    public ulong seed;

    // --- Metadata (8 bytes) ---
    public ushort biomeID;
    public ushort reserved;
    public uint aStarPortalChecksum;

    // --- Walkability (128 bytes) ---
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public byte[] walkability;

    // --- Dynamic State (8 bytes) ---
    public uint resourceStateChecksum;
    public byte heightByte;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] padding;

    public bool IsTileWalkable(int x, int y)
    {
        int tileIndex = y * 32 + x;
        int byteIndex = tileIndex / 8;
        int bitIndex = tileIndex % 8;
        if (walkability == null || byteIndex >= walkability.Length) return false;
        return (walkability[byteIndex] & (1 << bitIndex)) != 0;
    }

    public void SetTileWalkable(int x, int y, bool isWalkable)
    {
        int tileIndex = y * 32 + x;
        int byteIndex = tileIndex / 8;
        int bitIndex = tileIndex % 8;
        if (walkability == null) { walkability = new byte[128]; }
        if (byteIndex >= walkability.Length) return;
        if (isWalkable) { walkability[byteIndex] |= (byte)(1 << bitIndex); }
        else { walkability[byteIndex] &= (byte)~(1 << bitIndex); }
    }
}