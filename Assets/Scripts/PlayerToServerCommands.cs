using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerToServerCommands : NetworkBehaviour
{
    public static PlayerToServerCommands localPlayer;
    [SyncVar] public string LocalUniqueIdentifier;
    [SyncVar] public bool HasEntered = false;
    private void Start()
    {        
        if (isLocalPlayer)
        {
            localPlayer = this;            
            if (!FallingWords.instance.GameStarted)
            {              
                AddNewPlayer(SystemInfo.deviceUniqueIdentifier);               
            }
            else
            {
                NetworkManager.singleton.StopClient();
            }
        }
        else
        {
            Debug.Log("Nu sunt local bre");
        }
    }    
    public override void OnStopServer()
    {
        Debug.Log($"Client {name}, Identifier {LocalUniqueIdentifier} Stopped on Server");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if ((players.Length - 1) == 0)
        {
            Debug.Log($"No Players Left. Resetting ...");
            FallingWords.instance.ResetScene();
            return;
        }

        if (!HasEntered)
        {
            Debug.Log("Player tried to join with no space left");
        }
        else
        {
            removePlayer();
            /*
            if (HitSlapRazboi.instance.InititalSetupDone)
            {
               
            }

            else
            {
                HitSlapRazboi.instance.RemovePlayerBeforeGame(playerIndex);
            }
            */
        }
    }

    public override void OnStartServer()
    {
        Debug.Log($"Client {name} connected on Server");
    }

    [TargetRpc]
    public void DC()
    {
        NetworkManager.singleton.StopClient();
    }   

    [ClientRpc]
    public void SpawnWordForAll(string newWord,int intration)
    {
        FallingWords.instance.SpawnWord(newWord,intration);
    }
    [Command]
    public void StartGame()
    {
        FallingWords.instance.StartGame();
    }
    [Command]
    public void AddNewPlayer(string identifier)
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        if (Players.Length > 5)
        {
            DC();
        }
        else
        {            
            HasEntered = true;
            Players nStruct = new Players();
            nStruct.playerScript = this;

            for (int i = 0; i < FallingWords.instance.PlayersList.Count; i++)
            {
                if (FallingWords.instance.PlayersList[i].UniqueIdentifier == identifier)
                {
                    identifier += "a";
                }
            }

            LocalUniqueIdentifier = nStruct.UniqueIdentifier = identifier;
            nStruct.Score = 0;
            FallingWords.instance.PlayersList.Add(nStruct);
            
        }
    }
   
    [Command]
    public void removePlayer()
    {
        for(int i=0;i<FallingWords.instance.PlayersList.Count;i++)
        {
            if (FallingWords.instance.PlayersList[i].UniqueIdentifier == LocalUniqueIdentifier)
            {
                FallingWords.instance.PlayersList.RemoveAt(i);
            }
        }
    }    
    [Command]
    public void SendKeyToServer(Event keyevent)
    {
        FallingWords.instance.ReceiveLetterFromPlayer(keyevent, LocalUniqueIdentifier);
    }
    [Command]
    public void SendWordToServer(string word)
    {
        FallingWords.instance.ReceiveWordFromPlayer(word, LocalUniqueIdentifier);
    }
    [ClientRpc]
    public void ReturnKeyInfoToPlayers(char keyInfo)
    {
        for (int z = 0; z < 2; z++)
        {
            for (int i = 0; i < FallingWords.instance.WordsOnScreen.Count; i++)
            {
                for (int y = 0; y < FallingWords.instance.WordsOnScreen[i].Word.Length; y++)
                {
                    if (FallingWords.instance.WordsOnScreen[i].Word[y] == keyInfo)
                    {
                        FallingWords.instance.WordsOnScreen[i].LetterCovers[y].gameObject.SetActive(false);
                    }
                }
            }
        }
    }
    [ClientRpc]
    public void ReturnWordInfoToPlayers(int indexHit)
    {
        for (int z = 0; z < FallingWords.instance.WordsOnScreen[indexHit].Word.Length; z++)
        {
            FallingWords.instance.WordsOnScreen[indexHit].LetterCovers[z].gameObject.SetActive(false);
            FallingWords.instance.WordsOnScreen[indexHit].Letters[z].color = Color.gray;
        }
    }
    [ClientRpc]
    public void CleanPlayersLists()
    {
        foreach (WordToEntityStructure word in FallingWords.instance.WordsOnScreen)
        {
            Destroy(word.gameObject);
        }
        FallingWords.instance.WordsOnScreen.Clear();
    }
    [ClientRpc]
    public void CleanPlayersListIndex(int index)
    {
        GameObject preparedforDestroy = FallingWords.instance.WordsOnScreen[index].gameObject;
        FallingWords.instance.WordsOnScreen.RemoveAt(index);
        Destroy(preparedforDestroy);
    }
    [ClientRpc]
    public void UpdatePointsBoard()
    {

    }
}
