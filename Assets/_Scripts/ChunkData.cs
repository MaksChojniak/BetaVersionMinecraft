using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;


[Serializable]
public class ChunkData
{

    public BlockType[] blocks;
    public int chunkSize = 16;
    public int chunkHeight = 100;
    [JsonIgnore]
    public World worldReference;
    public Vector3Int worldPosition;
    public bool modifiedByThePlayer = false;
    public TreeData treeData;

    public ChunkData(int chunkSize, int chunkHeight, World world, Vector3Int worldPosition)
    {
        this.chunkHeight = chunkHeight;
        this.chunkSize = chunkSize;
        this.worldReference = world;
        this.worldPosition = worldPosition;
        blocks = new BlockType[chunkSize * chunkHeight * chunkSize];
    }

}