using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordToEntityStructure : MonoBehaviour
{
    public GameObject WordObject;
    public string Word;
    public TextMeshProUGUI[] Letters = new TextMeshProUGUI[5];
    public Image[] LetterCovers = new Image[5];

    public Vector3 MemorizeSpawnLocation;
    public Vector3 PointToTravelTo;
    public float TravelSpeed = 0.1f;

    private void Awake()
    {
        MemorizeSpawnLocation = new Vector3(transform.position.x,transform.position.y,transform.position.z);
    }

    private void Update()
    {
        if(Vector3.Distance(MemorizeSpawnLocation,PointToTravelTo) > 0.1f)
        {
            WordObject.transform.position = Vector3.Lerp(transform.position, PointToTravelTo, TravelSpeed * Time.deltaTime);
        }
    }

    public void SendWordToLetters(string word)
    {
        Word = word;
        for (int i = 0; i < 5; i++)
        {
            Letters[i].text = Word[i].ToString();
        }
    }
}
