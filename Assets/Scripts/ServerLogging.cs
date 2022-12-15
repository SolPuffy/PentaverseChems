using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

[System.Serializable]
public class ServerData
{
    public string TimeOfGameStart;
    public string TimeOfLoggingBackup;
    public int PlayerCount;
    public int AverageActionsPerformed;
    public List<playerActions> ActionsPerformed = new List<playerActions>();
    public serverAssets WordLists = new serverAssets();
}
[System.Serializable]
public class playerActions
{
    public int playerInGameIndex;
    public string playerUniqueID;
    public string actionType;
    public string actionContent;
    public bool isActionSuccessful;
    public string timeOfAction;
}
[System.Serializable]
public class serverAssets
{
    public List<string> UsedWords = new List<string>();
    public string[] AvailableWordsThisGame;
}
public class ServerLogging : MonoBehaviour
{
    private static ServerLogging InstanceLogging;
    private string PathToFile;
    [SerializeField] private ServerData InstanceData = new ServerData();

    private void Awake()
    {
        InstanceLogging = this;
        CheckFolderDataPath();
        
    }
    #region BackupFunctions
    private async Task PerformBackup()
    {        
        GetFileDataPath();
        UnityEngine.Debug.Log("performing backup...");
        if (InstanceData.TimeOfGameStart == "")
        {
            InstanceData.TimeOfGameStart = "Game was never started";
        }
        InstanceData.TimeOfLoggingBackup = DateTime.Now.ToString("G");
        InstanceData.PlayerCount = FallingWords.instance.PlayersList.Count;
        InstanceData.AverageActionsPerformed = Mathf.FloorToInt(InstanceData.ActionsPerformed.Count / InstanceData.PlayerCount);

        string JsonOutput = JsonUtility.ToJson(InstanceData, true);
        await System.IO.File.WriteAllTextAsync(PathToFile, JsonOutput);

        UnityEngine.Debug.Log($"Backup location: {PathToFile}, Backup date: {InstanceData.TimeOfLoggingBackup}");
    }
    private async Task ReadBackup(string inputToFile)
    {
        if (inputToFile.Length == 9)
        {
            inputToFile = ReadFileDataPath(inputToFile);
            if (File.Exists(inputToFile))
            {
                string JsonInput = await System.IO.File.ReadAllTextAsync(inputToFile);
                JsonUtility.FromJsonOverwrite(JsonInput, InstanceData);
            }
            else
            {
                UnityEngine.Debug.LogError("ReadBackup >> File does not exist");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("ReadBackup >> Invalid Input");
        }
    }
    #endregion
    #region StaticFunctions
    public static void AddActionFromPlayerToList(int PlayerIndex, string playerID, string actionType, string actionContents, bool actionSuc)
    {
        playerActions action = new playerActions();
        action.playerInGameIndex = PlayerIndex;
        action.playerUniqueID = playerID;
        action.actionType = actionType;
        action.actionContent = actionContents;
        action.isActionSuccessful = actionSuc;
        action.timeOfAction = DateTime.Now.ToString("T");
        ServerLogging.InstanceLogging.InstanceData.ActionsPerformed.Add(action);
    }
    public static void AddUsedWordToList(string word)
    {
        ServerLogging.InstanceLogging.InstanceData.WordLists.UsedWords.Add(word);
    }    
    public static void RegisterAvailableWordList(string[] words)
    {
        ServerLogging.InstanceLogging.InstanceData.WordLists.AvailableWordsThisGame = words;
    }
    public static void ResetCurrentLogData()
    {
        ServerLogging.InstanceLogging.InstanceData = new ServerData();
    }
    public static void RegisterStartTime()
    {
        ServerLogging.InstanceLogging.InstanceData.TimeOfGameStart = DateTime.Now.ToString("G");
    }    
    public async static void RequestLogBackup()
    {        
        await ServerLogging.InstanceLogging.PerformBackup();
    }    
    public async static Task<ServerData> RequestDataFromServer(string fileIndex)
    {
        await ServerLogging.InstanceLogging.ReadBackup(fileIndex);
        return ServerLogging.InstanceLogging.InstanceData;
    }    
    #endregion
    #region FilePathingAndGeneration
    private void CheckFolderDataPath()
    {
        string path = getFolderDataPath();
        if (Directory.Exists(path))
        {
            UnityEngine.Debug.Log("FolderFound");
            //doNothing
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }
    private string generateRandomSaveId()
    {
        string RandomToReturn = "";
        for (int i = 0; i < 9; i++)
        {
            RandomToReturn += UnityEngine.Random.Range(0, 9).ToString();
        }
        return RandomToReturn;
    }
    private string ReadFileDataPath(string textInput)
    {
#if UNITY_EDITOR
        return Application.dataPath + "/SaveFiles/" + textInput + ".txt";
#elif UNITY_ANDROID
        return Application.persistentDataPath + "/SaveFiles/" + textInput + ".txt";
#elif UNITY_IPHONE
        return Application.persistentDataPath + "/SaveFiles/" + textInput + ".txt";
#else
        return Application.dataPath + "/SaveFiles/" + textInput + ".txt";
#endif
    }
    private void GetFileDataPath()
    {
#if UNITY_EDITOR
        PathToFile = Application.dataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#elif UNITY_ANDROID
        PathToFile = Application.persistentDataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#elif UNITY_IPHONE
        PathToFile = Application.persistentDataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#else
        PathToFile = Application.dataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#endif
    }
    private string getFolderDataPath()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#elif UNITY_ANDROID
        UnityEngine.Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#elif UNITY_IPHONE
        UnityEngine.Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#else
        UnityEngine.Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#endif
    }
    #endregion

}
