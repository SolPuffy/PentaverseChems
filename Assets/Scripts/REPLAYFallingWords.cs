using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RePlayers
{
    //whatever other things
    public PlayerSlotAccess playerUI;
    public string UniqueIdentifier;
    public int Score;
}
public class REPLAYFallingWords : MonoBehaviour
{
    public static REPLAYFallingWords instance { get; set; }
    [Header("Base Script")]
    public List<string> BaseWordsVolume = new List<string>();
    [HideInInspector] public List<string> AdaptiveWordsVolume = new List<string>();
    [HideInInspector] public List<RePlayers> PlayersList = new List<RePlayers>();
    [Range(100, 1000)]
    public int AdaptiveWordSearchRange = 100;

    [Range(0, 10)]
    public float MaxSpawnHorizontalDistance = 0;
    [Range(0, 10)]
    public float DelayBetweenWords = 1;
    [Range(0.1f, 10f)]
    public float WordFallingSpeed = 1.33f;
    public bool DoWordCrumble = true;

    [HideInInspector] public List<WordAdditionalStructure> WordsOnScreen = new List<WordAdditionalStructure>();
    //[HideInInspector] public List<LetterToWordStructure> LettersOnScreen = new List<LetterToWordStructure>();
    [Header("Other Script Components")]
    //public LocalCommands InputsManagement;
    //public ScoringComponent ScoreManagement;
    public ReplayBehavior ReplayScript;

    [Header("Words Management")]
    public TextAsset textFileAsset;
    public RectTransform WordTransformPosition;
    public Transform WordTransformParent;
    public GameObject WordEntity;

    [Header("Other")]
    public int CooldownBetweenWordInputsSeconds = 5;
    public int InitialTimerStartValueSeconds = 300;
    public RectTransform[] WordToGoLocations = new RectTransform[5];
    public PlayerSlotAccess[] PlayerUI = new PlayerSlotAccess[5];
    public Sprite[] PlayerPortraits = new Sprite[5];
    public AttemptsReturnOverhaul AttemptsReturnUI;
    public bool GameStarted = false;
    private int TimerCurrentTime = 9999;
    private bool apprunning = false;
    private int wordsPlayed = 0;
    //private bool ended = false;

    ////////////////////////////////////////////////////////////

    //Workaround wierd bug that ocurrs when stopping Async/Await while it's running with a delay
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    private void OnApplicationQuit()
    {
        apprunning = false;
    }
    public void ReceiveLetterFromPlayer(char letter, string PlayerUUID)
    {
        bool missed = true;
        int Hits = 0;
        List<int> IndexOfWord = new List<int>();
        List<int> IndexOfCover = new List<int>();
        //DebugLog
        //Debug.Log($"Received Letter: {letter} from {PlayerUUID}");

        for (int i = 0; i < WordsOnScreen.Count; i++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (letter == WordsOnScreen[i].Structure.Letters[y].text.ToLower()[0])
                {
                    IndexOfWord.Add(i);
                    IndexOfCover.Add(y);
                    //StruckLetter(LettersOnScreen[i].Letter, PlayerUUID);
                }
                else
                {
                    //do nothing and wait for loop to finish
                }
            }
            for (int y = 0; y < WordsOnScreen[i].HeldLetters.Count; y++)
            {
                if (letter == WordsOnScreen[i].HeldLetters[y])
                {
                    missed = false;
                    Hits++;
                    WordsOnScreen[i].HeldLetters.RemoveAt(y);
                    i = 0;
                }
            }
        }
        for (int i = 0; i < IndexOfWord.Count; i++)
        {
            ReplayScript.ReturnKeyInfoToPlayers(IndexOfWord[i], IndexOfCover[i]);
        }

        /* string aro = "";
         int countlet = 0;
         foreach(WordAdditionalStructure a in WordsOnScreen)
         {
             foreach(char b in a.HeldLetters)
             {
                 aro += $" {b}";
                 countlet++;
             }
         }
         Debug.Log($"Letters Available {countlet}");
         Debug.Log($"Letters Array: {aro}");*/

