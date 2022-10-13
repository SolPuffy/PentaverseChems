using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotAccess : MonoBehaviour
{
    public GameObject AccessGameObject;
    public Image AccessPortraitImage;
    public TextMeshProUGUI AccessScoreText;

    private void Awake()
    {
        AccessPortraitImage.color = Color.blue;
    }
}
