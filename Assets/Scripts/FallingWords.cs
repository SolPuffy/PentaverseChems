using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Players
{
    //whatever other things
    public PlayerToServerCommands playerScript;
    public PlayerSlotAccess playerUI;
    public string UniqueIdentifier;
    public int Score;
}
public class LetterToWordStructure
{
    public char Letter;
    public int IndexOfWord;
}
public class FallingWords : MonoBehaviour
{   
    public static FallingWords instance { get; set; }    
    [Header("Base Script")]
    public List<string> BaseWordsVolume = new List<string>();
    [HideInInspector] public List<string> AdaptiveWordsVolume = new List<string>();
    [HideInInspector] public List<Players> PlayersList = new List<Players>();
    [Range(100, 1000)]
    public int AdaptiveWordSearchRange = 100;

    [Range(0,10)]
    public float MaxSpawnHorizontalDistance = 0;
    [Range(0, 10)]
    public float DelayBetweenWords = 1;
    [Range(0.1f, 10f)]
    public float WordFallingSpeed = 1.33f;
    public bool DoWordCrumble = true;

    [HideInInspector] public List<WordToEntityStructure> WordsOnScreen = new List<WordToEntityStructure>();
    [HideInInspector] public List<LetterToWordStructure> LettersOnScreen = new List<LetterToWordStructure>();
    [Header("Other Script Components")]
    public LocalCommands InputsManagement;
    public ScoringComponent ScoreManagement;

    [Header("Words Management")]
    public TextAsset textFileAsset;
    public RectTransform WordTransformPosition;
    public Transform WordTransformParent;
    public GameObject WordEntity;

