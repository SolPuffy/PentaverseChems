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
    public bool AllowInput = false;
    private void Start()
    {        
        if (isLocalPlayer)
        {
            localPlayer = this;            
            if (!FallingWords.instance.GameStarted)
            {
                Debug.Log(SystemInfo.deviceUniqueIdentifier);
                AddNewPlayer();               
            }
            else
            {
                Debug.Log("Game has already started, disconnect");
                DC();
            }
        }
        else
        {
            //Debug.Log("Nu sunt local bre");
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
            //Debug.Log("Player tried to join with no space left");
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
        //Debug.Log($"Client {name} connected on Server");
    }

    [TargetRpc]
    public void DC()
    {
        NetworkManager.singleton.StopClient();
    }   

    [ClientRpc]
    public void SpawnWordForAll(string newWord,int intration, bool targeted, bool isLocal)
    {
        FallingWords.instance.SpawnWord(newWord,intration,targeted, isLocal);
    }
    [Command]
    public void StartGame()
    {
        FallingWords.instance.StartGame();
        KillStart();
    }

    [ClientRpc]
    public void KillStart()
    {
        LocalCommands.KillStartt();
    }
    [Command]
    public void AddNewPlayer()
    {
        string identifier = Guid.NewGuid().ToString();
        //Debug.Log($"Unique {identifier} trying to join");
        //Deny player if playercount >= 5
        if (FallingWords.instance.PlayersList.Count >= 5)
        {
            Debug.Log("Too many players, disconnect");
            DC();
            return;
        }
        //Deny Player if ID is already present
        foreach (Players player in FallingWords.instance.PlayersList)
        {
            //Debug.Log($"Comparing {player.UniqueIdentifier} to {identifier}");
            if (player.UniqueIdentifier == identifier)
            {
                Debug.Log("ID already exists, disconnect");
                DC();
                return;
            }
        }

        HasEntered = true;
        Players nStruct = new Players();
        nStruct.playerScript = this;
        LocalUniqueIdentifier = nStruct.UniqueIdentifier = identifier;
        nStruct.Score = 0;
        nStruct.playerUI = FallingWords.instance.PlayerUI[FallingWords.instance.PlayersList.Count];
        FallingWords.instance.PlayersList.Add(nStruct);
    }
    [Command]
    public void RefreshCooldown(PlayerToServerCommands local)
    {
        local.UpdateCooldownTimer(FallingWords.instance.CooldownBetweenWordInputs);
    }
    [TargetRpc]
    public void UpdateCooldownTimer(int newCooldown)
    {
        //Debug.Log("Cooldown Set To " + newCooldown);
        FallingWords.instance.InputsManagement.localInputTargetCooldown = newCooldown;
    }
    [TargetRpc]
    public void ReturnServerPlayerSetting(int newCooldown,int indexer, int indexOfPortraitImage)
    {
        FallingWords.instance.PlayerUI[indexer].AccessSlotIndicatorImage.color = Color.blue;
        FallingWords.instance.PlayerUI[indexer].PlayerPortraitImage.sprite = FallingWords.instance.PlayerPortraits[indexOfPortraitImage];
        FallingWords.instance.InputsManagement.localInputTargetCooldown = newCooldown;
        AllowInput = true;
    }
    [ClientRpc]
    public void ReturnSetPlayersPortraits(int indexer)
    {
        FallingWords.instance.PlayerUI[indexer].gameObject.SetActive(true);
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
    public void SendKeyToServer(char key)
    {
        FallingWords.instance.ReceiveLetterFromPlayer(key, LocalUniqueIdentifier);
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
                for (int y = 0; y < FallingWords.instance.WordsOnScreen[i].HeldWord.Length; y++)
                {
                    if (FallingWords.instance.WordsOnScreen[i].HeldWord[y] == keyInfo)
                    {
                        FallingWords.instance.WordsOnScreen[i].Structure.LetterCovers[y].gameObject.SetActive(false);
                    }
                }
            }
        }
    }
    [ClientRpc]
    public void ReturnWordCoversOnCrumble(int iter,int targetedindex)
    {
        FallingWords.instance.WordsOnScreen[iter].Structure.LetterCovers[targetedindex].gameObject.SetActive(false);
    }
    [ClientRpc]
    public void ReturnWordInfoToPlayers(int indexHit)
    {
        for (int z = 0; z < FallingWords.instance.WordsOnScreen[indexHit].HeldWord.Length; z++)
        {
            FallingWords.instance.WordsOnScreen[indexHit].Structure.LetterCovers[z].gameObject.SetActive(false);
            FallingWords.instance.WordsOnScreen[indexHit].Structure.Letters[z].color = Color.gray;
        }
    }
    [ClientRpc]
    public void ReturnAttemptedWordGlobally(string attemptedWord)
    {
        FallingWords.instance.AttemptsReturnUI.addAdditionalGlobalWord(attemptedWord);
    }
    [TargetRpc]
    public void ReturnAttemptedWordLocally(string attemptedWord)
    {
        FallingWords.instance.AttemptsReturnUI.addAdditionalLocalWord(attemptedWord);
    }
    [ClientRpc]
    public void ReturnAttemptedLetterGlobally(char attemptedLetter)
    {
        FallingWords.instance.AttemptsReturnUI.addAdditionalGlobalLetter(attemptedLetter);
    }
    [TargetRpc]
    public void ReturnAttemptedLetterLocally(char attemptedLetter)
    {
        FallingWords.instance.AttemptsReturnUI.addAdditionalLocalLetter(attemptedLetter);
    }
    [ClientRpc]
    public void ReturnHideWordAtTarget(int indexHit)
    {
        for (int z = 0; z < FallingWords.instance.WordsOnScreen[indexHit].HeldWord.Length; z++)
        {
            FallingWords.instance.WordsOnScreen[indexHit].Structure.LetterCovers[z].gameObject.SetActive(true);
            FallingWords.instance.WordsOnScreen[indexHit].Structure.Letters[z].color = Color.white;
        }
    }
    [ClientRpc]
    public void CleanPlayersLists()
    {
        foreach (WordAdditionalStructure word in FallingWords.instance.WordsOnScreen)
        {
            Destroy(word.Structure.gameObject);
        }
        FallingWords.instance.WordsOnScreen.Clear();
    }
    [ClientRpc]
    public void CleanPlayersListIndex(int index)
    {
        GameObject preparedforDestroy = FallingWords.instance.WordsOnScreen[index].Structure.gameObject;
        FallingWords.instance.WordsOnScreen.RemoveAt(index);
        Destroy(preparedforDestroy);
    }
    [ClientRpc]
    public void UpdatePointsBoard(int index,int UpdatedScore)
    {
        FallingWords.instance.PlayerUI[index].AccessScoreText.text = UpdatedScore.ToString();
    }

    //BACKUP
    [Command]
    public void RequestManualBackup()
    {        
        ServerLogging.RequestLogBackup();
    }
}
