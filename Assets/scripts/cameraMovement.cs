using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UIElements;

public class cameraMovement : MonoBehaviourPunCallbacks
{
    private GameObject[] players;
    public GameObject player;
    Camera mainCamera;

    public float cameraZoomDivider = 1.2f;
    public float cameraSpeed = 5f;
    // private CharacterController playerController;
   public Rigidbody2D rb;

    private Vector3 cameraStartPosition;

   
    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        /*
        foreach (var onePlayer in players)
        {
            //pixels have player tag as well so we only want the parents
            if (onePlayer.transform.parent == null)
            {
                if (onePlayer.GetComponent<PhotonView>().IsMine)
                {
                    player = onePlayer;
                }
            }
        }
        rb = player.GetComponent<Rigidbody2D>();
        */
        mainCamera = Camera.main;
        cameraStartPosition = mainCamera.transform.position;
        
    }

    

    public override void OnJoinedRoom()
    {
        //we have to wait before we instantiate the player
        players = returnPlayers();
        foreach (var onePlayer in players)
        {
            if (onePlayer.GetComponent<PhotonView>().IsMine)
            {
                player = onePlayer;
            }
        }
        rb = player.GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        cameraStartPosition = mainCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        //if x values are between 0 and 1 player is seen by the camera
        //print(player);
        Vector2 viewPos = mainCamera.WorldToViewportPoint(player.transform.position);

        //we want to move the camera about 0.2 from 0 or 1 so the player is still in sight
        if (viewPos.x < 0.4)
        {
            mainCamera.transform.position -= Vector3.right * cameraSpeed * Time.deltaTime;

        }
        if (viewPos.x > 0.6)
        {
            mainCamera.transform.position += Vector3.right * cameraSpeed * Time.deltaTime;

        }
        if (viewPos.y < 0.4)
        {
            //if we stop and are not in the middle then the camera stops as well because velocity is 0, so check if it is 0 and if it is then use cameraspeed
            float velocity = rb.velocity.y;
            float toPlayerDireetion = player.transform.position.y - mainCamera.transform.position.y;
            toPlayerDireetion /= Mathf.Abs(toPlayerDireetion);
            bool isStatic = (velocity < 0.1f && velocity > -0.1f);
            mainCamera.transform.position += Vector3.up * (isStatic ?  toPlayerDireetion * cameraSpeed:velocity) * Time.deltaTime;
        }
        if (viewPos.y > 0.6)
        {
            float velocity = rb.velocity.y;
            
            float toPlayerDireection = player.transform.position.y - mainCamera.transform.position.y;
            toPlayerDireection /= Mathf.Abs(toPlayerDireection);
            bool isStatic = (velocity < 0.1f && velocity > -0.1f);
            mainCamera.transform.position += Vector3.up * (isStatic ? toPlayerDireection * cameraSpeed:velocity) * Time.deltaTime;
        }

        //check if we are really far away
        if (Mathf.Abs(viewPos.y) > 1.5 || Mathf.Abs(viewPos.x) > 1.5)
        {
            //teleport the camera to the player
            Vector3 toPlayer = player.transform.position - mainCamera.transform.position;
            toPlayer.z = 0;
            mainCamera.transform.position += toPlayer;
        }
        //zoom in and out to capture all players
        updateCameraZoom();

    }

    GameObject[] returnPlayers()
    {
        //we dont want the pixels, only the parent
        List<GameObject> returnPlayers = new List<GameObject>();
        GameObject[] allGO = GameObject.FindGameObjectsWithTag("Player");
        foreach (var GO in allGO)
        {
           // if (GO.transform.parent == null)
          //  {
                returnPlayers.Add(GO);
          //  }
        }

        return returnPlayers.ToArray();
    }
    void updateCameraZoom()
    {
        players = returnPlayers();
        
        float cameraSize = 0;
       
        foreach (var onePlayer in players)
        {
           
                Vector3 difference = onePlayer.transform.position - player.transform.position;
                cameraSize += difference.magnitude;
            
        }
        //we want to divide cameraSize with playercount so it reflects an avarage value
        cameraSize = cameraSize / (players.Length * cameraZoomDivider);
        //5 is default value
        cameraSize += 5;
        //move the camera further from players to capture all players
        
        //we want to update only the z value but also allow the camera to move sideways
        mainCamera.orthographicSize = cameraSize;
    }
}
