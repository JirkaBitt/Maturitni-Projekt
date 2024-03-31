using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ChooseLevel : MonoBehaviour
{
    public GameObject inputID;
    //this houses the lobby script
    public GameObject lobby;
    //warning for the user if something goes wrong
    public GameObject warning;
    public AudioSource musicPlayer;
    void Start()
    {
        Application.targetFrameRate = 60;
        GameObject assets = GameObject.Find("AssetHolder");
        if (assets != null)
        {
            Destroy(assets);
        }
        //find if we have already started playing music
        int musicPlayersCount = GameObject.FindGameObjectsWithTag("music").Length;
        if (musicPlayersCount == 1)
        {
            DontDestroyOnLoad(musicPlayer);
        }
        else
        {
            Destroy(musicPlayer);
            //reset the volume
            GameObject existingPlayer = GameObject.FindWithTag("music");
            existingPlayer.GetComponent<AudioSource>().volume = 1f;
        }
    }
    public void CreateNew()
    {
        //create new assets
        SceneManager.LoadScene("createPrefabs");
    }

    public void LoadLevels()
    {
        //load created assets
        SceneManager.LoadScene("savedLevels");
    }
    public void JoinRoom()
    {
        //check ID from input with room names if it matches than join that room
        TMP_InputField inputText = inputID.GetComponent<TMP_InputField>();
        Lobby lobbyScript = lobby.GetComponent<Lobby>();
        //if result is false there was a problem
        bool result = lobbyScript.CheckIfRoomExists(inputText.text);
        if (result)
        {
            //room holder holds the id of the room as its name, we need it to join the room from create prefabs
            GameObject roomHolder = new GameObject();
            roomHolder.tag = "roomName";
            roomHolder.name = inputText.text;
            DontDestroyOnLoad(roomHolder);
            SceneManager.LoadScene("createPlayer");
        }
        else
        {
            //something went wrong, probably the room does not exist
            warning.SetActive(true);
        }
    }
    public void HideWarning()
    {
        warning.SetActive(false);
    }

}
