using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Players
{
    //whatever other things
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
    private void Update()
    {
        //Local testing
        if(Input.GetKeyDown(KeyCode.K))
        {
            RequestToSpawnWords();
        }
    }
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

    [HideInInspector] public List<WordToEntityStructure> WordsOnScreen = new List<WordToEntityStructure>();
    [HideInInspector] public List<LetterToWordStructure> LettersOnScreen = new List<LetterToWordStructure>();
    [Header("Other Script Components")]
    public LocalCommands InputsManagement;
    public ScoringComponent ScoreManagement;

    [Header("Words Management")]
    public RectTransform WordTransformPosition;
    public Transform WordTransformParent;
    public GameObject WordEntity;
    public TMP_InputField WordInputField;
    private bool InputFieldState = false;

    [Header("Other")]
    public RectTransform[] WordToGoLocations = new RectTransform[5];
    private bool apprunning = false;

    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        apprunning = true;
    }
    private void Start()
    {
        //Take a random chunk of "AdaptiveWordSearch" entities out of the available words to use for gameplay
        int chunkStart = UnityEngine.Random.Range(0, (BaseWordsVolume.Count - 1 - AdaptiveWordSearchRange));
        AdaptiveWordsVolume.AddRange(BaseWordsVolume.GetRange(chunkStart, AdaptiveWordSearchRange));
    }
    //Workaround wierd bug that ocurrs when stopping Async/Await while it's running with a delay
    private void OnApplicationQuit()
    {
        apprunning = false;
    }
    private void ClearWords()
    {
        foreach(Transform word in WordTransformParent.gameObject.GetComponentsInChildren<Transform>())
        {
            DestroyImmediate(word.gameObject);
        }
    }
    //
    //Detect Keyboard Inputs & return their keycodes (mare grija cu asta ca dupa zic astia ca bagam key loggers)
    public async void OnGUI()
    {
        while (true)
        {
            Event keyevent = Event.current;
            try
            {
                if (keyevent.isKey && Input.GetKeyDown(keyevent.keyCode) && !InputFieldState)
                {
                    if(keyevent.keyCode == KeyCode.Return)
                    {
                        AccessInputField();
                        return;
                    }
                    CheckLetterOnScreen(keyevent);
                }
            }
            catch { }
            await Task.Yield();
        }
    }

    public void AccessInputField()
    {
        //if not open, open the input field
        if(!InputFieldState)
        {
            InputFieldState = true;
            WordInputField.gameObject.SetActive(true);
            WordInputField.text = "";
            WordInputField.Select();

        }
        //else close it and verify the word
        else
        {
            InputFieldState = false;
            if(WordInputField.text.Length < 5)
            {
                //cannot input word with less than 5 characters
                Debug.Log("Cannot input word with less than 5 characters");
                return;
            }
            else
            {
                CheckWordOnScreen(WordInputField.text);
            }
            WordInputField.gameObject.SetActive(false);
        }
    }

    public void CheckLetterOnScreen(Event keyevent)
    {
        Debug.Log("Detected key code: " + keyevent.keyCode);
    }

    public void CheckWordOnScreen(string word)
    {
        Debug.Log("Detected word code: " + word);
    }

    public async void RequestToSpawnWords()
    {
        //Execute Only if server
        for (int i = 0; i < 5; i++)
        {
            if(apprunning)
            {
                int targetedIndex = UnityEngine.Random.Range(0, AdaptiveWordsVolume.Count - 1);
                string newWord = AdaptiveWordsVolume[targetedIndex];
                AdaptiveWordsVolume.RemoveAt(targetedIndex);
                ServerCommands.instance.SpawnWordForAll(newWord, i);
                await Task.Delay((int)(DelayBetweenWords * 1000));
            }
            else
            {
                Debug.Log("WordSpawn Operaction has been canceled");
                break;
            }
        }
    }
    public void RequestToReplaceWordOnSlot(int slotIndex)
    {
        int targetedIndex = UnityEngine.Random.Range(0, AdaptiveWordsVolume.Count - 1);
        string newWord = AdaptiveWordsVolume[targetedIndex];
        AdaptiveWordsVolume.RemoveAt(targetedIndex);
        ServerCommands.instance.SpawnWordForAll(newWord, slotIndex);
    }
    public void SpawnWord(string newWord,int iteration)
    {
        WordsOnScreen.Add(Instantiate(WordEntity, WordTransformPosition.localToWorldMatrix.GetPosition() + new Vector3(UnityEngine.Random.Range(-MaxSpawnHorizontalDistance * 100,MaxSpawnHorizontalDistance * 100),0,0), Quaternion.identity, WordTransformParent).GetComponent<WordToEntityStructure>());
        int newIndexer = WordsOnScreen.Count - 1;
        WordsOnScreen[newIndexer].SendWordToLetters(newWord);

        WordsOnScreen[newIndexer].PointToTravelTo = WordToGoLocations[iteration].position;
        WordsOnScreen[newIndexer].TravelSpeed = WordFallingSpeed;

        //AdaptiveWordsVolume.RemoveAt(IndexOfWord);
        //remove word as soon as it's spawned for everyone
    }

}
