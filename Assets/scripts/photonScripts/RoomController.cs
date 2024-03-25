using System;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
public class RoomController : MonoBehaviourPunCallbacks
{
    //spawn points for player and weapons
    public Vector3[] spawnPoint;
    //serializable int for controlling how many weapons should be in game
    public int maxWeaponCount = 5;
    //array of names of the weapons for spawning
    public string[] WeaponNames;
    //reference to the object that is creating all the assets, for changing the player texture
    public GameObject CreatePrefabs;
    //reference to the object that is showing the stats of all players at the bottom
    public GameObject displayIcons;
    //end screen
    public GameObject endGUI;
    //end controller handles the showing of the results
    public GameObject endController;
    //in game UI for showing buttons, clock, etc
    public GameObject inGameUI;
    //reference to the timer of the game
    public GameObject clock;
    //object that creates the start game button or the text that is shown to other players
    public GameObject startGameButton;
    //object that shows the id of the room
    public GameObject RoomIDText;
    //reference to my player so we dont have find him averytime
    private GameObject myPlayer;
    //if we can soawn weapons, it is false when we are showing results
    public bool spawningEnabled = true;
    //reference to the playagain button in the result panel, we want to deactivate it when the master starts a game, so that the player isnt able to join after the game was started
    public GameObject playAgainButton;
    //check if game is in progress
    public bool gameIsActive = false;
    void Start()
    {
        //set the default fps to 60
        Application.targetFrameRate = 60;
        inGameUI.SetActive(true);
        //In case we started this demo with the wrong scene being active, simply load the menu scene
        if (PhotonNetwork.CurrentRoom == null)
        {
            //some error occured, return to the lobby
            SceneManager.LoadScene("ChooseLevel");
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //spawn character, this is only for the room creator, others are created in OnJoinedRoom
                PhotonNetwork.InstantiateRoomObject("Arena", Vector3.zero, Quaternion.identity);
                print("instantiate from start");
                spawnPoint = findRespawnPositions();
                //spawn the player
                int randonSpawn = Random.Range(0, spawnPoint.Length);
                GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[randonSpawn], Quaternion.identity);
                //we have to assign the position again, bcs it can spawn us in the middle of the platform
                player.transform.position = spawnPoint[randonSpawn];
                myPlayer = player;
                //disable some scripts before the start
                beforeStart(player);
                PhotonView playerView = player.GetPhotonView();
                //name him after the player so we can find him in onGUI
                CameraMovement camScript = Camera.main.GetComponent<CameraMovement>();
                camScript.enabled = true;
                camScript.player = player;
                camScript.rb = player.GetComponent<Rigidbody2D>();
                //we have to call it on create prefabs because room controller is disabled at the start of the game
                string playerID = PhotonNetwork.LocalPlayer.UserId;
                //we want to rename the player on all clients to his id and change his texture to the one the player has drawn
                CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(playerID, playerView.ViewID,PhotonNetwork.LocalPlayer.NickName);
                displayIcons.GetComponent<DisplayPlayerStats>().addPlayerTexture(playerID);
            }
            else
            {
                joinedFromCreate();
            }
        }
    }
    public void leave()
    {
        //this will be called when the button is pressed
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        //We have left the Room, return back to the GameLobby
        photonView.RPC("updateCameraPlayers",RpcTarget.All);
        SceneManager.LoadScene("ChooseLevel");
    }

    public void joinedFromCreate()
    {
        base.OnJoinedRoom();
        //we have to wait until we are in room, it can take some time
        //then we want to instantiate a player that will be visible for every player in room
        spawnPoint = findRespawnPositions(); 
        // int numberOfPlayers = PhotonNetwork.CurrentRoom.Players.Count;
        int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
        //instantiate player
        GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[randomSpawnIndex], Quaternion.identity, 0);
        player.transform.position = spawnPoint[randomSpawnIndex];
        myPlayer = player;
        beforeStart(player);
        //name him after the player so we can find him in onGUI
        PhotonView playerView = player.GetPhotonView();
        CameraMovement camScript = Camera.main.GetComponent<CameraMovement>();
        camScript.enabled = true;
        camScript.player = player;
        camScript.rb = player.GetComponent<Rigidbody2D>();
        camScript.updatePlayers();
        //call rpc buffered so it runs even for players that join later
        //we have to call it on the photonview of the roomcontroller because that is the only one with namePlayer function
        CreatePrefab createScript = CreatePrefabs.GetComponent<CreatePrefab>();
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        createScript.changePlayer(player);
        createScript.renamePlayer(playerID, playerView.ViewID,PhotonNetwork.LocalPlayer.NickName);
        //add the icon of this player to the bottom of the screen
        displayIcons.GetComponent<DisplayPlayerStats>().addPlayerTexture(playerID);
        //add the player to all camera movement scripts
        photonView.RPC("updateCameraPlayers",RpcTarget.All);
    }

    IEnumerator spawnWeapon()
    {
        while (true)
       {
           //only the master client can spawn weapons, this is here because when master leaves, someone else will be master
           while (!PhotonNetwork.IsMasterClient)
           {
               yield return new WaitForSeconds(5);
           }
           if (spawningEnabled)
           {
               GameObject[] weapons = GameObject.FindGameObjectsWithTag("weapon");
               while (weapons.Length >= maxWeaponCount)
               {
                   yield return new WaitForSeconds(5);
                   weapons = GameObject.FindGameObjectsWithTag("weapon");
                   if (!spawningEnabled)
                   {
                       break;
                   }
               }
               //it can happen that we will have max weapons and when we destroy them that we get here because we are in the previos loop, so we want to skip this iteration, because it would spawn a Weapon when we are showing the results
               if (!spawningEnabled)
               {
                   continue;
               }
               int randomWeaponIndex = Random.Range(0, WeaponNames.Length);
               //find random spawn position
               int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
               //spawn the Weapon, we want it as a room object so when master leaves it does not destroy all the weapons
               GameObject weapon = PhotonNetwork.InstantiateRoomObject(WeaponNames[randomWeaponIndex],
                   spawnPoint[randomSpawnIndex],
                   Quaternion.identity, 0);
           }
           int randomWait = Random.Range(8, 15);
           yield return new WaitForSeconds(randomWait);
       }
    }
    Vector3[] findRespawnPositions()
    {
        List<Vector3> spawnList = new List<Vector3>();
        GameObject[] ground = GameObject.FindGameObjectsWithTag("ground");
        //iterate for every platform
        foreach (var platform in ground)
        {
            PolygonCollider2D coll = platform.GetComponent<PolygonCollider2D>();
            //retrieve paths from collider
            List<Tuple<Vector2, Vector2>> pathPoints = new List<Tuple<Vector2, Vector2>>();
            for (int i = 0; i < coll.pathCount; i++)
            {
                //retrieve bounds
                pathPoints.Add(getBounds(coll.GetPath(i),coll));
            }
            foreach (var bounds in pathPoints)
            {
                int minX = (int)bounds.Item1.x;
                int maxX = (int)bounds.Item2.x;
                int maxY = (int)bounds.Item1.y;
                //we want to leave a bit of room on the sides and at the top
                int spawnY = (int)maxY + 1;
                for (int x = (int)minX + 2; x < (int)maxX - 2; x++)
                {
                    //now we have increments of 1 on the length of the platform
                    Vector3 position = new Vector3(x, spawnY, 0);
                    //check if we are safe to spawn there by casting a raycast to the spawnPosition and checking if it collides with something
                    RaycastHit2D[] hits = Physics2D.CircleCastAll(position, 0.7f, Vector2.zero);
                    if (hits.Length == 0)
                    {
                        print("no hits!!!!!!!!!!");
                        spawnList.Add(position);
                    }
                    else
                    {
                        //check if some of the hits are ground
                        bool hitGround = false;
                        foreach (var oneHit in hits)
                        {
                            if (oneHit.collider.gameObject.CompareTag("ground"))
                            {
                                hitGround = true;
                                break;
                            } 
                        }
                        if (!hitGround)
                        {
                            //we are save, we did not hit any ground object
                            spawnList.Add(position);
                        }
                    }
                }
            }
        }
        //return found spawnPoints
        return spawnList.ToArray();
    }
    private void displaySpawnPoints()
    {
        //this is used just to visualize the spawn point for debugging 
        foreach (var spawn in spawnPoint)
        {
            GameObject dis = new GameObject();
            SpriteRenderer rend = dis.AddComponent<SpriteRenderer>();
            rend.sprite = myPlayer.GetComponent<SpriteRenderer>().sprite;
            dis.transform.localScale = new Vector3(0.05f, 0.05f, 1);
            dis.transform.position = spawn;
        }
    }
    private Tuple<Vector2, Vector2> getBounds(Vector2[] path,PolygonCollider2D coll)
    {
        Vector2 minX = new Vector2(1000,-1000);
        Vector2 maxX = new Vector2(-1000,-1000);
        Vector2 offset = coll.offset;
        foreach (var pointLocal in path)
        {
            //we have to transform from local space to world space
            //we have to apply offset as well because it is relative to the collider
            Vector2 point = coll.transform.TransformPoint(pointLocal + offset);
            float xValue = point.x;
            //we want to find the most left corner and the most right corner
            if (xValue < minX.x)
            {
                minX.x = xValue;
            }
            if (xValue > maxX.x)
            {
                maxX.x = xValue;
            }
            //we have to retrieve the roof of the platform
            if (minX.y < point.y)
            {
                minX.y = point.y;
                maxX.y = point.y;
            }
        }
        return new Tuple<Vector2, Vector2>(minX, maxX);
    }

    [PunRPC]
    public void updateCameraPlayers()
    {
        CameraMovement camScript = Camera.main.GetComponent<CameraMovement>();
        camScript.updatePlayers();
    }
    public void beforeStart(GameObject player)
    {
        //we dont want the players to move when the game hasnt started yet
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement.enabled = false;
        //show everyone the room id so others can join
        RoomIDText.GetComponent<TextMeshProUGUI>().SetText("Room ID: " + PhotonNetwork.CurrentRoom.Name);
        //startgamebutton displays the button for the master and info text for others
        startGameButton.SetActive(true);
    }

    public void startGameButtonCallBack()
    {
        //AllViaServer should call it on all clients at the same time
        photonView.RPC("startGame",RpcTarget.AllViaServer);
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        //spawn a weapon for every player
        for (int i = 0; i < playerCount; i++)
        {
            int randomWeaponIndex = Random.Range(0, WeaponNames.Length);
            //find random spawn position
            int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
            //spawn the Weapon, we want it as a room object so when master leaves it does not destroy all the weapons
            GameObject weapon = PhotonNetwork.InstantiateRoomObject(WeaponNames[randomWeaponIndex],
                spawnPoint[randomSpawnIndex],
                Quaternion.identity, 0);
        }
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }
    public void playAgain()
    {
        //we have decided from the results that we want to play again
        endGUI.SetActive(false);
        inGameUI.SetActive(true);
        RoomIDText.SetActive(true);
        //we have deleted the player so we have to spawn a new one
        int randomIndex = Random.Range(0, spawnPoint.Length);
        GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[randomIndex], Quaternion.identity);
        myPlayer = player;
        //disable some scripts before the start
        beforeStart(player);
        PhotonView playerView = player.GetPhotonView();
        //name him after the player so we can find him in onGUI
        CameraMovement camScript = Camera.main.GetComponent<CameraMovement>();
        camScript.enabled = true;
        camScript.player = player;
        camScript.rb = player.GetComponent<Rigidbody2D>();
        camScript.updatePlayers();
        //we have to call it on create prefabs because room controller is disabled at the start of the game
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        //only set the room to open if the master has clicked to play again
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
        else
        {
            CreatePrefabs.GetComponent<CreatePrefab>().changePlayer(player);
        }
        CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(playerID, playerView.ViewID,PhotonNetwork.LocalPlayer.NickName);
        photonView.RPC("updateCameraPlayers",RpcTarget.AllBuffered);
        displayIcons.SetActive(true);
        displayIcons.GetComponent<DisplayPlayerStats>().addPlayerTexture(playerID);
    }
    [PunRPC]
    public void startGame()
    {
        //enable movement
        myPlayer.GetComponent<PlayerMovement>().enabled = true;
        spawningEnabled = true;
        gameIsActive = true;
        //now start the clock
        clock.GetComponent<CountDown>().startCount();
        RoomIDText.SetActive(false);
        startGameButton.SetActive(false);
        //disable the button even for those that are still in the result ui
        playAgainButton.SetActive(false);
        //only the master will spawn the weapons, but we want to run it at all clients in case that one of them bbecomes master
        StartCoroutine(spawnWeapon());
    }

    public void callEndGame()
    {
        photonView.RPC("endGame",RpcTarget.AllViaServer);
    }
    [PunRPC] public void endGame()
    {
        //game has ended, show the results
        inGameUI.SetActive(false);
        startGameButton.SetActive(false);
        playAgainButton.SetActive(true);
        spawningEnabled = false;
        gameIsActive = false;
        CameraMovement camScript = Camera.main.GetComponent<CameraMovement>();
        camScript.enabled = false;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (PhotonNetwork.IsMasterClient)
        {
            //we want to delete every Weapon
            GameObject[] weapons = GameObject.FindGameObjectsWithTag("weapon");
            foreach (var weap in weapons)
            {
                PhotonNetwork.Destroy(weap);
            }
        }
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (var bull in bullets)
        {
            Destroy(bull);
        }
        int playerCount = players.Length;
        endGUI.SetActive(true);
        //show the results based on score
        ShowAllResults showResults = endController.GetComponent<ShowAllResults>();
        PlayerStats[] stats = new PlayerStats[playerCount];
        Dictionary<string, int> scores = new Dictionary<string, int>();
        Dictionary<string, int> placements = new Dictionary<string, int>();
        Dictionary<string, string> nicks = new Dictionary<string, string>();
        
        for (int i = 0; i < playerCount; i++)
        {
            stats[i] = players[i].GetComponent<PlayerStats>();
            //reset the weapon, bcs we have deleted it so that when player is deleted it does not throw an error when it wants to drop the weapon that does not exist
            stats[i].currentWeapon = null;
            scores.Add(players[i].name,stats[i].score);
            //here we cannot add the name of the gameobject bcs we dont know if it matches with the photon array
            nicks.Add(PhotonNetwork.PlayerList[i].UserId,PhotonNetwork.PlayerList[i].NickName);
        }
        var sortedDict = scores.OrderBy(pair => pair.Value).ToList();
        //order by sorts it in an ascending order, but we want descending, player with highest score wins
        sortedDict.Reverse();
        for (int i = 0; i < playerCount; i++)
        {
            placements.Add(sortedDict[i].Key,i+1);
        }
        showResults.showResults(players,placements,nicks);
        //reset the display icons dictionaries
        DisplayPlayerStats disStats = displayIcons.GetComponent<DisplayPlayerStats>();
        disStats.clearValues();
        displayIcons.SetActive(false);
        foreach (var player in players)
        {
            PhotonView playerView = player.GetComponent<PhotonView>();
            if (playerView.IsMine)
            {
                PhotonNetwork.Destroy(player);
            }
        }
        //stop the coroutine, so that if we play again the weapons dont spawn twice
        StopCoroutine("spawnWeapon");
       
    }
}
