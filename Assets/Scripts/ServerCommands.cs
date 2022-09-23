using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommands : MonoBehaviour
{
    public static ServerCommands instance;
    private string LocalUniqueIdentifier;
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
    private void Start()
    {
        LocalUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
    }

    //[ClientRpc]
    public void SpawnWordForAll(string newWord,int intration)
    {
        FallingWords.instance.SpawnWord(newWord,intration);
    }
    //ON CONNECT & [Command]
    public void AddNewPlayer()
    {
        Players nStruct = new Players();
        nStruct.UniqueIdentifier = LocalUniqueIdentifier;
        nStruct.Score = 0;
        FallingWords.instance.PlayersList.Add(nStruct);
    }
    //ON DISCONNECT & [Command]
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
    //[Command]
    public void SendKeyToServer(Event keyevent)
    {
        FallingWords.instance.ReceiveLetterFromPlayer(keyevent, LocalUniqueIdentifier);
    }
    //[Command]
    public void SendWordToServer(string word)
    {
        FallingWords.instance.ReceiveWordFromPlayer(word, LocalUniqueIdentifier);
    }
    //[ClientRpc]
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
    //[ClientRpc]
    public void ReturnWordInfoToPlayers(int indexHit)
    {
        for (int z = 0; z < FallingWords.instance.WordsOnScreen[indexHit].Word.Length; z++)
        {
            FallingWords.instance.WordsOnScreen[indexHit].LetterCovers[z].gameObject.SetActive(false);
            FallingWords.instance.WordsOnScreen[indexHit].Letters[z].color = Color.gray;
        }
    }
    //[ClientRpc]
    public void UpdatePointsBoard()
    {

    }
}
