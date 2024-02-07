using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class startGameButton : MonoBehaviourPunCallbacks
{
    public GameObject controller;
    public GameObject startButton;
    public GameObject textObject;
    public GUIStyle StartStyle = new GUIStyle();
    
    void Start()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.SetActive(true);
            textObject.SetActive(false);
        }
        else
        {
            startButton.SetActive(false);
            textObject.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.SetActive(true);
            textObject.SetActive(false);
        }
        else
        {
            startButton.SetActive(false);
            textObject.SetActive(true);
        }
        base.OnMasterClientSwitched(newMasterClient);
    }

    public void startGame()
    {
        controller.GetComponent<PUN2_RoomController>().startGameButtonCallBack();
    }
}
