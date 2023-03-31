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
    public Transform personalReturnTextLoc;
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

    public void getInput(string type, string content, string IDSending)
    {
        Debug.Log($"input : {content}");
        bool success = false;
        switch (type)
        {
            case "LetterSend":
                {
                    foreach(WordAdditionalStructure word in WordsOnScreen)
                    {
                        for(int i=0;i<5;i++)
                        {
                            if (word.Structure.Letters[i].text == content && word.Structure.Letters[i].color != Color.red)
                            {
                                success = true;
                                word.Structure.Letters[i].color = Color.red;
                                AwardPoints(IDSending, true, 1, content);
                            }
                        }
                    }
                    if(!success)
                    {
                        AwardPoints(IDSending, false, 1, content);
                    }
                    break;
                }
            case "WordSend":
                {
                    for(int i = 0;i<WordsOnScreen.Count;i++)
                    {
                        if (WordsOnScreen[i].HeldWord == content)
                        {
                            GameObject preparedforDestroy = WordsOnScreen[i].Structure.gameObject;
                            WordsOnScreen.RemoveAt(i);
                            Destroy(preparedforDestroy);
                            SpawnWord(ReplayScript.savefileSnapshot.WordLists.UsedWordsThisGame[wordsPlayed].UsedWord,i,true);
                            AwardPoints(IDSending, true, 1, content, true);

                        }    
                    }
                    if (!success)
                    {
                        AwardPoints(IDSending, false, 1, content, true);
                    }
                    break;
                }
            default:break;
        }
    }

    public void SpawnWord(string newWord, int iteration, bool targeted = false)
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
            newIndexer = wordsPlayed;
        }

        WordsOnScreen[newIndexer].HeldWord = newWord;

        Debug.Log($"new word:{newWord},iteration:{iteration},targeted:{targeted}");

        WordsOnScreen[newIndexer].Structure.SendWordToLetters(newWord);
        WordsOnScreen[newIndexer].Structure.PointToTravelTo = WordToGoLocations[iteration].position;
        WordsOnScreen[newIndexer].Structure.TravelSpeed = WordFallingSpeed;

        wordsPlayed++;
    }
}
