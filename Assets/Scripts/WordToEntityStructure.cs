using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordToEntityStructure : MonoBehaviour
{
    public TextMeshProUGUI[] Letters = new TextMeshProUGUI[5];
    public Image[] LetterCovers = new Image[5];

    public Vector3 MemorizeSpawnLocation;
    public Vector3 PointToTravelTo;
    public float TravelSpeed = 0.1f;
    private bool keepUpdate = true;

    private void Awake()
    {
        MemorizeSpawnLocation = new Vector3(transform.position.x,transform.position.y,transform.position.z);
    }
    private void Start()
    {
        FakeUpdate();
    }
    public async void FakeUpdate()
    {
        while(keepUpdate)
        {
            if (Vector3.Distance(transform.position, PointToTravelTo) > 1f)
            {
                gameObject.transform.position = Vector3.Lerp(transform.position, PointToTravelTo, TravelSpeed * Time.deltaTime);
            }
            else
            {
                break;
            }
            await Task.Yield();
        }
        
    }    
    public void SendWordToLetters(string word)
    {
        for (int i = 0; i < 5; i++)
        {
            Letters[i].text = word[i].ToString();
        }
    }

    private void OnDestroy()
    {
        keepUpdate = false;
    }
}
