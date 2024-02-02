using System;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


//https://sharpcoderblog.com/blog/make-a-multiplayer-game-in-unity-3d-using-pun-2
//from this tutorial
public class PUN2_RoomController : MonoBehaviourPunCallbacks
{

    //Player instance prefab, must be located in the Resources folder
    public GameObject playerPrefab;
    //Player spawn point
    public Vector3[] spawnPoint;
    public int maxWeaponCount = 5;
    public GameObject[] weaponPrefabs;
    public Transform weaponSpawnPoint;
    private bool canSpawnWeapon = true;
    //public GUIStyle percentageStyle;
    private List<GameObject> spawnedWeapons = new List<GameObject>();

    public string[] WeaponNames;

    public GameObject CreatePrefabs;

    public GameObject displayIcons;

    public GameObject endGUI;

    public GameObject endController;

   public GameObject inGameUI;

    public bool hideGUI;

    public GameObject clock;

    public GameObject startGameButton;

    public GameObject RoomIDText;

    public GameObject displayStats;

    private GameObject myPlayer;

    public bool spawningEnabled = true;
    
    //private List<playerStats> _statsList = new List<playerStats>();
    // Use this for initialization
    void Start()
    {
       
        //set the default fps to 60
        Application.targetFrameRate = 60;
        inGameUI.SetActive(true);
        //In case we started this demo with the wrong scene being active, simply load the menu scene
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("Is not in the room, returning back to Lobby");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
            Application.Quit();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //fetch created assets
               // getCreatedAssets();
                //spawn character, this is only for the room creator, others are created in OnJoinedRoom
                print("instantiate from start");
                spawnPoint = findRespawnPositions();
                //GameObject arena = PhotonNetwork.Instantiate("Arena", Vector3.zero, Quaternion.identity);
                GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[0], Quaternion.identity);
                myPlayer = player;
                //disable some scripts before the start
                beforeStart(player);
                PhotonView playerView = player.GetPhotonView();
                //name him after the player so we can find him in onGUI
                cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
                camScript.enabled = true;
                camScript.player = player;
                camScript.rb = player.GetComponent<Rigidbody2D>();
                //we have to call it on create prefabs because room controller is disabled at the start of the game
                string playerID = PhotonNetwork.LocalPlayer.UserId;
                CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(playerID, playerView.ViewID);
                displayIcons.GetComponent<displayPlayerStats>().addPlayerTexture(playerID);
                //call rpc buffered so it runs even for players that join later
                //we have to call it on the photonview of the roomcontroller because that is the only one with namePlayer function
                //photonView.RPC("namePlayer",RpcTarget.AllBuffered,playerView.ViewID,PhotonNetwork.LocalPlayer.UserId);
                
            }
        }
    }

    private void Update()
    {
        //only the master client will spawn weapons
    }

    void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;
        if(hideGUI)
            return;
        //Show the Room name
        GUI.Label(new Rect(135, 5, 200, 25), PhotonNetwork.CurrentRoom.Name);
    }
  
    public override void OnLeftRoom()
    {
        //We have left the Room, return back to the GameLobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChooseLevel");
    }
    public override void OnJoinedRoom()
    {
        /*
        base.OnJoinedRoom();

        //we have to wait until we are in room, it can take some time
        //then we want to instantiate a player that will be visible for every player in room
    
        print("instantiate from onjoined");
        int numberOfPlayers = PhotonNetwork.CurrentRoom.Players.Count;
        //spawn the 
        int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
      
        GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[randomSpawnIndex], Quaternion.identity, 0);
        //name him after the player so we can find him in onGUI
        PhotonView playerView = player.GetPhotonView();
        
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.enabled = true;
        camScript.player = player;
        camScript.rb = player.GetComponent<Rigidbody2D>();
        //call rpc buffered so it runs even for players that join later
        //we have to call it on the photonview of the roomcontroller because that is the only one with namePlayer function
        photonView.RPC("namePlayer",RpcTarget.AllBuffered,playerView.ViewID,PhotonNetwork.LocalPlayer.UserId);
        */
    }

    public void joinedFromCreate()
    {
        base.OnJoinedRoom();
        //we have to wait until we are in room, it can take some time
        //then we want to instantiate a player that will be visible for every player in room
        print("instantiate from create!!!!!!!");
        spawnPoint = findRespawnPositions();
       // int numberOfPlayers = PhotonNetwork.CurrentRoom.Players.Count;
       int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
      
        GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[randomSpawnIndex], Quaternion.identity, 0);
        myPlayer = player;
        beforeStart(player);
        //name him after the player so we can find him in onGUI
        PhotonView playerView = player.GetPhotonView();
        
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.enabled = true;
        camScript.player = player;
        camScript.rb = player.GetComponent<Rigidbody2D>();
        camScript.updatePlayers();
        //call rpc buffered so it runs even for players that join later
        //we have to call it on the photonview of the roomcontroller because that is the only one with namePlayer function
        CreatePrefab createScript = CreatePrefabs.GetComponent<CreatePrefab>();
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        createScript.renamePlayer(playerID, playerView.ViewID);
        createScript.changePlayer(player);
        displayIcons.GetComponent<displayPlayerStats>().addPlayerTexture(playerID);
        //add the player to all camera movement scripts
        photonView.RPC("updateCameraPlayers",RpcTarget.AllBuffered);
    }

    IEnumerator spawnWeapon()
    {
        
        //wait before spawning the weapon

        // if (spawnedWeapons.Count < maxWeaponCount)
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
               while (!(weapons.Length <= maxWeaponCount))
               {
                   yield return new WaitForSeconds(5);
               }

               int randomWeaponIndex = Random.Range(0, WeaponNames.Length);
               //find random spawn position
               int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
               //GameObject weaponPrefab = weaponPrefabs[randomWeaponIndex];
               //spaw weapon for all player and save it in a list
               GameObject weapon = PhotonNetwork.InstantiateRoomObject(WeaponNames[randomWeaponIndex],
                   spawnPoint[randomSpawnIndex],
                   Quaternion.identity, 0);

               spawnedWeapons.Add(weapon);
           }
           int randomWait = Random.Range(8, 15);
           yield return new WaitForSeconds(randomWait);
       }
            //select random weapon 
           // int randomWeaponIndex = Random.Range(0, weaponPrefabs.Length);
       
    }
