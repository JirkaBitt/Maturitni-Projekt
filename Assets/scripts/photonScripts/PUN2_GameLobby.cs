using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


//https://sharpcoderblog.com/blog/make-a-multiplayer-game-in-unity-3d-using-pun-2
//from this tutorial
public class PUN2_GameLobby : MonoBehaviourPunCallbacks
{
    string playerName = "Player 1";
    //Users are separated from each other by gameversion (which allows you to make breaking changes).
    string gameVersion = "0.9";
    //The list of created rooms
    List<RoomInfo> createdRooms = new List<RoomInfo>();
    //Use this name when creating a Room
    string roomName = "Room 1";
    bool joiningRoom = false;
    // Start is called before the first frame update
    void Start()
    {
        //This makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + cause.ToString() + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        //After we connected to Master server, join the Lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("We have received the Room list");
        //After this callback, update the room list
        createdRooms = roomList;
    }

    public void createRoomWithoutUI()
    {
        //create random roomName with random name 
        roomName = Random.Range(10000,99999).ToString();
        joiningRoom = true;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)4; //Set any number
        //this is for finding our players stats
        roomOptions.PublishUserId = true;
        print("join Lobby");
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        
        
    }

    public bool joinRoomWithName(string joinName)
    {
        joiningRoom = true;

        //Set our Player name
        PhotonNetwork.NickName = playerName;

        //Join the Room
        print("clickedJoinRoom");

        //if (checkIfRoomExists(joinName))
       // {
            //it is a legit room
            PhotonNetwork.JoinRoom(joinName);
            return true;
       // }
        joiningRoom = false;
        return false;
    }

    public bool checkIfRoomExists(string joinName)
    {
        foreach (var roomInfo in createdRooms)
        {
            if (roomInfo.Name == joinName)
            {
                return true;
            }
        }
        return false;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
        joiningRoom = false;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
        joiningRoom = false;
    }

   

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        //Set our player name
        PhotonNetwork.NickName = playerName;
        //Load the Scene called GameLevel (Make sure it's added to build settings)
        PhotonNetwork.LoadLevel("GameLevel");
       
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        print("OnJoinedRoom");
    }
}
