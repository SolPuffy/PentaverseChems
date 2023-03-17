using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changeSceneReplay : MonoBehaviour
{
    public void ButtonInteractionOpenReplayMode()
    {
        SceneManager.LoadSceneAsync("ReplayLocalScene", LoadSceneMode.Additive);
    }
    public void ButtonInteractionReturnToMainScreen()
    {
        SceneManager.UnloadSceneAsync("ReplayLocalScene");
    }
}
