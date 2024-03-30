using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
public class Lobby : MonoBehaviourPunCallbacks
{
    public string playerName = "Player 1";
    //users are separated by game version, so if we update something, older versions wont be able to run it
    string gameVersion = "0.9";
    //the list of created rooms
    List<RoomInfo> createdRooms = new List<RoomInfo>();
    //use this name when creating a Room
    string roomName = "Room 1";
    void Start()
    {
        //this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
        {
            //set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            //connect to the photon masterserver
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
        //after we connected to Master server, join the Lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //after this callback, update the room list
        createdRooms = roomList;
    }
    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("ChooseLevel");
        }
        //create random roomName with random name 
        roomName = Random.Range(10000,99999).ToString();
        RoomOptions roomOptions = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = (byte)4,
            //this is for finding our players stats
            PublishUserId = true
        };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
    public bool JoinRoomWithID(string roomID)
    {
        //set our Player name
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.JoinRoom(roomID);
        return true;
    }

    public bool CheckIfRoomExists(string roomID)
    {
        foreach (var roomInfo in createdRooms)
        {
            if (roomInfo.Name == roomID)
            {
                return true;
            }
        }
        return false;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SceneManager.LoadScene("ChooseLevel");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SceneManager.LoadScene("ChooseLevel");
    }
    public override void OnCreatedRoom()
    {
        //set our player name
        PhotonNetwork.NickName = playerName;
        //load the game
        PhotonNetwork.LoadLevel("GameLevel");
       
    }
}
