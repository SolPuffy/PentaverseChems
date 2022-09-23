using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommands : MonoBehaviour
{
    public static ServerCommands instance;
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(instance);
        }
        else
        {
            instance = this;
        }
    }
    //[ClientRpc]
    public void SpawnWordForAll(string newWord,int intration)
    {
        FallingWords.instance.SpawnWord(newWord,intration);
    }
    //ON CONNECT & [Command]
    public void AddNewPlayer()
    {
        FallingWords.instance.PlayersList.Add(new Players());
        int newIndexer = FallingWords.instance.PlayersList.Count - 1;

        FallingWords.instance.PlayersList[newIndexer].UniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
        FallingWords.instance.PlayersList[newIndexer].Score = 0;
    }
}
