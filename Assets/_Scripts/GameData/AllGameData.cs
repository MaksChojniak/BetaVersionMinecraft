using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;





[Serializable]
public class GameData
{
    public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
    public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;
    public string jsonChunkDataDictionary;
    public int chunkSize;
    public int chunkHeight;
}


[Serializable]
public class WorldData
{
    public int mapSizeInChunk;
    public int chunkSize;
    public int chunkHeight;
    public int chunkDrawRange;
    public Vector2Int mapSeedOffset;
    public bool tree;

    public WorldData()
    {
        mapSizeInChunk = 1;
        chunkSize = 1;
        chunkHeight = 1;
        chunkDrawRange = 1;
        mapSeedOffset = Vector2Int.zero;
        tree = false;
    }
}


[Serializable]
public class PlayerData
{
    public Vector3 playerPosition;
    public Vector3 spawnPos;
    public Quaternion playerRotation;

    public PlayerData()
    {
        spawnPos = new Vector3(15, 15, 15);
        playerPosition = spawnPos;
        playerRotation = Quaternion.identity;
    }
}


[Serializable]
public class TextureData
{
    public BlockType blockType;
    public Vector2Int up, down, side;
    public bool isSolid = true;
    public bool generatesCollider = true;
    public int durability = 1;
    public bool placable = true;
}


[Serializable]
public struct WorldGenerationData
{
    public List<Vector3Int> chunkPositionsToCreate;
    public List<Vector3Int> chunkDataPositionsToCreate;
    public List<Vector3Int> chunkPositionsToRemove;
    public List<Vector3Int> chunkDataToRemove;
    public List<Vector3Int> chunkPositionsToUpdate;
}
