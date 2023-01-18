using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedLines : MonoBehaviour
{
    public Image[] lines = new Image[9];
    public RectTransform[] linesPositions = new RectTransform[9];
    public bool[] positiveDirection = new bool[9];
    public List<int> movementLimit = new List<int>();
    public List<float> movementVelocity = new List<float>();
    public List<Vector3> InitialPosition = new List<Vector3>();
    public bool IsReady = false;
    [Range(0.1f, 1f)]
    public float AverageVelocity = 1f;
    private void Start()
    {
        for(int i=0;i<9;i++)
        {
            InitialPosition.Add(new Vector3(linesPositions[i].position.x, linesPositions[i].position.y, linesPositions[i].position.z));
            movementLimit.Add(Random.Range(40,140));
            movementVelocity.Add(Random.Range(0.01f,0.1f));
        }
        IsReady = true;
    }
}
