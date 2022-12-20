using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LocalCommands : MonoBehaviour
{
    private bool InputFieldState = false;
    private int localInputCurrentCooldown = 0;
    public float localInputTargetCooldown = 0;
    public TMP_InputField WordInputField;
    public BarCooldownVisual cooldownBar;
    [SerializeField] Transform _gameStart;
    [SerializeField] TMP_Dropdown droopy;
    [SerializeField] Transform PlayersUI;
    [SerializeField] Transform EndGame;
    public TextMeshProUGUI TimerText;
    public static Transform GameStart;


    private void Awake()
    {
        GameStart = _gameStart;
        GameStart.gameObject.SetActive(true);
    }

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
            //Refuse any input while the game hasn't started
            if (!PlayerToServerCommands.localPlayer.AllowInput)
            {
                return;
            }
            //Refuse Key if it's not a-z or Return

            if (!Regex.IsMatch(keyevent.keyCode.ToString(),"^([aA-zZ]{1})?(Return)?$"))
            {
                Debug.Log("Returned");
                return;
            }
            if (keyevent.keyCode == KeyCode.Return)
            {
                AccessInputField(); //closing of inputfield is set in the object's event field "on end edit"
                return;
            }
            //Refuse Key while input is on cooldown
            if (localInputCurrentCooldown > 0)
            {
                return;
            }
            PlayerToServerCommands.localPlayer.RefreshCooldown(PlayerToServerCommands.localPlayer);
            CheckLetterOnScreen(keyevent);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad4))
        {
            Debug.Log("Sending log command");
            PlayerToServerCommands.localPlayer.RequestManualBackup();
        }          
    }
    private void FixedUpdate()
    {
        if(localInputCurrentCooldown > 0)
        {
            localInputCurrentCooldown--;
            UpdateCooldownBar();
        }
    }

    public void UpdateCooldownBar()
    {
        if(localInputCurrentCooldown > Mathf.CeilToInt(localInputTargetCooldown * 0.66f))
        {
            cooldownBar.left.gameObject.SetActive(false);
            cooldownBar.middle.gameObject.SetActive(false);
            cooldownBar.right.gameObject.SetActive(false);
            return;
        }
        if (!(localInputCurrentCooldown > Mathf.CeilToInt(localInputTargetCooldown * 0.66f)))
        {
            cooldownBar.left.gameObject.SetActive(true);
        }
        if (!(localInputCurrentCooldown > Mathf.CeilToInt(localInputTargetCooldown * 0.33f)))
        {
            cooldownBar.middle.gameObject.SetActive(true);
        }
        if (!(localInputCurrentCooldown > 0))
        {
            cooldownBar.right.gameObject.SetActive(true);
        }
    }    

    /*public void UpdateCooldownBar()
    {
        if(localInputCurrentCooldown > 0)
        {
            cooldownBar.AccessBadge.SetActive(true);
        }
        else
        {
            cooldownBar.AccessBadge.SetActive(false);
        }
        cooldownBar.AccessLeftBar.fillAmount = (float)localInputCurrentCooldown / localInputTargetCooldown;
        cooldownBar.AccessRightBar.fillAmount = (float)localInputCurrentCooldown / localInputTargetCooldown;

        switch((float)localInputCurrentCooldown / localInputTargetCooldown)
        {
            case > 0.65f:
                {
                    cooldownBar.AccessLeftBar.color = Color.red;
                    cooldownBar.AccessRightBar.color = Color.red;
                    break;
                }
            case > 0.30f:
                {
                    cooldownBar.AccessLeftBar.color = Color.yellow;
                    cooldownBar.AccessRightBar.color = Color.yellow;
                    break;
                }
            default:
                {
                    cooldownBar.AccessLeftBar.color = Color.green;
                    cooldownBar.AccessRightBar.color = Color.green;
                    break;
                }
        }
    }*/
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
        localInputCurrentCooldown = (int)localInputTargetCooldown;
        PlayerToServerCommands.localPlayer.SendKeyToServer(keyevent.keyCode.ToString().ToLower()[0]);
    }
    //LOCAL
    public void CheckWordOnScreen(string word)
    {
        PlayerToServerCommands.localPlayer.SendWordToServer(word);
        //Debug.Log("Detected word code: " + word);
    }

    public void StartGame()
    {
        int gametime = 0;
        switch (droopy.value)
        {
            case 0:
                gametime = 60;
                break;

            case 1:
                gametime = 120;
                break;

            case 2:
                gametime = 180;
                break;

            case 3:
                gametime = 240;
                break;

            case 4:
                gametime = 300;
                break;

            default:
                gametime = 60;
                break;

        }
        PlayerToServerCommands.localPlayer.StartGame(gametime);
    }

    public static void KillStartt()
    {
        GameStart.gameObject.SetActive(false);
    }

    public void EndGames()
    {
        EndGame.gameObject.SetActive(true);
        PlayersUI.transform.parent = EndGame.transform;
    }
}
