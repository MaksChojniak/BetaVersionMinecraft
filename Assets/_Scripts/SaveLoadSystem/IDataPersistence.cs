using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    void LoadPlayerData(PlayerData data);
    void SavePlayerData(PlayerData data);

    void LoadWorldData(WorldData data);
    void SaveWorldData(WorldData data);

    void LoadGameData(GameData data);
    void SaveGameData(GameData data);



}