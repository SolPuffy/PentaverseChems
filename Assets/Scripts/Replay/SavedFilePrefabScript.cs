using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SavedFilePrefabScript : MonoBehaviour
{
    public string FileName = "";
    [SerializeField] TextMeshProUGUI textField;

    public void SetFileName(string file)
    {
        FileName = file;
        textField.text = file;
    }

    public void onclick()
    {
        CheckReplayList.Onselect(FileName);
    }
}
