using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LocalCommands : MonoBehaviour
{
    //LOCAL
    //Detect Keyboard Inputs & return their keycodes (mare grija cu asta ca dupa zic astia ca bagam key loggers xd)
    public void OnGUI()
    {
        //Disable Key Logging while inputfield is active
        if (FallingWords.instance.InputFieldState)
        {
            return;
        }
        Event keyevent = Event.current;
        if (keyevent.isKey && Input.GetKeyDown(keyevent.keyCode) && !FallingWords.instance.InputFieldState)
        {
            if (keyevent.keyCode == KeyCode.Return)
            {
                AccessInputField(); //closing of inputfield is set in the object's event field "on end edit"
                return;
            }
            CheckLetterOnScreen(keyevent);
        }
    }
    //LOCAL
    public void AccessInputField()
    {
        //if not open, open the input field
        if (!FallingWords.instance.InputFieldState)
        {
            FallingWords.instance.InputFieldState = true;
            FallingWords.instance.WordInputField.gameObject.SetActive(true);
            FallingWords.instance.WordInputField.text = "";
            FallingWords.instance.WordInputField.Select();

        }
        //else close it and verify the word
        else
        {
            FallingWords.instance.InputFieldState = false;
            if (FallingWords.instance.WordInputField.text.Length < 5)
            {
                //cannot input word with less than 5 characters
                Debug.Log("Cannot input word with less than 5 characters");
                return;
            }
            else
            {
                CheckWordOnScreen(FallingWords.instance.WordInputField.text);
            }
            FallingWords.instance.WordInputField.gameObject.SetActive(false);
        }
    }
    //LOCAL
    public void CheckLetterOnScreen(Event keyevent)
    {
        PlayerToServerCommands.instance.SendKeyToServer(keyevent);
        //Debug.Log("Detected key code: " + keyevent.keyCode);
    }
    //LOCAL
    public void CheckWordOnScreen(string word)
    {
        PlayerToServerCommands.instance.SendWordToServer(word);
        Debug.Log("Detected word code: " + word);
    }
}
