using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
    public int mapSizeInChunks;
    public int chunkSize, chunkHeight;
    public int chunkDrawingRange;
    public Vector2Int mapSeedOffset;

    public bool newGame = false;
    public bool IsWorldCreated { get; private set; }

    public GameObject chunkPrefab;
    public WorldRenderer worldRenderer;
    public GameData gameData;
    public TerrainGenerator terrainGenerator;

    CancellationTokenSource taskTokenSource = new CancellationTokenSource();
    public UnityEvent OnWorldCreated, OnNewChunksGenerated;




    void Awake()
    {
        mapSizeInChunks = 0;
        chunkSize = 0;
        chunkHeight = 0;
        chunkDrawingRange = 0;
        mapSeedOffset = Vector2Int.zero;
    }

    public void IsNewGame()
    {
        newGame = true;
    }

    public async void GenerateWorld()
    {
        DataPersistenceManager slSystem = GameObject.Find("SaveLoadSystem").GetComponent<DataPersistenceManager>();
        if (newGame == true)
        {
            this.gameData = new GameData
            {
                chunkHeight = this.chunkHeight,
                chunkSize = this.chunkSize,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };

            await GenerateWorld(Vector3Int.zero);
        }
        else
        {
            Dictionary<Vector3Int, ChunkData> loadedChunkDataDictionary = slSystem.loadedChunkDataDictionary;
            this.gameData = new GameData
            {
                chunkHeight = this.chunkHeight,
                chunkSize = this.chunkSize,
                chunkDataDictionary = loadedChunkDataDictionary,
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };
            mapSizeInChunks = slSystem.worldData.mapSizeInChunk;
            chunkSize = slSystem.worldData.chunkSize;
            chunkHeight = slSystem.worldData.chunkHeight;
            chunkDrawingRange = slSystem.worldData.chunkDrawRange;
            mapSeedOffset = slSystem.worldData.mapSeedOffset;
;
            Vector3Int intPlayerPosition = new Vector3Int(Mathf.FloorToInt(slSystem.playerData.playerPosition.x), Mathf.FloorToInt(slSystem.playerData.playerPosition.y), Mathf.FloorToInt(slSystem.playerData.playerPosition.z));
            await GenerateWorld(intPlayerPosition);
        }
        Cursor.lockState = CursorLockMode.Locked;

    }

    private async Task GenerateWorld(Vector3Int position)
    {
        terrainGenerator.GenerateBiomePoints(position, chunkDrawingRange, chunkSize, mapSeedOffset);

        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsThatPlayerSees(position), taskTokenSource.Token);

        foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
        {
            WorldDataHelper.RemoveChunk(this, pos);
        }

        foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
        {
            WorldDataHelper.RemoveChunkData(this, pos);
        }


        ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;

        try
        {
            dataDictionary = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }


        foreach (var calculatedData in dataDictionary)
        {
            this.gameData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);
        }
        foreach (var chunkData in this.gameData.chunkDataDictionary.Values)
        {
            AddTreeLeafs(chunkData);
        }

        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        List<ChunkData> dataToRender = this.gameData.chunkDataDictionary
            .Where(keyvaluepair => worldGenerationData.chunkPositionsToCreate.Contains(keyvaluepair.Key))
            .Select(keyvalpair => keyvalpair.Value)
            .ToList();

        try
        {
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));
    }

    private void AddTreeLeafs(ChunkData chunkData)
    {
        foreach (var treeLeafes in chunkData.treeData.treeLeafesSolid)
        {
            Chunk.SetBlock(chunkData, treeLeafes, BlockType.Tree_Leafs_Solid);
        }
    }

    private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
        return Task.Run(() =>
        {

            foreach (ChunkData data in dataToRender)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                MeshData meshData = Chunk.GetChunkMeshData(data);
                dictionary.TryAdd(data.worldPosition, meshData);
            }

            return dictionary;
        }, taskTokenSource.Token
        );
    }

    private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
    {
        ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();
        return Task.Run(() =>
        {
            foreach (Vector3Int pos in chunkDataPositionsToCreate)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);

                dictionary.TryAdd(pos, newData);

            }
            return dictionary;
        },
        taskTokenSource.Token
        );

    }

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary)
    {
        foreach (var item in meshDataDictionary)
        {
            CreateChunk(this.gameData, item.Key, item.Value);
            yield return new WaitForEndOfFrame();
        }
        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }

    private void CreateChunk(GameData gameData
        , Vector3Int position, MeshData meshData)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(gameData, position, meshData);
        gameData.chunkDictionary.Add(position, chunkRenderer);
    }

    private Dictionary<Vector3Int, int> Durability = new Dictionary<Vector3Int, int>();
    internal bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return false;

        Vector3Int pos = GetBlockPos(hit);

        WorldDataHelper.SetNewBlock(chunk.ChunkData.worldReference, pos, blockType, Durability, false);

        chunk.ModifiedByThePlayer = true;

        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {

                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                if (chunkToUpdate != null)
                    chunkToUpdate.UpdateChunk();
            }

        }
        chunk.UpdateChunk();
        return true;
    }

    internal bool PlaceBlock(RaycastHit hit, BlockType blockType)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
        {
            Debug.Log(chunk);
            return false;
        }
        Vector3Int pos = GetBlockPos(hit);
        pos = PlaceBlockPosition(pos, hit);

        WorldDataHelper.SetNewBlock(chunk.ChunkData.worldReference, pos, blockType, Durability, true);

        chunk.ModifiedByThePlayer = true;

        chunk.UpdateChunk();
        return true;
    }

    private Vector3Int GetBlockPos(RaycastHit hit)
    {
        Vector3 pos = new Vector3(
             GetBlockPositionIn(hit.point.x, hit.normal.x),
             GetBlockPositionIn(hit.point.y, hit.normal.y),
             GetBlockPositionIn(hit.point.z, hit.normal.z)
             );

        return Vector3Int.RoundToInt(pos);
    }

    private Vector3Int PlaceBlockPosition(Vector3Int hitPosition, RaycastHit hit)
    {
        Vector3 position = new Vector3(
            hitPosition.x + hit.normal.x,
            hitPosition.y + hit.normal.y,
            hitPosition.z + hit.normal.z
            );
        return Vector3Int.RoundToInt(position);
    }

    private float GetBlockPositionIn(float pos, float normal)
    {
        if (Mathf.Abs(pos % 1) == 0.5f)
        {
            pos -= (normal / 2);
        }

        return (float)pos;
    }

    private WorldGenerationData GetPositionsThatPlayerSees(Vector3Int playerPosition)
    {
        List<Vector3Int> allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, playerPosition);

        List<Vector3Int> allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundPlayer(this, playerPosition);

        List<Vector3Int> chunkPositionsToCreate = WorldDataHelper.SelectPositonsToCreate(this.gameData, allChunkPositionsNeeded, playerPosition);
        List<Vector3Int> chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositonsToCreate(this.gameData, allChunkDataPositionsNeeded, playerPosition);

        List<Vector3Int> chunkPositionsToRemove = WorldDataHelper.GetUnnededChunks(this.gameData, allChunkPositionsNeeded);
        List<Vector3Int> chunkDataToRemove = WorldDataHelper.GetUnnededData(this.gameData, allChunkDataPositionsNeeded);

        WorldGenerationData data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToRemove = chunkPositionsToRemove,
            chunkDataToRemove = chunkDataToRemove,
            chunkPositionsToUpdate = new List<Vector3Int>()
        };
        return data;

    }

    internal async void LoadAdditionalChunksRequest(GameObject player)
    {
        Debug.Log("Load more chunks");
        await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
        OnNewChunksGenerated?.Invoke();
    }

    internal BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
    {
        Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
        ChunkData containerChunk = null;

        gameData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk == null)
            return BlockType.Nothing;
        Vector3Int blockInCHunkCoordinates = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
        return Chunk.GetBlockFromChunkCoordinates(containerChunk, blockInCHunkCoordinates);
    }

    public void OnDisable()
    {
        taskTokenSource.Cancel();
    }

}