    [Header("Other")]
    public int CooldownBetweenWordInputs = 300;
    public RectTransform[] WordToGoLocations = new RectTransform[5];
    public PlayerSlotAccess[] PlayerListUI = new PlayerSlotAccess[5];
    public bool GameStarted = false;
    private bool apprunning = false;

    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        if (GameObject.Find("NetworkManager") == null) SceneManager.LoadScene(0);

        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        if (Application.isBatchMode) { Debug.Log("I Am server WordGame"); apprunning = true; }
    }
    private void Start()
    {
        //ONLY RUN AS SERVER
        if (!Application.isBatchMode) return;

        if(textFileAsset != null)
        {
            BaseWordsVolume.Clear();
            BaseWordsVolume = new List<string>(Regex.Split(textFileAsset.text,"[\\n]"));
        }

        //Take a random chunk of "AdaptiveWordSearch" entities out of the available words to use for gameplay
        int chunkStart = UnityEngine.Random.Range(0, (BaseWordsVolume.Count - 1 - AdaptiveWordSearchRange));
        AdaptiveWordsVolume.AddRange(BaseWordsVolume.GetRange(chunkStart, AdaptiveWordSearchRange));
    }

    public int SendCooldownOnConnect()
    {
        return CooldownBetweenWordInputs;
    }    

    //Workaround wierd bug that ocurrs when stopping Async/Await while it's running with a delay
    private void OnApplicationQuit()
    {
        apprunning = false;
    }
    public void ReceiveLetterFromPlayer(char letter,string PlayerUUID)
    {
        bool missed = true;

        //DebugLog
        Debug.Log($"Received Letter: {letter}");

        for (int i=0;i<LettersOnScreen.Count;i++)
        {
            if (letter == LettersOnScreen[i].Letter)
            {
                missed = false;
                StruckLetter(LettersOnScreen[i].Letter, PlayerUUID);
            }
            else
            {
                //do nothing and wait for loop to finish
            }
        }
        if(missed)
        {
            MissedLetter(PlayerUUID,letter);
        }
    }
    //parcurge toate literele de pe ecran
    //parcurge toate cuvintele de pe ecran si verifica daca litera apasata se regaseste in cuvinte
    //dezactiveaza index-ul literei lovite si scoate cover-ul
    public void StruckLetter(char LetterStruck,string PlayerUUID)
    {
        int Hits = 0;
        for (int z = 0; z < 2; z++)
        {
            for (int i = 0; i < LettersOnScreen.Count; i++)
            {
                if (LettersOnScreen[i].Letter == LetterStruck)
                {
                    Hits++;
                    LettersOnScreen.RemoveAt(i);
                }
            }
        }
        PlayersList[0].playerScript.ReturnKeyInfoToPlayers(LetterStruck);

        AwardPoints(PlayerUUID, true, Hits, LetterStruck.ToString());
    }

    public void MissedLetter(string PlayerUUID,char letter)
    {
        AwardPoints(PlayerUUID, false, 1,letter.ToString());
    }

    public void AwardPoints(string PlayerUUID,bool PositivePoints,int quantity,string LetterWord,bool ContainsWord = false)
    {
        //Debug.Log($"Hits quantity :{PositivePoints} {quantity}");
        //build a system that awards different/multiple points based on type and quantity or if it is a word award extra
        foreach(Players player in PlayersList)
        {
            if(player.UniqueIdentifier == PlayerUUID)
            {
                if(PositivePoints)
                {
                    if(ContainsWord)
                    {
                        player.Score += 300;
                    }
                    else
                    {
                        //checkvowels
                        if (Regex.IsMatch(LetterWord,"[aAeEiIoOuU]"))
                        {
                            player.Score += 95 * quantity;
                        }
                        else
                        {
                            player.Score += 110 * quantity;
                        }
                    }
                }
                else
                {
                    if(ContainsWord)
                    {
                        player.Score -= 200;
                    }
                    else
                    {
                        player.Score -= 100;
                    }
                }
                break;
            }
        }
        Debug.Log($"Score debugging: {PlayersList[0].Score}");
        for(int i = 0; i <PlayersList.Count;i++)
        {
            if(PlayerUUID == PlayersList[i].UniqueIdentifier)
            {
                PlayersList[0].playerScript.UpdatePointsBoard(i, PlayersList[i].Score);
            }
        }    
        
    }
    
    public void ReceiveWordFromPlayer(string word,string PlayerUUID)
    {
        char[] chars = word.ToCharArray();
        bool missed = true;

        //Debug
        for (int i=0;i<WordsOnScreen.Count;i++)
        {
            Debug.Log($"Comparing {WordsOnScreen[i]} with {word}");
            if (WordsOnScreen[i].Word == word)
            {
                Debug.Log($"FOUND word '{word}'");
                missed = false;
                for(int y=0;y< WordsOnScreen[i].Word.Length;y++)
                {
                    for(int k=0;k<LettersOnScreen.Count;k++)
                    {
                        if (chars[y] == LettersOnScreen[k].Letter)
                        {
                            LettersOnScreen.RemoveAt(k);
                        }
                    }
                    PlayersList[0].playerScript.ReturnWordInfoToPlayers(i);
                }
                RequestToReplaceWordOnSlot(i);
            }
            else
            {
                Debug.Log($"No word '{word}' found");
                //do nothing, wait for loop to end
            }
        }
        //WordsOnScreen.RemoveAt(indexHit);
        if (missed)
        {
            AwardPoints(PlayerUUID, false, 1, word, true);
        }
        else
        {
            AwardPoints(PlayerUUID, true, 1, word, true);
        }
    }
    public void BreakAndSaveWordLetters(string wordToBreak, int iteration)
    {
        for(int i=0;i<wordToBreak.Length;i++)
        {
            LetterToWordStructure nStruct = new LetterToWordStructure();
            nStruct.Letter = wordToBreak[i];
            nStruct.IndexOfWord = iteration;
            LettersOnScreen.Add(nStruct);
        }
    }
    //Execute Only if server ////////////////////////////////////
    public async void RequestToSpawnWords()
    {
        //Destroy previous words & set new ones
        foreach (WordToEntityStructure word in WordsOnScreen)
        {
            Destroy(word.gameObject);
        }
        WordsOnScreen.Clear();
        PlayersList[0].playerScript.CleanPlayersLists();

        for (int i = 0; i < 5; i++)
        {
            if(apprunning)
            {
                int targetedIndex = UnityEngine.Random.Range(0, AdaptiveWordsVolume.Count - 1);
                string newWord = AdaptiveWordsVolume[targetedIndex];
                AdaptiveWordsVolume.RemoveAt(targetedIndex);

                BreakAndSaveWordLetters(newWord, i);

                SpawnWord(newWord,i);
                PlayersList[0].playerScript.SpawnWordForAll(newWord, i, false);

                await Task.Delay((int)(DelayBetweenWords * 1000));
            }
            else
            {
                //Debug.Log("WordSpawn Operaction has been canceled");
                return;
            }
        }

        CrumbleWordBits(false);       
        //Debug.Log($"Words Cleared & New Ones Set, remaining in playable list = {AdaptiveWordsVolume.Count}");
    }
    public void CrumbleWordBits(bool isTargeted, int target = -1)
    {
        Debug.Log(DoWordCrumble);
        if(!DoWordCrumble)
        {
            //Debug.Log("Word Crumble is disabled, skipping");
            return;
        }
        //Targets a single word for 'crumble' as part of the "RequestToReplaceWordOnSlot" command
        if (isTargeted)
        {
            int targetedindex = UnityEngine.Random.Range(0, 49) / 10;
            for (int y = 0; y < LettersOnScreen.Count; y++)
            {
                if (LettersOnScreen[y].IndexOfWord == target && LettersOnScreen[y].Letter == WordsOnScreen[target].Word[targetedindex])
                {
                    //Debug.Log($"Crumble letter {LettersOnScreen[y]} from word {WordsOnScreen[target].Word[targetedindex]}");
                    LettersOnScreen.RemoveAt(y);
                }
            }
            WordsOnScreen[target].LetterCovers[targetedindex].gameObject.SetActive(false);
            PlayersList[0].playerScript.ReturnWordCoversOnCrumble(target, targetedindex);
        }
        //Targets all the words for 'crumble' as part of the " RequestToSpawnWords" command
        else
        {
            //Debug.Log("Started Crumble Loop");
            for (int i = 0; i < WordsOnScreen.Count; i++)
            {
                int targetedindex = UnityEngine.Random.Range(0, 4);
                for (int y = 0; y < LettersOnScreen.Count; y++)
                {
                    if (LettersOnScreen[y].IndexOfWord == i && LettersOnScreen[y].Letter == WordsOnScreen[i].Word[targetedindex])
                    {
                        LettersOnScreen.RemoveAt(y);
                    }
                }
                //WordsOnScreen[i].LetterCovers[targetedindex].gameObject.SetActive(false);
                PlayersList[0].playerScript.ReturnWordCoversOnCrumble(i,targetedindex);
            }
        }
    }
    //Execute Only if server ///////////////////////////////////
    public void RequestToReplaceWordOnSlot(int slotIndex)
    {
        //Destroy previous word at target index & replace at index
        GameObject preparedforDestroy = WordsOnScreen[slotIndex].gameObject;
        WordsOnScreen.RemoveAt(slotIndex);
        Destroy(preparedforDestroy);
        PlayersList[0].playerScript.CleanPlayersListIndex(slotIndex);

        int targetedIndex = UnityEngine.Random.Range(0, AdaptiveWordsVolume.Count - 1);
        string newWord = AdaptiveWordsVolume[targetedIndex];
        AdaptiveWordsVolume.RemoveAt(targetedIndex);

        BreakAndSaveWordLetters(newWord, slotIndex);

        SpawnWord(newWord, slotIndex,true);
        PlayersList[0].playerScript.SpawnWordForAll(newWord, slotIndex,true);

        PlayersList[0].playerScript.ReturnHideWordAtTarget(slotIndex);

        CrumbleWordBits(true,slotIndex);

        //Debug.Log($"Word Replaced at index {slotIndex}, remaining in playable list = {AdaptiveWordsVolume.Count}");
    }
    public void SpawnWord(string newWord, int iteration, bool targeted = false)
    {
        Debug.Log($"NewWord {newWord} at index {iteration}");
        int newIndexer = 0;
        if (targeted)
        {
            WordsOnScreen.Insert(iteration,Instantiate(WordEntity, WordTransformPosition.localToWorldMatrix.GetPosition() + new Vector3(UnityEngine.Random.Range(-MaxSpawnHorizontalDistance * 100, MaxSpawnHorizontalDistance * 100), 0, 0), Quaternion.identity, WordTransformParent).GetComponent<WordToEntityStructure>());
            newIndexer = iteration;
        }    
        else
        {
            WordsOnScreen.Add(Instantiate(WordEntity, WordTransformPosition.localToWorldMatrix.GetPosition() + new Vector3(UnityEngine.Random.Range(-MaxSpawnHorizontalDistance * 100, MaxSpawnHorizontalDistance * 100), 0, 0), Quaternion.identity, WordTransformParent).GetComponent<WordToEntityStructure>());

            newIndexer = WordsOnScreen.Count - 1;
        }

        WordsOnScreen[newIndexer].SendWordToLetters(newWord);

        WordsOnScreen[newIndexer].PointToTravelTo = WordToGoLocations[iteration].position;
        WordsOnScreen[newIndexer].TravelSpeed = WordFallingSpeed;

    }

    public void StartGame()
    {
        for(int i=0;i<PlayersList.Count;i++)
        {
            PlayersList[i].playerScript.ReturnCooldownInputSetting(CooldownBetweenWordInputs,i);
            PlayersList[i].playerScript.ReturnSetPlayersPortraits(i);
        }
        GameStarted = true;
        RequestToSpawnWords();
    }

    public void ResetScene()
    {
        GameStarted = false;
        PlayersList.Clear();
        AdaptiveWordsVolume.Clear();
        foreach(WordToEntityStructure word in WordsOnScreen)
        {
            Destroy(word.gameObject);
        }
        WordsOnScreen.Clear();
        LettersOnScreen.Clear();

        int chunkStart = UnityEngine.Random.Range(0, (BaseWordsVolume.Count - 1 - AdaptiveWordSearchRange));
        AdaptiveWordsVolume.AddRange(BaseWordsVolume.GetRange(chunkStart, AdaptiveWordSearchRange));
    }
}
