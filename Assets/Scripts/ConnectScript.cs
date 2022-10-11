using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ConnectScript : MonoBehaviour
{
    [SerializeField] NetworkManager NManager;
    private void Awake()
    {
        Application.targetFrameRate = 30;
        if (Application.isBatchMode)
            NManager.StartServer();
    }

    public void StartClient()
    {
        NManager.StartClient();
    }
}
