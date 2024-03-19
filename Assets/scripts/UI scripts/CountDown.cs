using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class CountDown : MonoBehaviour
{
    public GameObject controller;
    public int minutes;
   
    private void OnEnable()
    {
        TextMeshProUGUI clock = gameObject.GetComponent<TextMeshProUGUI>();
        clock.SetText(minutes + ":00");
    }
    public void startCount()
    {
        StartCoroutine(count());
    }
    IEnumerator count()
    {
        //start the countdown
        TextMeshProUGUI clock = gameObject.GetComponent<TextMeshProUGUI>();
        int seconds = 0;
        int fullTime = minutes * 60;
        while (true)
        {
            yield return new WaitForSeconds(1);
            seconds++;
            int timeRemaining = fullTime - seconds;
            int minutesRemaining = timeRemaining / 60;
            int secondsRemaining = timeRemaining % 60;
            string text = "";
            if (secondsRemaining < 10)
            {
                //we want to have the zero before seconds
                 text = minutesRemaining + ":0" + secondsRemaining;
            }
            else
            {
                text = minutesRemaining + ":" + secondsRemaining;
            }
            clock.SetText(text);
            if (timeRemaining <= 0)
            {
                //the time has run out, end the game
                break;
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {  
            //end the game
            RoomController roomController = controller.GetComponent<RoomController>();
            roomController.callEndGame();
        }
    }
}
