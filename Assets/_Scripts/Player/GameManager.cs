using Cinemachine;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameManager;
    public GameObject playerPrefab;
    private GameObject player;
    public Vector3Int currentPlayerChunkPosition;
    private Vector3Int currentChunkCenter = Vector3Int.zero;

    public World world;

    public float detectionTime = 1;
    public CinemachineVirtualCamera camera_VM;

    public BlockDataSO blockData;

    PlayerData playerData;

    public Vector3 spawnPos;
    public Vector3 playerPositions;
    public Quaternion playerRotation;

    public Transform chunkStorage;
    public bool newGame = false;
    public bool loadGame = false;

    public bool keyClicked = false;


    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        keyClicked = false;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (keyClicked == false)
            {
                Cursor.lockState = CursorLockMode.None;
                keyClicked = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                keyClicked = false;
            }
        }


        Following();



        if (player != null)
        {
            playerPositions = player.transform.position;
            playerRotation = player.transform.rotation;
        }
    }

    public void Following()
    {
        PlayerData playerData = GameObject.Find("SaveLoadSystem").GetComponent<DataPersistenceManager>().playerData;
        if (loadGame == true)
        {
            gameManager.transform.position = playerData.playerPosition;
            loadGame = false;
        }
        else if (newGame == true)
        {
            this.gameObject.transform.position = new Vector3Int(world.chunkSize / 2, 100, world.chunkSize / 2);
            newGame = false;
        }
        else if (player != null)
        {
            gameManager.transform.position = player.transform.position;
        }
    }

    private void SpawnPlayer()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (player != null)
            return;
        Vector3Int raycastStartposition = new Vector3Int(Mathf.FloorToInt(gameManager.transform.position.x), 100, Mathf.FloorToInt(gameManager.transform.position.z));
        RaycastHit hit;
        if (Physics.Raycast(raycastStartposition, Vector3.down, out hit, 120))
        {
            playerData = GameObject.Find("SaveLoadSystem").GetComponent<DataPersistenceManager>().playerData;
            spawnPos = hit.point + Vector3Int.up;

            if (playerData == null || newGame == true)
            {
                playerData = new PlayerData();
                playerData.spawnPos = spawnPos;
                playerData.playerPosition = spawnPos;
            }
            else if (playerData != null)
            {
            }

            if (playerData.spawnPos != playerData.playerPosition)
            {
                if (playerData != null)
                    player = Instantiate(playerPrefab, playerData.playerPosition, playerData.playerRotation);
            }
            else
            {
                player = Instantiate(playerPrefab, playerData.spawnPos, Quaternion.identity);
            }

            camera_VM.Follow = player.transform.GetChild(0);
            StartCheckingTheMap();

            Toolbar toolbar = GameObject.FindWithTag("Toolbar").GetComponent<Toolbar>();
            toolbar.Create();
        }
    }
    public void IsNewGame()
    {
        newGame = true;
    }

    public void IsLoadGame()
    {
        loadGame = true;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
    public void StartCheckingTheMap()
    {
        SetCurrentChunkCoordinates();
        StopAllCoroutines();
        StartCoroutine(CheckIfShouldLoadNextPosition());
    }

    IEnumerator CheckIfShouldLoadNextPosition()
    {
        yield return new WaitForSeconds(detectionTime);
        if (
            Mathf.Abs(currentChunkCenter.x - player.transform.position.x) > world.chunkSize ||
            Mathf.Abs(currentChunkCenter.z - player.transform.position.z) > world.chunkSize ||
            (Mathf.Abs(currentPlayerChunkPosition.y - player.transform.position.y) > world.chunkHeight)
            )
        {
            world.LoadAdditionalChunksRequest(player);

        }
        else
        {
            StartCoroutine(CheckIfShouldLoadNextPosition());
        }
    }


    private void SetCurrentChunkCoordinates()
    {
        currentPlayerChunkPosition = WorldDataHelper.ChunkPositionFromBlockCoords(world, Vector3Int.RoundToInt(player.transform.position));
        currentChunkCenter.x = currentPlayerChunkPosition.x + world.chunkSize / 2;
        currentChunkCenter.z = currentPlayerChunkPosition.z + world.chunkSize / 2;
    }

}
