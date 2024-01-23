using System;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
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
    public GUIStyle percentageStyle;
    private List<GameObject> spawnedWeapons = new List<GameObject>();
    public playerStats[] playerStats;

    public string[] WeaponNames;

    public GameObject CreatePrefabs;
    //private List<playerStats> _statsList = new List<playerStats>();
    // Use this for initialization
    void Start()
    {
       
        //set the default fps to 60
        Application.targetFrameRate = 60;
        //In case we started this demo with the wrong scene being active, simply load the menu scene
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("Is not in the room, returning back to Lobby");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
            Application.Quit();
        }
        else
        {
            spawnPoint = findRespawnPositions();
            if (PhotonNetwork.IsMasterClient)
            {
                //fetch created assets
               // getCreatedAssets();
                //spawn character, this is only for the room creator, others are created in OnJoinedRoom
                print("instantiate from start");
                //GameObject arena = PhotonNetwork.Instantiate("Arena", Vector3.zero, Quaternion.identity);
                GameObject player = PhotonNetwork.Instantiate("Character", spawnPoint[0], Quaternion.identity);
                PhotonView playerView = player.GetPhotonView();
                //name him after the player so we can find him in onGUI
                cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
                camScript.enabled = true;
                camScript.player = player;
                camScript.rb = player.GetComponent<Rigidbody2D>();
                //we have to call it on create prefabs because room controller is disabled at the start of the game
                CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(PhotonNetwork.LocalPlayer.UserId, playerView.ViewID);
                //call rpc buffered so it runs even for players that join later
                //we have to call it on the photonview of the roomcontroller because that is the only one with namePlayer function
                //photonView.RPC("namePlayer",RpcTarget.AllBuffered,playerView.ViewID,PhotonNetwork.LocalPlayer.UserId);
                
            }
        }
    }

    private void Update()
    {
        //only the master client will spawn weapons
        if (canSpawnWeapon && PhotonNetwork.IsMasterClient && spawnedWeapons.Count <= maxWeaponCount)
        {
            StartCoroutine(spawnWeapon());
        }
    }

    void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        //Leave this Room
        if (GUI.Button(new Rect(5, 5, 125, 25), "Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }

        //Show the Room name
        GUI.Label(new Rect(135, 5, 200, 25), PhotonNetwork.CurrentRoom.Name);

      
        //Show the list of the players connected to this Room
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            //Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
            string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? ": MasterClient" : "");
            GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].NickName + isMasterClient);

           
            //we want to show percentages of damage taken in the bottom corner
            percentageStyle.fontSize = 40;
            percentageStyle.normal.textColor = Color.white;
            //find the stats with userId
            playerStats stats = GameObject.Find(PhotonNetwork.PlayerList[i].UserId).GetComponent<playerStats>();
            GUI.Label(new Rect(60 + i * 200, Screen.height - 180, 100, 50), PhotonNetwork.PlayerList[i].NickName + "\n" + stats.percentage+" %" + "\n" + "Knockouts: " + stats.Knockouts, percentageStyle);
        }
    }
  
    public override void OnLeftRoom()
    {
        //We have left the Room, return back to the GameLobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
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
        camScript.updatePlayers();
        //call rpc buffered so it runs even for players that join later
        //we have to call it on the photonview of the roomcontroller because that is the only one with namePlayer function
        CreatePrefabs.GetComponent<CreatePrefab>().renamePlayer(PhotonNetwork.LocalPlayer.UserId, playerView.ViewID);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.updatePlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        cameraMovement camScript = Camera.main.GetComponent<cameraMovement>();
        camScript.updatePlayers();
    }

    IEnumerator spawnWeapon()
    {
        canSpawnWeapon = false;
        //wait before spawning the weapon
        var randomWait = Random.Range(8, 15);
        yield return new WaitForSeconds(randomWait);
        
       // if (spawnedWeapons.Count < maxWeaponCount)
        
            //select random weapon 
           // int randomWeaponIndex = Random.Range(0, weaponPrefabs.Length);
           int randomWeaponIndex = Random.Range(0, WeaponNames.Length);
            //find random spawn position
            int randomSpawnIndex = Random.Range(0, spawnPoint.Length);
            //GameObject weaponPrefab = weaponPrefabs[randomWeaponIndex];
            //spaw weapon for all player and save it in a list
            GameObject weapon = PhotonNetwork.InstantiateRoomObject(WeaponNames[randomWeaponIndex], spawnPoint[randomSpawnIndex],
                Quaternion.identity, 0);
           
            spawnedWeapons.Add(weapon);
        

        canSpawnWeapon = true;
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
}
