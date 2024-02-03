using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class startGameButton : MonoBehaviour
{
    public GameObject controller;
    public GUIStyle StartStyle = new GUIStyle();
    void Start()
    {
        StartStyle.fontSize = 80;
        StartStyle.normal.textColor = Color.red;
        StartStyle.alignment = TextAnchor.MiddleCenter;
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
            Rect rect = new Rect(0,0,1000,80);
            rect.center = new Vector2(Screen.width / 2, Screen.height / 2 - 400);
            GUI.Label(rect,"Waiting for the creator to start the game",StartStyle);
        }
    }
}
