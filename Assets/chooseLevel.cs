using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class chooseLevel : MonoBehaviour
{
    //new game room
    public GameObject createButton;

    public GameObject inputID;
    //join existing room
    public GameObject joinButton;

    public GameObject lobby;

    public GameObject warning;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void createNew()
    {
        SceneManager.LoadScene("createPrefabs");
    }
    public void joinRoom()
    {
        //check ID from input with room names if it matches than join that room
        TMP_InputField inputText = inputID.GetComponent<TMP_InputField>();
        
        PUN2_GameLobby lobbyScript = lobby.GetComponent<PUN2_GameLobby>();
        //if result is false there was a problem
        bool result = lobbyScript.joinRoomWithName(inputText.text);
        
        if (!result)
        {
            warning.SetActive(true);
        }
    }

    public void hideWarning()
    {
        warning.SetActive(false);
    }
}
