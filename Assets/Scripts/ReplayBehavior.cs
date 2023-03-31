using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using Mirror;

public class ReplayBehavior : MonoBehaviour
{
    public ServerData savefileSnapshot;
    public TextMeshProUGUI MissingRefferenceToTimerText;
    [Range(0f, 10f)]
    public int ReplayActionDelay = 4;
    public void OnTextChangePreventSymbolsBeforeID(TMP_InputField input)
    {
        if (Regex.IsMatch(input.text[0].ToString(), "-"))
        {
            input.text = "error: symbols denied";
            input.DeactivateInputField();
            return;
        }
    }
    public async void OnTextSubmitReceiveNewReplayID(TMP_InputField input)
    {
        input.DeactivateInputField();
        if (input.text.Length > 9)
        {
            return;
        }
        if (input.text.Length < 9)
        {
            input.text = "error: short ID";
            return;
        }

        await ServerLogging.RequestDataFromServer(input.text);
        input.gameObject.SetActive(false);

        savefileSnapshot = await ServerLogging.RequestInstanceData();
        Debug.Log("Begin Save Playback.");
        await Playback();
        Debug.Log("Playback Finished.");
    }

    public async Task Playback()
    {
        //Setup Game!
        REPLAYFallingWords.instance.AdaptiveWordsVolume = savefileSnapshot.WordLists.AvailableWordsThisGame;
        for (int i = 0; i < savefileSnapshot.PlayerCount; i++)
        {
            AddNewPlayer();
        }
        REPLAYFallingWords.instance.StartGame(savefileSnapshot.GameTime);
        //Play Game!
        foreach (gameActions action in savefileSnapshot.ActionsPerformed)
        {
            if(ReplayActionDelay <= 0)
            {
                await Task.Yield();
            }

            switch(action.actionType)
            {
                case "LetterSend":
                    {
                        SendKeyToServer(action.actionContent[0], action.identifier);
                        break;
                    }
                case "WordSend":
                    {
                        SendWordToServer(action.actionContent,action.identifier);
                        break;
                    }
                default:break;
            }
            await Task.Delay((int)((ReplayActionDelay * 0.25f) * 1000));
        }

        Debug.Log("Reached The End of the Replay");

    }
    //[Command]
    public void AddNewPlayer()
    {
        RePlayers nStruct = new RePlayers();
        nStruct.UniqueIdentifier = savefileSnapshot.WinningPlacements[REPLAYFallingWords.instance.PlayersList.Count].UniqueIdOfPlayer;
        nStruct.Score = 0;
        nStruct.playerUI = REPLAYFallingWords.instance.PlayerUI[REPLAYFallingWords.instance.PlayersList.Count];
        REPLAYFallingWords.instance.PlayerUI[REPLAYFallingWords.instance.PlayersList.Count].PlayerPortraitImage.sprite = REPLAYFallingWords.instance.PlayerPortraits[savefileSnapshot.WinningPlacements[REPLAYFallingWords.instance.PlayersList.Count].PortraitIndex];
        REPLAYFallingWords.instance.PlayersList.Add(nStruct);


    }
    //[ClientRpc]
    public void CleanPlayersListIndex(int index)
    {
        GameObject preparedforDestroy = REPLAYFallingWords.instance.WordsOnScreen[index].Structure.gameObject;
        REPLAYFallingWords.instance.WordsOnScreen.RemoveAt(index);
        Destroy(preparedforDestroy);
    }
    //[ClientRpc]
    public void CleanPlayersLists()
    {
        foreach (WordAdditionalStructure word in REPLAYFallingWords.instance.WordsOnScreen)
        {
            Destroy(word.Structure.gameObject);
        }
        REPLAYFallingWords.instance.WordsOnScreen.Clear();
    }
    //[ClientRpc]
    public void UpdateGameTimers(int TimeLeft)
{
    string TimeToTextFormat = "";
    int minutes = (TimeLeft / 30) / 60;
    int seconds = (TimeLeft / 30) % 60;
    if (seconds >= 10)
    {
        TimeToTextFormat = $"{minutes}:{seconds}";
    }
    else
    {
        TimeToTextFormat = $"{minutes}:0{seconds}";
    }

    MissingRefferenceToTimerText.text = TimeToTextFormat;
    //SEND TIME TO TEXT BOI REFERENCE

}

//[ClientRpc]
public void ReturnSetPlayersPortraits(int indexer)
{
    REPLAYFallingWords.instance.PlayerUI[indexer].gameObject.SetActive(true);

    REPLAYFallingWords.instance.PlayerUI[indexer].PlayerPortraitImage.sprite = REPLAYFallingWords.instance.PlayerPortraits[indexer];
}


//[Command]
public void SendKeyToServer(char key,string identifier)
{
    REPLAYFallingWords.instance.ReceiveLetterFromPlayer(key, identifier);
}
//Command]
public void SendWordToServer(string word,string identifier)
{
    REPLAYFallingWords.instance.ReceiveWordFromPlayer(word, identifier);
}
//[ClientRpc]
public void ReturnKeyInfoToPlayers(int WordIndex, int LetterOnWordIndex)
{
    REPLAYFallingWords.instance.WordsOnScreen[WordIndex].Structure.LetterCovers[LetterOnWordIndex].SetActive(false);
    //Debug.Log($"Uncovering {WordIndex},{LetterOnWordIndex}");
}
//[ClientRpc]
public void ReturnWordCoversOnCrumble(int iter, int targetedindex)
{
    REPLAYFallingWords.instance.WordsOnScreen[iter].Structure.LetterCovers[targetedindex].gameObject.SetActive(false);
}
//[ClientRpc]
public void ReturnWordInfoToPlayers(int indexHit)
{
    for (int z = 0; z < REPLAYFallingWords.instance.WordsOnScreen[indexHit].HeldWord.Length; z++)
    {
        REPLAYFallingWords.instance.WordsOnScreen[indexHit].Structure.LetterCovers[z].gameObject.SetActive(false);
        REPLAYFallingWords.instance.WordsOnScreen[indexHit].Structure.Letters[z].color = Color.gray;
    }
}
//[TargetRpc]
public void ReturnAttemptedWordLocally(string attemptedWord)
{
    REPLAYFallingWords.instance.AttemptsReturnUI.SpawnNewText(attemptedWord, true);
}
//[TargetRpc]
public void ReturnAttemptedLetterLocally(char attemptedLetter)
{
    REPLAYFallingWords.instance.AttemptsReturnUI.SpawnNewText(attemptedLetter.ToString(), false);
}
//[ClientRpc]
public void ReturnHideWordAtTarget(int indexHit)
{
    for (int z = 0; z < REPLAYFallingWords.instance.WordsOnScreen[indexHit].HeldWord.Length; z++)
    {
        REPLAYFallingWords.instance.WordsOnScreen[indexHit].Structure.LetterCovers[z].gameObject.SetActive(true);
        REPLAYFallingWords.instance.WordsOnScreen[indexHit].Structure.Letters[z].color = Color.white;
    }
}
//[ClientRpc]
public void UpdatePointsBoard(int index, int UpdatedScore)
{
    REPLAYFallingWords.instance.PlayerUI[index].AccessScoreText.text = UpdatedScore.ToString();
}

}
     
     