        if (missed)
        {
            MissedLetter(PlayerUUID, letter);
        }
        else
        {
            StruckLetter(PlayerUUID, letter, Hits);
        }
        for (int i = 0; i < PlayersList.Count; i++)
        {
            if (PlayerUUID == PlayersList[i].UniqueIdentifier)
            {
                ReplayScript.ReturnAttemptedLetterLocally(letter);
            }
        }
    }
    //parcurge toate literele de pe ecran
    //parcurge toate cuvintele de pe ecran si verifica daca litera apasata se regaseste in cuvinte
    //dezactiveaza index-ul literei lovite si scoate cover-ul
    public void StruckLetter(string PlayerUUID, char LetterStruck, int Hits)
    {
        AwardPoints(PlayerUUID, true, Hits, LetterStruck.ToString());
    }

    public void MissedLetter(string PlayerUUID, char letter)
    {
        AwardPoints(PlayerUUID, false, 1, letter.ToString());
    }

    public void AwardPoints(string PlayerUUID, bool PositivePoints, int quantity, string LetterWord, bool ContainsWord = false)
    {
        foreach (RePlayers player in PlayersList)
        {
            if (player.UniqueIdentifier == PlayerUUID)
            {
                if (PositivePoints)
                {
                    if (ContainsWord)
                    {
                        player.Score += 5;
                    }
                    else
                    {
                        if (Regex.IsMatch(LetterWord, "[aAeEiIoOuU]"))
                        {
                            player.Score += 2 * quantity;
                        }
                        else
                        {
                            player.Score += 2 * quantity;
                        }
                    }
                }
                else
                {
                    if (ContainsWord)
                    {
                        player.Score -= 2;
                    }
                    else
                    {
                        player.Score -= 1;
                    }
                }
                break;
            }
        }

        for (int i = 0; i < PlayersList.Count; i++)
        {
            if (PlayerUUID == PlayersList[i].UniqueIdentifier)
            {
                ReplayScript.UpdatePointsBoard(i, PlayersList[i].Score);
            }
        }
    }

    public void ReceiveWordFromPlayer(string word, string PlayerUUID)
    {
        //Debug.Log($"Received Word: {word} from {PlayerUUID}");

        word = word.ToLower();
        char[] chars = word.ToCharArray();
        bool missed = true;

        //Debug
        for (int i = 0; i < WordsOnScreen.Count; i++)
        {
            //Debug.Log($"Wtf is going on with {word}");
            //Debug.Log($"Comparing {WordsOnScreen[i].Word} with {word}");
            if (WordsOnScreen[i].HeldWord == word)
            {
                //Debug.Log($"FOUND word '{word}'");
                missed = false;
                ReplayScript.ReturnWordInfoToPlayers(i);
                RequestToReplaceWordOnSlot(i);
            }
            else
            {
                //Debug.Log($"No word '{word}' found");
                //do nothing, wait for loop to end
            }
        }
        //RETURN ATTEMPTED WORDS TO PLAYER BOARDS
        for (int i = 0; i < PlayersList.Count; i++)
        {
            if (PlayerUUID == PlayersList[i].UniqueIdentifier)
            {
                ReplayScript.ReturnAttemptedWordLocally(word);
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
        for (int i = 0; i < wordToBreak.Length; i++)
        {
            WordsOnScreen[iteration].HeldLetters.Add(wordToBreak[i]);
        }

        /*string aro = "";
        int countlet = 0;
        foreach (WordAdditionalStructure a in WordsOnScreen)
        {
            foreach (char b in a.HeldLetters)
            {
                aro += $" {b}";
                countlet++;
            }
        }
        Debug.Log($"Letters Available {countlet}");
        Debug.Log($"Letters Array: {aro}");*/

    }
    //Execute Only if server ////////////////////////////////////
    public async void RequestToSpawnWords()
    {
        //Destroy previous words & set new ones
        foreach (WordAdditionalStructure word in WordsOnScreen)
        {
            Destroy(word.Structure.gameObject);
        }
        WordsOnScreen.Clear();
        ReplayScript.CleanPlayersLists();

        for (int i = 0; i < 5; i++)
        {
            if (apprunning)
            {
                string newWord = ReplayScript.savefileSnapshot.WordLists.AvailableWordsThisGame[ReplayScript.savefileSnapshot.WordLists.UsedWordsThisGame[wordsPlayed].UsedWordIndex];
                wordsPlayed++;



                //AdaptiveWordsVolume.RemoveAt(targetedIndex);

                SpawnWord(newWord, i);
                BreakAndSaveWordLetters(newWord, i);

                //ReplayScript.SpawnWordForAll(newWord, i, false, true);

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
        //Debug.Log(DoWordCrumble);
        if (!DoWordCrumble)
        {
            //Debug.Log("Word Crumble is disabled, skipping");
            return;
        }
        //Targets a single word for 'crumble' as part of the "RequestToReplaceWordOnSlot" command
        if (isTargeted)
        {
            int targetedindex = UnityEngine.Random.Range(0, 4);
            WordsOnScreen[target].HeldLetters.RemoveAt(targetedindex);
            //WordsOnScreen[target].Structure.LetterCovers[targetedindex].gameObject.SetActive(false); Server doesn't need to crumble words
            ReplayScript.ReturnWordCoversOnCrumble(target, targetedindex);
        }
        //Targets all the words for 'crumble' as part of the " RequestToSpawnWords" command
        else
        {
            //Debug.Log("Started Crumble Loop");
            for (int i = 0; i < WordsOnScreen.Count; i++)
            {
                int targetedindex = UnityEngine.Random.Range(0, 4);
                WordsOnScreen[i].HeldLetters.RemoveAt(targetedindex);
                //WordsOnScreen[i].LetterCovers[targetedindex].gameObject.SetActive(false);
                ReplayScript.ReturnWordCoversOnCrumble(i, targetedindex);
            }
        }

        string aro = "";
        int countlet = 0;
        foreach (WordAdditionalStructure a in WordsOnScreen)
        {
            foreach (char b in a.HeldLetters)
            {
                aro += $" {b}";
                countlet++;
            }
        }
        //Debug.Log($"Letters Available {countlet}");
        //Debug.Log($"Letters Array: {aro}");
    }
    //Execute Only if server ///////////////////////////////////
    public void RequestToReplaceWordOnSlot(int slotIndex)
    {
        //Destroy previous word at target index & replace at index
        GameObject preparedforDestroy = WordsOnScreen[slotIndex].Structure.gameObject;
        WordsOnScreen.RemoveAt(slotIndex);
        Destroy(preparedforDestroy);
        //ReplayScript.CleanPlayersListIndex(slotIndex);

        string newWord = ReplayScript.savefileSnapshot.WordLists.AvailableWordsThisGame[ReplayScript.savefileSnapshot.WordLists.UsedWordsThisGame[wordsPlayed].UsedWordIndex];
        wordsPlayed++;
        //AdaptiveWordsVolume.RemoveAt(targetedIndex);

        SpawnWord(newWord, slotIndex, true);
        BreakAndSaveWordLetters(newWord, slotIndex);

        //ReplayScript.SpawnWordForAll(newWord, slotIndex, true, true);

        ReplayScript.ReturnHideWordAtTarget(slotIndex);

        CrumbleWordBits(true, slotIndex);

        //Debug.Log($"Word Replaced at index {slotIndex}, remaining in playable list = {AdaptiveWordsVolume.Count}");
    }
    public void SpawnWord(string newWord, int iteration, bool targeted = false, bool localEntity = false)
    {

        //Debug.Log($"NewWord '{newWord}' spawned at index '{iteration}'.");
        int newIndexer = 0;
        if (targeted)
        {
            WordsOnScreen.Insert(iteration, Instantiate(WordEntity, WordTransformPosition.localToWorldMatrix.GetPosition() + new Vector3(UnityEngine.Random.Range(-MaxSpawnHorizontalDistance * 100, MaxSpawnHorizontalDistance * 100), 0, 0), Quaternion.identity, WordToGoLocations[iteration]).GetComponent<WordAdditionalStructure>());
            newIndexer = iteration;
        }
        else
        {
            WordsOnScreen.Add(Instantiate(WordEntity, WordTransformPosition.localToWorldMatrix.GetPosition() + new Vector3(UnityEngine.Random.Range(-MaxSpawnHorizontalDistance * 100, MaxSpawnHorizontalDistance * 100), 0, 0), Quaternion.identity, WordToGoLocations[iteration]).GetComponent<WordAdditionalStructure>());
            newIndexer = WordsOnScreen.Count - 1;
        }

        if (!localEntity)
        {
            WordsOnScreen[newIndexer].HeldWord = newWord;
        }

        Debug.Log($"new word:{newWord},iteration:{iteration},targeted:{targeted},localEntity:{localEntity}");

        WordsOnScreen[newIndexer].Structure.SendWordToLetters(newWord);
        WordsOnScreen[newIndexer].Structure.PointToTravelTo = WordToGoLocations[iteration].position;
        WordsOnScreen[newIndexer].Structure.TravelSpeed = WordFallingSpeed;
    }

    public void StartGame(int TimerSeconds)
    {
        InitialTimerStartValueSeconds = TimerSeconds;

        for (int i = 0; i < PlayersList.Count; i++)
        {

            //ReplayScript.ReturnServerPlayerSetting(CooldownBetweenWordInputsSeconds * 30, i);
            ReplayScript.ReturnSetPlayersPortraits(i);
            //ReplayScript.CorrectPortraitOrder(PlayersList[i].UniqueIdentifier, i);
        }

        TimerCurrentTime = 30 * InitialTimerStartValueSeconds;
        ReplayScript.UpdateGameTimers(TimerCurrentTime);

        apprunning = true;
        GameStarted = true;
        RequestToSpawnWords();
    }

    public void ResetScene()
    {
        GameStarted = false;
        //ended = false;
        PlayersList.Clear();
        AdaptiveWordsVolume.Clear();
        foreach (WordAdditionalStructure word in WordsOnScreen)
        {
            Destroy(word.Structure.gameObject);
        }
        WordsOnScreen.Clear();

        int chunkStart = UnityEngine.Random.Range(0, (BaseWordsVolume.Count - 1 - AdaptiveWordSearchRange));
        AdaptiveWordsVolume.AddRange(BaseWordsVolume.GetRange(chunkStart, AdaptiveWordSearchRange));
    }
}
