using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WorldDataHelper
{
    public static Vector3Int ChunkPositionFromBlockCoords(World world, Vector3Int worldBlockPosition)
    {
        return new Vector3Int
        {
            x = Mathf.FloorToInt(worldBlockPosition.x / (float)world.chunkSize) * world.chunkSize,
            y = Mathf.FloorToInt(worldBlockPosition.y / (float)world.chunkHeight) * world.chunkHeight,
            z = Mathf.FloorToInt(worldBlockPosition.z / (float)world.chunkSize) * world.chunkSize
        };
    }

    internal static List<Vector3Int> GetChunkPositionsAroundPlayer(World world, Vector3Int playerPosition)
    {
        int startX = playerPosition.x - (world.chunkDrawingRange) * world.chunkSize;
        int startZ = playerPosition.z - (world.chunkDrawingRange) * world.chunkSize;
        int endX = playerPosition.x + (world.chunkDrawingRange) * world.chunkSize;
        int endZ = playerPosition.z + (world.chunkDrawingRange) * world.chunkSize;

        List<Vector3Int> chunkPositionsToCreate = new List<Vector3Int>();
        for (int x = startX; x <= endX; x += world.chunkSize)
        {
            for (int z = startZ; z <= endZ; z += world.chunkSize)
            {
                Vector3Int chunkPos = ChunkPositionFromBlockCoords(world, new Vector3Int(x, 0, z));
                chunkPositionsToCreate.Add(chunkPos);
                if (x >= playerPosition.x - world.chunkSize
                    && x <= playerPosition.x + world.chunkSize
                    && z >= playerPosition.z - world.chunkSize
                    && z <= playerPosition.z + world.chunkSize)
                {
                    for (int y = -world.chunkHeight; y >= playerPosition.y - world.chunkHeight * 2; y -= world.chunkHeight)
                    {
                        chunkPos = ChunkPositionFromBlockCoords(world, new Vector3Int(x, y, z));
                        chunkPositionsToCreate.Add(chunkPos);
                    }
                }
            }
        }

        return chunkPositionsToCreate;
    }

    internal static void RemoveChunkData(World world, Vector3Int pos)
    {
        world.gameData.chunkDataDictionary.Remove(pos);
    }

    internal static void RemoveChunk(World world, Vector3Int pos)
    {
        ChunkRenderer chunk = null;
        if (world.gameData.chunkDictionary.TryGetValue(pos, out chunk))
        {
            world.worldRenderer.RemoveChunk(chunk);
            world.gameData.chunkDictionary.Remove(pos);
        }
    }

    internal static List<Vector3Int> GetDataPositionsAroundPlayer(World world, Vector3Int playerPosition)
    {
        int startX = playerPosition.x - (world.chunkDrawingRange + 1) * world.chunkSize;
        int startZ = playerPosition.z - (world.chunkDrawingRange + 1) * world.chunkSize;
        int endX = playerPosition.x + (world.chunkDrawingRange + 1) * world.chunkSize;
        int endZ = playerPosition.z + (world.chunkDrawingRange + 1) * world.chunkSize;

        List<Vector3Int> chunkDataPositionsToCreate = new List<Vector3Int>();
        for (int x = startX; x <= endX; x += world.chunkSize)
        {
            for (int z = startZ; z <= endZ; z += world.chunkSize)
            {
                Vector3Int chunkPos = ChunkPositionFromBlockCoords(world, new Vector3Int(x, 0, z));
                chunkDataPositionsToCreate.Add(chunkPos);
                if (x >= playerPosition.x - world.chunkSize
                    && x <= playerPosition.x + world.chunkSize
                    && z >= playerPosition.z - world.chunkSize
                    && z <= playerPosition.z + world.chunkSize)
                {
                    for (int y = -world.chunkHeight; y >= playerPosition.y - world.chunkHeight * 2; y -= world.chunkHeight)
                    {
                        chunkPos = ChunkPositionFromBlockCoords(world, new Vector3Int(x, y, z));
                        chunkDataPositionsToCreate.Add(chunkPos);
                    }
                }
            }
        }

        return chunkDataPositionsToCreate;
    }

    internal static ChunkRenderer GetChunk(World worldReference, Vector3Int worldPosition)
    {
        if (worldReference.gameData.chunkDictionary.ContainsKey(worldPosition))
            return worldReference.gameData.chunkDictionary[worldPosition];
        return null;
    }

    internal static void GetDurability(Vector3Int pos, BlockType blockType, Dictionary<Vector3Int, int> Durability)
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        for (int i = 0; i < gameManager.blockData.textureDataList.Count; i++)
        {
            if (gameManager.blockData.textureDataList[i].blockType == blockType)
            {
                if (!Durability.ContainsKey(pos))
                {
                    Durability.Add(pos, 0);
                    Durability[pos] = gameManager.blockData.textureDataList[i].durability;
                }

                break;
            }
        }
    }

    internal static void SetDurability(ChunkData chunkData, Vector3Int localPosition, Vector3Int pos, BlockType blockType, Dictionary<Vector3Int, int> Durability)
    {
        int durability;
        GetDurability(pos, blockType, Durability);
        if (Durability.TryGetValue(pos, out durability))
        {

            durability -= 1;
            if (durability <= 0)
            {

                if (
                Chunk.GetBlockFromChunkCoordinates(chunkData, localPosition + new Vector3Int(1, 0, 0)) == BlockType.Water ||
                Chunk.GetBlockFromChunkCoordinates(chunkData, localPosition + new Vector3Int(0, 1, 0)) == BlockType.Water ||
                Chunk.GetBlockFromChunkCoordinates(chunkData, localPosition + new Vector3Int(0, 0, 1)) == BlockType.Water ||
                Chunk.GetBlockFromChunkCoordinates(chunkData, localPosition + new Vector3Int(-1, 0, 0)) == BlockType.Water ||
                Chunk.GetBlockFromChunkCoordinates(chunkData, localPosition + new Vector3Int(0, 0, -1)) == BlockType.Water)
                {
                    Chunk.SetBlock(chunkData, localPosition, BlockType.Water);
                    Durability.Remove(pos);
                }
                else
                {
                    Chunk.SetBlock(chunkData, localPosition, BlockType.Air);
                    Durability.Remove(pos);
                }
            }
            else
            {
                Durability[pos] = durability;
            }
        }
        else Durability.Add(pos, durability);
    }

    internal static void SetNewBlock(World worldReference, Vector3Int pos, BlockType blockType, Dictionary<Vector3Int, int> Durability, bool place)
    {
        ChunkData chunkData = GetChunkData(worldReference, pos);
        if (chunkData != null)
        {
            Vector3Int localPosition = Chunk.GetBlockInChunkCoordinates(chunkData, pos);
            BlockType localType = Chunk.GetBlockFromChunkCoordinates(chunkData, localPosition);


            if (localType.Equals(BlockType.Grass_Dirt) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Grass_Dirt, Durability);
            }
            else if (localType.Equals(BlockType.Dirt) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Dirt, Durability);
            }
            else if (localType.Equals(BlockType.Grass_Stone) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Grass_Stone, Durability);
            }
            else if (localType.Equals(BlockType.Stone) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Stone, Durability);
            }
            else if (localType.Equals(BlockType.Tree_Trunk) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Tree_Trunk, Durability);
            }
            else if (localType.Equals(BlockType.Tree_Leafes_Transparent) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Tree_Leafes_Transparent, Durability);
            }
            else if (localType.Equals(BlockType.Tree_Leafs_Solid) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Tree_Leafs_Solid, Durability);
            }
            else if (localType.Equals(BlockType.Water) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Water, Durability);
            }
            else if (localType.Equals(BlockType.Sand) && place == false)
            {
                SetDurability(chunkData, localPosition, pos, BlockType.Sand, Durability);
            }
            else
            {
                Chunk.SetBlock(chunkData, localPosition, blockType);
            }

        }
    }

    internal static void SetBlock(World worldReference, Vector3Int pos, BlockType blockType)
    {
        ChunkData chunkData = GetChunkData(worldReference, pos);
        if (chunkData != null)
        {

            Vector3Int localPosition = Chunk.GetBlockInChunkCoordinates(chunkData, pos);
            Chunk.SetBlock(chunkData, localPosition, blockType);

        }
    }

    public static ChunkData GetChunkData(World worldReference, Vector3Int worldBlockPosition)
    {
        Vector3Int chunkPosition = ChunkPositionFromBlockCoords(worldReference, worldBlockPosition);

        ChunkData containerChunk = null;

        worldReference.gameData.chunkDataDictionary.TryGetValue(chunkPosition, out containerChunk);

        return containerChunk;
    }

    internal static List<Vector3Int> GetUnnededData(GameData gameData, List<Vector3Int> allChunkDataPositionsNeeded)
    {
        return gameData.chunkDataDictionary.Keys
    .Where(pos => allChunkDataPositionsNeeded.Contains(pos) == false && gameData.chunkDataDictionary[pos].modifiedByThePlayer == false)
    .ToList();

    }

    internal static List<Vector3Int> GetUnnededChunks(GameData gameData, List<Vector3Int> allChunkPositionsNeeded)
    {
        List<Vector3Int> positionToRemove = new List<Vector3Int>();
        foreach (var pos in gameData.chunkDictionary.Keys
            .Where(pos => allChunkPositionsNeeded.Contains(pos) == false))
        {
            if (gameData.chunkDictionary.ContainsKey(pos))
            {
                positionToRemove.Add(pos);

            }
        }

        return positionToRemove;
    }

    internal static List<Vector3Int> SelectPositonsToCreate(GameData gameData, List<Vector3Int> allChunkPositionsNeeded, Vector3Int playerPosition)
    {
        return allChunkPositionsNeeded
            .Where(pos => gameData.chunkDictionary.ContainsKey(pos) == false)
            .OrderBy(pos => Vector3.Distance(playerPosition, pos))
            .ToList();
    }

    internal static List<Vector3Int> SelectDataPositonsToCreate(GameData gameData, List<Vector3Int> allChunkDataPositionsNeeded, Vector3Int playerPosition)
    {
        return allChunkDataPositionsNeeded
            .Where(pos => gameData.chunkDataDictionary.ContainsKey(pos) == false)
            .OrderBy(pos => Vector3.Distance(playerPosition, pos))
            .ToList();
    }
}