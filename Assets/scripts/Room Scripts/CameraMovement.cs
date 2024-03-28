using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviourPunCallbacks
{
    //all the players in the room
    public GameObject[] players;
    //my player
    public GameObject player;
    Camera mainCamera;
    //the current zoom, it adjusts as players are further apart
    public float cameraZoomDivider = 1.2f;
    //movement speed of the camera
    public float cameraSpeed = 5f;
    //the rigidbody of my player
    public Rigidbody2D rb;
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        mainCamera = Camera.main;
        //move camera to player
        Vector2 toPlayer = player.transform.position - mainCamera.transform.position;
        mainCamera.transform.position += (Vector3)toPlayer;

    }
    // Update is called once per frame
    void Update()
    {
        //if x values are between 0 and 1 then the player is seen by the camera
        if (player == null)
        {
            return;
        }
        Vector2 viewPos = mainCamera.WorldToViewportPoint(player.transform.position);
        //we want to move the camera about 0.4 from 0 or 1 so the player is still in sight
        //we dont want to copy the position of the player, it does not look good, this way it created an effect that the player is pushing the  camera along
        if (viewPos.x < 0.4)
        {
            mainCamera.transform.position -= Vector3.right * cameraSpeed * Time.deltaTime;

        }
        if (viewPos.x > 0.6)
        {
            mainCamera.transform.position += Vector3.right * cameraSpeed * Time.deltaTime;

        }
        if (viewPos.y < 0.4 || viewPos.y > 0.6)
        {
            //if we stop and are not in the middle then the camera stops as well because velocity is 0, so check if it is 0 and if it is then use cameraspeed
            float velocity = rb.velocity.y;
            float toPlayerDirection = player.transform.position.y - mainCamera.transform.position.y;
            toPlayerDirection /= Mathf.Abs(toPlayerDirection);
            bool isStatic = (velocity < 0.1f && velocity > -0.1f);
            mainCamera.transform.position += Vector3.up * (isStatic ? toPlayerDirection * cameraSpeed:velocity) * Time.deltaTime;
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
        UpdateCameraZoom();
    }

    public void  UpdatePlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }
    void UpdateCameraZoom()
    {
        float cameraSize = 0;
        try
        {
             foreach (var onePlayer in players)
             {
                Vector3 difference = onePlayer.transform.position - player.transform.position;
                cameraSize += difference.magnitude;
             }
        }
        catch (Exception e)
        {
            //some player could have left the game via crash or something else, so we have to have catch and update the players in there
            UpdatePlayers();
            Console.WriteLine(e);
            throw;
        }
       
        //we want to divide cameraSize with playercount so it reflects an avarage value
        cameraSize /= ((players.Length) * cameraZoomDivider);
        //5 is default value
        cameraSize += 5;
        //this zooms out the camera
        mainCamera.orthographicSize = cameraSize;
    }
}
