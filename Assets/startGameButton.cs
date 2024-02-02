using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class startGameButton : MonoBehaviour
{
    public GameObject controller;
    public GUIStyle fontStyle;
    void Start()
    {
        fontStyle.fontSize = 80;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
       
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            
            if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 400, 300, 60), "Start the Game"))
            {
                //now start the game
                controller.GetComponent<PUN2_RoomController>().startGameButtonCallBack();
            }
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 500, Screen.height - 400, 1000, 100),"Waiting for the creator to start the game",fontStyle);
        }
    }
}