/*
    IEnumerator dropWeapon(GameObject wep)
    {
        Rigidbody2D rb = wep.GetComponent<Rigidbody2D>();
        Collider2D coll = wep.GetComponent<Collider2D>();
        //enable gravity
        rb.simulated = true;
        //check if we are touching something
        yield return new WaitUntil(coll.IsTouchingLayers);
        //stop gravity
        rb.simulated = false;

    }
    
    [PunRPC] public void namePlayer(int gameobjectID, string nameID)
    {
        print("rename player");
        GameObject player = PhotonView.Find(gameobjectID).gameObject;
        player.name = nameID;
    }
*/
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

          //  Vector3 platformSize = coll.bounds.size;
            //find middle of the gameobject
            /*
            float middleX = platformTransform.position.x;
            float middleY = platformTransform.position.y;
            //min and max is half the size from the middle
            float minX = middleX - platformSize.x / 2;
            float maxX = middleX + platformSize.x / 2;
            
            float minY = middleY - platformSize.y / 2;
            float maxY = middleY + platformSize.y / 2;
*/
            foreach (var bounds in pathPoints)
            {
                int minX = (int)bounds.Item1.x;
                int maxX = (int)bounds.Item2.x;
                int maxY = (int)bounds.Item1.y;
                
                int spawnY = (int)maxY + 2;
                for (int x = (int)minX + 2; x < (int)maxX - 2; x++)
                {
                    //now we have increments of 1 on the length of the platform
                    Vector3 position = new Vector3(x, spawnY, 0);
                    //check if we are safe to spawn there by casting a raycast to the spawnPosition and checking if it collides with something
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.transform.position, position, out hit, Mathf.Infinity))
                    {
                        // Debug.DrawRay(Camera.main.transform.position, position * hit.distance, Color.yellow);
                        if (!hit.collider.gameObject.CompareTag("ground"))
                        {
                            //we did not hit ground 
                            spawnList.Add(position);
                        }
                        //we did hit something, dont spawn here
                    }
                    else
                    {
                        //we did not hit anything we are safe to spawn here
                        spawnList.Add(position);
                    }

                }
            }
        }
        //return found spawnPoints
        return spawnList.ToArray();
    }

    void getCreatedAssets()
    {
        //find assets and assign them
        GameObject player = GameObject.Find("Character");
        playerPrefab = player;
        player.SetActive(false);
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("weapon");
        foreach (var weapon in weapons)
        {
            //disable the weapons
            weapon.SetActive(false);
        }
        GameObject arena = GameObject.Find("Arena");
        arena.SetActive(false);
        weaponPrefabs = weapons;
        Console.WriteLine("Finished Fetching Assets");
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
            print(point.x);
            float xValue = point.x;
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
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.updatePlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.updatePlayers();
        
    }

    public void beforeStart(GameObject player)
    {
        playerMovement movement = player.GetComponent<playerMovement>();
        movement.enabled = false;
        
        RoomIDText.GetComponent<TextMeshProUGUI>().SetText(PhotonNetwork.CurrentRoom.Name);
        startGameButton.SetActive(true);
/*
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
            startGameButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                photonView.RPC("startGame",RpcTarget.All);
                PhotonNetwork.CurrentRoom.IsOpen = false;
            });
        }
        */
    }

    public void startGameButtonCallBack()
    {
        photonView.RPC("startGame",RpcTarget.All);
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }
    public void playAgain()
    {
        displayStats.SetActive(true);
        endGUI.SetActive(false);
        inGameUI.SetActive(true);
        
        int randomIndex = Random.Range(0, spawnPoint.Length);
        GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[randomIndex], Quaternion.identity);
        myPlayer = player;
        //disable some scripts before the start
        beforeStart(player);
        PhotonView playerView = player.GetPhotonView();
        //name him after the player so we can find him in onGUI
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.enabled = true;
        camScript.player = player;
        camScript.rb = player.GetComponent<Rigidbody2D>();
        //we have to call it on create prefabs because room controller is disabled at the start of the game
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(playerID, playerView.ViewID);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
        else
        {
            CreatePrefabs.GetComponent<CreatePrefab>().changePlayer(player);
            
        }
        CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(playerID, playerView.ViewID);
        photonView.RPC("updateCameraPlayers",RpcTarget.AllBuffered);
        displayIcons.GetComponent<displayPlayerStats>().addPlayerTexture(playerID);
    }
    
    [PunRPC]
    public void startGame()
    {
        //GameObject player = GameObject.Find(PhotonNetwork.LocalPlayer.UserId);
        myPlayer.GetComponent<playerMovement>().enabled = true;
        spawningEnabled = true;
        //now start the clock
        clock.GetComponent<countDown>().startCount();
        RoomIDText.SetActive(false);
        startGameButton.SetActive(false);
        //only the master will spawn the weapons, but we want to run it at all clients in case that one of them bbecomes master
        StartCoroutine(spawnWeapon());
        
    }
    public void endGame()
    {
        inGameUI.SetActive(false);
        startGameButton.SetActive(false);
        hideGUI = true;
        spawningEnabled = false;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("weapon");
        foreach (var weap in weapons)
        {
            Destroy(weap);
        }
        int playerCount = players.Length;
        endGUI.SetActive(true);
        showAllResults showResults = endController.GetComponent<showAllResults>();
        playerStats[] stats = new playerStats[playerCount];
        Dictionary<string, int> scores = new Dictionary<string, int>();
        Dictionary<string, int> placements = new Dictionary<string, int>();
        Dictionary<string, string> nicks = new Dictionary<string, string>();
        
        for (int i = 0; i < playerCount; i++)
        {
            stats[i] = players[i].GetComponent<playerStats>();
            scores.Add(players[i].name,stats[i].score);
            nicks.Add(PhotonNetwork.PlayerList[i].UserId,PhotonNetwork.PlayerList[i].NickName);
            players[i].SetActive(false);
        }
        var sortedDict = scores.OrderBy(pair => pair.Value).ToList();
        for (int i = 0; i < playerCount; i++)
        {
            placements.Add(sortedDict[i].Key,i+1);
        }
        showResults.showResults(players,placements,nicks);
        foreach (var player in players)
        {
            displayPlayerStats disStats = displayIcons.GetComponent<displayPlayerStats>();
            disStats.playerIDs.Clear();
            disStats.characterTextures.Clear();
            disStats.characterInfos.Clear();
            Destroy(player);
        }
        displayIcons.SetActive(false);
    }
}
