using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class countDown : MonoBehaviour
{
    public GameObject controller;

    public int minutes;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    private void OnEnable()
    {
        TextMeshProUGUI clock = gameObject.GetComponent<TextMeshProUGUI>();
        clock.SetText(minutes + ":00");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startCount()
    {
        StartCoroutine(count());
    }
    IEnumerator count()
    {
        
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
            PUN2_RoomController roomController = controller.GetComponent<PUN2_RoomController>();
            roomController.callEndGame();
        }
       
        //now end the game
    }
}
