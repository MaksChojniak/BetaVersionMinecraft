using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileNamePlayerData;
    [SerializeField] private string fileNameWorldData;
    [SerializeField] private string fileNameGameData;
    [SerializeField] private bool useEncryption;

    public PlayerData playerData;
    public WorldData worldData;
    public GameData gameData;

    public Dictionary<Vector3Int, ChunkData> loadedChunkDataDictionary = new Dictionary<Vector3Int, ChunkData>();



    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;
    }

    private void Start()
    {

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileNamePlayerData, fileNameWorldData, fileNameGameData, useEncryption);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }
    public void NewGame()
    {
        print("new game");
        PlayerData player = new PlayerData();
        WorldData block = new WorldData();
        GameData game = new GameData();


        this.playerData = new PlayerData();
        this.worldData = new WorldData();
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        print("load game");
        this.playerData = dataHandler.LoadPlayerData();
        this.worldData = dataHandler.LoadWorldData();
        this.gameData = dataHandler.LoadGameData();

        if (this.playerData == null || this.worldData == null || this.gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadPlayerData(playerData);
            dataPersistenceObj.LoadWorldData(worldData);
            dataPersistenceObj.LoadGameData(gameData);
        }

        Dictionary<string, ChunkData> newLoadedChunkDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, ChunkData>>(this.gameData.jsonChunkDataDictionary);
        string[] stringKeys = newLoadedChunkDataDictionary.Keys.ToArray();
        Vector3Int[] keys = new Vector3Int[stringKeys.Length];
        ChunkData[] values = newLoadedChunkDataDictionary.Values.ToArray();

        for (int i = 0; i < stringKeys.Length; i++)
        {
            keys[i] = StringToVector3(stringKeys[i]);
            loadedChunkDataDictionary.Add(keys[i], values[i]);
            loadedChunkDataDictionary[keys[i]].worldReference = GameObject.Find("World").GetComponent<World>();
        }


        GameObject.Find("World").GetComponent<World>().GenerateWorld();
    }

    public void SaveGame()
    {
        World world = GameObject.Find("World").GetComponent<World>();
        this.gameData = world.gameData;
        this.gameData.jsonChunkDataDictionary = JsonConvert.SerializeObject(this.gameData.chunkDataDictionary, Formatting.Indented);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = player.transform.position;
        Quaternion playerRot = player.transform.rotation;
        this.playerData.spawnPos = GameObject.Find("GameManager").GetComponent<GameManager>().spawnPos;
        this.playerData.playerPosition = playerPos;
        this.playerData.playerRotation = playerRot;

        this.worldData.mapSizeInChunk = world.mapSizeInChunks;
        this.worldData.chunkSize = world.chunkSize;
        this.worldData.chunkHeight = world.chunkHeight;
        this.worldData.chunkDrawRange = world.chunkDrawingRange;
        this.worldData.mapSeedOffset =  world.mapSeedOffset;




        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SavePlayerData(playerData);
            dataPersistenceObj.SaveWorldData(worldData);
            dataPersistenceObj.SaveGameData(gameData);
        }


        dataHandler.SavePlayerData(playerData);
        dataHandler.SaveWorldData(worldData);
        dataHandler.SaveGameData(gameData);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
        print("Saved game");
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }



    public static Vector3Int StringToVector3(string sVector)
    {
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        string[] sArray = sVector.Split(',');

        Vector3Int result = new Vector3Int(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]),
            int.Parse(sArray[2]));

        return result;
    }
}