using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class CheckReplayList : MonoBehaviour
{    
    [SerializeField] private GameObject ScrollContentPrefab;
    [SerializeField] ScrollRect scrollView;
    [SerializeField] Image StartButton;
    private string replayname;
    public static CheckReplayList instance;

    private void Awake()
    {
        instance = this;
        Destroy(NetworkManager.singleton.gameObject);
    }

    private void Start()
    {
        replayname = null;
        for (int i = 0; i < scrollView.content.childCount; i++)
        {
            //Debug.Log("Destroying child " + i);
            Destroy(scrollView.content.GetChild(i).gameObject);
        }
        GetAllSaveFiles();
    }

    private async void GetAllSaveFiles()
    {
        Debug.Log("TGrying to get save files");
        string[] files = await ServerLogging.RequestDirData();
        foreach (string file in files)
        {
             GameObject g = Instantiate(ScrollContentPrefab, scrollView.content.transform);
             g.GetComponent<SavedFilePrefabScript>().SetFileName(file);                       
        }
    }

    public void StartFileReplay()
    {
        ReplayBehavior.instance.ConfirmSelectReplay(replayname);
        gameObject.SetActive(false);
    }

    public static void Onselect(string name)
    {
        instance.replayname = name;
        instance.StartButton.color = Color.green;
    }
    
    private string getFolderDataPath()
    {
#if UNITY_EDITOR
        //Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#elif UNITY_ANDROID
        //Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#elif UNITY_IPHONE
        //Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#else
        //Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#endif
    }  
}
