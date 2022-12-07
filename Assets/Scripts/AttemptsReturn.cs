using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttemptsReturn : MonoBehaviour
{
    public TextMeshProUGUI localWord;
    public TextMeshProUGUI globalWord;
    public TextMeshProUGUI localLetter;
    public TextMeshProUGUI globalLetter;

    public void addAdditionalLocalWord(string word)
    {
        string text = localWord.text;
        string newtext = "";
        if (text.Length > 11)
        {
            newtext = text.Substring(text.IndexOf('\n') + 1) + '\n' + word;
        }
        else
        {
            newtext = text + '\n' + word;
        }
        localWord.text = newtext;
    }
    public void addAdditionalGlobalWord(string word)
    {
        string text = globalWord.text;
        string newtext = "";
        if(text.Length > 11)
        {
            newtext = text.Substring(text.IndexOf('\n') + 1) + '\n' + word;
        }
        else
        {
            newtext = text + '\n' + word;
        }    
        globalWord.text = newtext;
    }
    public void addAdditionalLocalLetter(char letter)
    {
        string text = localLetter.text;
        string newtext = "";
        if (text.Length > 2)
        {
            newtext = letter + text.Substring(0, 2);
        }
        else
        {
            newtext = letter + text;
        }
        localLetter.text = newtext;
    }
    public void addAdditionalGlobalLetter(char letter)
    {
        string text = globalLetter.text;
        string newtext = "";
        if(text.Length > 4)
        {
            newtext = letter + text.Substring(0, 4);
        }
        else
        {
            newtext = letter + text;
        }
        globalLetter.text = newtext;
    }
}
