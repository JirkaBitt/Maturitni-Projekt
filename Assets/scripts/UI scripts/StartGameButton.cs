using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class StartGameButton : MonoBehaviourPunCallbacks
{
    public GameObject controller;
    public GameObject startButton;
    public GameObject textObject;
    void Start()
    {
        //if the player is the master show him the button, otherwise show the player the text info that we are wating before the master starts it
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
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //we have to open the room bcs it gets open when player clicks the play again button but we changed the master after it got clicked
            PhotonNetwork.CurrentRoom.IsOpen = true;
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

    public override void OnEnable()
    {
        //check if the master hasnt switched
        base.OnEnable();
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
    public void StartGame()
    {
        controller.GetComponent<RoomController>().StartGameButtonCallBack();
    }
}
