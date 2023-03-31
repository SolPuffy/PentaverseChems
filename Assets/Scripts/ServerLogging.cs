using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ServerData
{
    public string TimeOfGameStart;
    public string TimeOfLoggingBackup;
    public int GameTime;
    public int PlayerCount;
    public int AverageActionsPerformed;
    public List<WinningPlacement> WinningPlacements = new List<WinningPlacement>();
    public List<gameActions> ActionsPerformed = new List<gameActions>();
    public BoardSetup currentSetup = new BoardSetup();
    public serverAssets WordLists = new serverAssets();
}
[System.Serializable]
public class gameActions
{
    public int GameIndex;
    public string identifier;
    public string actionType;
    public string actionContent;
    public bool isActionSuccessful;
    public string timeOfAction;
}
[System.Serializable]
public class WinningPlacement
{
    public string UniqueIdOfPlayer;
    public int PortraitIndex;
    public int Score;
}
[System.Serializable]
public class BoardSetup
{

}
[System.Serializable]
public class serverAssets
{
    public List<usedWords> UsedWordsThisGame = new List<usedWords>();
    public List<string> AvailableWordsThisGame = new List<string>();
}
[System.Serializable]
public class usedWords
{
    public string UsedWord;
    public int IndexOnScreen;
    public int UsedWordIndex;
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
        //GetFileDataPath();
        UnityEngine.Debug.Log("performing backup...");
        if (InstanceData.TimeOfGameStart == "")
        {
            InstanceData.TimeOfGameStart = "Game was never started";
        }
        InstanceData.TimeOfLoggingBackup = DateTime.Now.ToString("G");
        InstanceData.PlayerCount = FallingWords.instance.PlayersList.Count;
        InstanceData.AverageActionsPerformed = Mathf.FloorToInt(InstanceData.ActionsPerformed.Count / InstanceData.PlayerCount);

        string JsonOutput = JsonUtility.ToJson(InstanceData);
        //await System.IO.File.WriteAllTextAsync(PathToFile, JsonOutput);

