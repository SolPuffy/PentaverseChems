using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class ReplayBehavior : MonoBehaviour
{
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
        if(input.text.Length > 9)
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

        Debug.Log("Begin Save Playback.");
    }
}
