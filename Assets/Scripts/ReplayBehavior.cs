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
    public Transform[] MissingRefferenceToReturnText = new Transform[5];
    public static ReplayBehavior instance;
    [Range(0f, 10f)]
    public int ReplayActionDelay = 4;
    private void Awake()
    {
        instance = this;   
    }
    public void OnTextChangePreventSymbolsBeforeID(TMP_InputField input)
    {
        if (input.text.Length > 0 && Regex.IsMatch(input.text[0].ToString(), "-") )
        {
            input.text = "error: symbols denied";
            input.DeactivateInputField();
            return;
        }
    }   

    IEnumerator Playback()
    {
        //Setup Game!
        REPLAYFallingWords.instance.AdaptiveWordsVolume = savefileSnapshot.WordLists.AvailableWordsThisGame;
        for (int i = 0; i < savefileSnapshot.PlayerCount; i++)
        {
            AddNewPlayer();
        }
        for (int i= 0; i < 5; i++)
        {
            REPLAYFallingWords.instance.SpawnWord(savefileSnapshot.WordLists.UsedWordsThisGame[i].UsedWord, i);
        }
       yield return new WaitForSeconds(1.5f);
        //Play Game!
        foreach (gameActions action in savefileSnapshot.ActionsPerformed)
        {

            yield return new WaitForSeconds(ReplayActionDelay * 0.25f);
            switch (action.actionType)
            {
                case "LetterSend":
                    {
                        REPLAYFallingWords.instance.getInput(action.actionType, action.actionContent, action.identifier);
                        foreach (RePlayers player in REPLAYFallingWords.instance.PlayersList)
                        {
                            if (player.UniqueIdentifier == action.identifier)
                            {
                                REPLAYFallingWords.instance.AttemptsReturnUI.SpawnNewText(action.actionContent, false, true, player.personalReturnTextLoc);
                            }
                        }
                        break;
                    }
                case "WordSend":
                    {
                        REPLAYFallingWords.instance.getInput(action.actionType, action.actionContent.ToLower(), action.identifier);
                        foreach (RePlayers player in REPLAYFallingWords.instance.PlayersList)
                        {
                            if (player.UniqueIdentifier == action.identifier)
                            {
                                REPLAYFallingWords.instance.AttemptsReturnUI.SpawnNewText(action.actionContent,true,true,player.personalReturnTextLoc);
                            }
                        }
                        break;
                    }
                default:break;
            }            
        }
    }
    //[Command]
    public void AddNewPlayer()
    {
        RePlayers nStruct = new RePlayers();
        nStruct.UniqueIdentifier = savefileSnapshot.WinningPlacements[REPLAYFallingWords.instance.PlayersList.Count].UniqueIdOfPlayer;
        nStruct.Score = 0;
        nStruct.playerUI = REPLAYFallingWords.instance.PlayerUI[REPLAYFallingWords.instance.PlayersList.Count];
        nStruct.personalReturnTextLoc = MissingRefferenceToReturnText[REPLAYFallingWords.instance.PlayersList.Count];

        nStruct.playerUI.gameObject.SetActive(true);

        REPLAYFallingWords.instance.PlayersList.Add(nStruct);
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
//[TargetRpc]
public void ReturnAttemptedWordLocally(string attemptedWord)
{
    
}
//[TargetRpc]
public void ReturnAttemptedLetterLocally(char attemptedLetter)
{
    REPLAYFallingWords.instance.AttemptsReturnUI.SpawnNewText(attemptedLetter.ToString(), false);
}
//[ClientRpc]
public void UpdatePointsBoard(int index, int UpdatedScore)
{
    REPLAYFallingWords.instance.PlayerUI[index].AccessScoreText.text = UpdatedScore.ToString();
}

    public async void ConfirmSelectReplay(string fileName)
    {
        await ServerLogging.RequestDataFromServer(fileName);
        //input.gameObject.SetActive(false);

        savefileSnapshot = await ServerLogging.RequestInstanceData();
        Debug.Log("Begin Save Playback.");
        StartCoroutine( Playback());
        Debug.Log("Playback Finished.");
    }

}
     
     