        string key = "d34efaf7-ab1b-4d91-897a-63ed3efc2abf-48326fdb-a038-4ca2-ad29-2b11a170ad0b";
        string generatedID = generateRandomSaveId();
        string subDir = "words";
        byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonOutput);
        //Debug.Log($"Trying to send element at adress: http://localhost:3000/api/postfile?subDir={subDir}&fileName={generatedID}");
        UnityWebRequest request = UnityWebRequest.Post($"http://localhost:3000/api/postfile?subDir={subDir}&fileName={generatedID}", "POST");
        request.SetRequestHeader("x-api-key", key);
        request.uploadHandler = new UploadHandlerRaw(payload);
        //request.downloadHandler = new DownloadHandlerBuffer();

        await Task.FromResult(request.SendWebRequest());
        while (request.result == UnityWebRequest.Result.InProgress)
        {
            Debug.LogError($"Save attempt in progress! Code:{request.uploadProgress}");
            await Task.Yield();
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Save failed! Error:{request.error}");
            return;
        }
        else
        {
            string text = request.downloadHandler.text;
            Debug.Log(text);
            Debug.Log("Save Successful");
        }

        UnityEngine.Debug.Log($"Backup location: {PathToFile}, Backup date: {InstanceData.TimeOfLoggingBackup}");
    }
    /*Deprecated Saving to local check method
    private Task<bool> CheckBackup(string input)
    {
        return Task.FromResult(File.Exists(ReadFileDataPath(input)));
    }
    */
    private async Task ReadBackup(string fileIndex)
    {
        string key = "d34efaf7-ab1b-4d91-897a-63ed3efc2abf-48326fdb-a038-4ca2-ad29-2b11a170ad0b";
        string subDir = "words";
        //Debug.Log($"Trying to find element at adress: http://localhost:3000/api/getfile?subDir={subDir}&fileName={fileIndex}");
        UnityWebRequest request = UnityWebRequest.Get($"http://localhost:3000/api/getfile?subDir={subDir}&fileName={fileIndex}");
        request.SetRequestHeader("x-api-key", key);

        await Task.FromResult(request.SendWebRequest());
        while (request.result == UnityWebRequest.Result.InProgress)
        {
            await Task.Yield();
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Load failed! Error:{request.error}");
            return;
        }
        else
        {
            string text = request.downloadHandler.text;

            string ClearedText = Regex.Replace(text, @"\\", "");
            string RetrievedData = ClearedText.Substring(1, ClearedText.Length - 2);

            JsonUtility.FromJsonOverwrite(RetrievedData, ServerLogging.InstanceLogging.InstanceData);
            Debug.Log("Load Successful");
        }
    }
    #endregion
    #region StaticFunctions
    public static void AddActionToList(int IdentifyingIndex, string UniqueID, string actionType, string actionContents, bool actionSuc)
    {
        ServerLogging.InstanceLogging.InstanceData.ActionsPerformed.Add(new gameActions { GameIndex = IdentifyingIndex, identifier = UniqueID, actionType = actionType, actionContent = actionContents, isActionSuccessful = actionSuc, timeOfAction = DateTime.Now.ToString("T") });
    }
    public static void AddUsedWordToList(string wordSelected,int indexOnScreen)
    {
        ServerLogging.InstanceLogging.InstanceData.WordLists.UsedWordsThisGame.Add(new usedWords { UsedWord = wordSelected, IndexOnScreen = indexOnScreen ,UsedWordIndex = ServerLogging.InstanceLogging.InstanceData.WordLists.AvailableWordsThisGame.IndexOf(wordSelected) });
    }
    public static void RegisterGameTime(int timeInSeconds)
    {
        ServerLogging.InstanceLogging.InstanceData.GameTime = timeInSeconds;
    }
    public static void RegisterAvailableWordList(string[] words)
    {
        ServerLogging.InstanceLogging.InstanceData.WordLists.AvailableWordsThisGame.AddRange(words);
    }
    public static void ResetCurrentLogData()
    {
        ServerLogging.InstanceLogging.InstanceData = new ServerData();
    }
    public static void RegisterStartTime()
    {
        ServerLogging.InstanceLogging.InstanceData.TimeOfGameStart = DateTime.Now.ToString("G");
    }
    public static void RegisterWinningPlacements(WinningPlacement[] placements)
    {
        InstanceLogging.InstanceData.WinningPlacements.AddRange(placements);
    }
    public async static void RequestLogBackup()
    {
        await ServerLogging.InstanceLogging.PerformBackup();
    }
    public async static Task<ServerData> RequestInstanceData()
    {
        return await Task.FromResult(ServerLogging.InstanceLogging.InstanceData);
    }
    /* Deprecated Saving to local check method
    public async static Task<bool> CheckBackupPresence(string input)
    {
        return await ServerLogging.InstanceLogging.CheckBackup(input);
    }
    */
    public async static Task RequestDataFromServer(string fileIndex)
    {
        await ServerLogging.InstanceLogging.ReadBackup(fileIndex);
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
        return Application.dataPath + "/SaveFiles/" + "Cards" + textInput + ".json";
#elif UNITY_ANDROID
        return Application.persistentDataPath + "/SaveFiles/" + "Cards" + textInput + ".json";
#elif UNITY_IPHONE
        return Application.persistentDataPath + "/SaveFiles/" + "Cards" + textInput + ".json";
#else
        return Application.dataPath + "/SaveFiles/" + "Cards" + textInput + ".json";
#endif
    }
    private void GetFileDataPath()
    {
#if UNITY_EDITOR
        PathToFile = Application.dataPath + "/SaveFiles/" + "Cards" + generateRandomSaveId() + ".json";
#elif UNITY_ANDROID
        PathToFile = Application.persistentDataPath + "/SaveFiles/" + "Cards" + generateRandomSaveId() + ".json";
#elif UNITY_IPHONE
        PathToFile = Application.persistentDataPath + "/SaveFiles/" + "Cards" + generateRandomSaveId() + ".json";
#else
        PathToFile = Application.dataPath + "/SaveFiles/" + "Cards" + generateRandomSaveId() + ".json";
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
