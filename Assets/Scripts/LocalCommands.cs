using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LocalCommands : MonoBehaviour
{
    private bool InputFieldState = false;
    public TMP_InputField WordInputField;
    //LOCAL
    //Detect Keyboard Inputs & return their keycodes (mare grija cu asta ca dupa zic astia ca bagam key loggers xd)
    public void OnGUI()
    {
        //Disable Key Logging while inputfield is active
        if (InputFieldState)
        {
            return;
        }
        Event keyevent = Event.current;
        if (keyevent.isKey && Input.GetKeyDown(keyevent.keyCode) && !InputFieldState)
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
        if (!InputFieldState)
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
            if (WordInputField.text.Length < 5)
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
