using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class checkPlayerBounds : MonoBehaviourPunCallbacks
{
    private PUN2_RoomController controller;
    public Vector3[] respawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("_RoomController").GetComponent<PUN2_RoomController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject gObject = other.gameObject;
        //PhotonView photonView = gameObject.GetComponent<PhotonView>();
        //we can only delete the player from isMine
        if (gObject.CompareTag("Player")){
            
            playerStats stats = gObject.GetComponent<playerStats>();
            stats.percentage = 0;
            stats.Knockouts += 1;
            GameObject weapon = stats.currentWeapon;
            //delete weapon if player is holding it
            if (weapon != null)
            {
                //pickWeapon pickScript = other.GetComponent<pickWeapon>();
                int photonID = weapon.GetPhotonView().ViewID;
                //drop the weapon and then destroy it
                //pickScript.dropWeapon(photonID);
                //PhotonNetwork.Destroy(weapon);
                photonView.RPC("deleteWeapon",RpcTarget.All,photonID,gObject.GetPhotonView().ViewID);
            }

            int randomIndex = Random.Range(0, controller.spawnPoint.Length);
            Vector3 selectedPoint = controller.spawnPoint[randomIndex];
            gObject.transform.position = selectedPoint;
            //reset the velocity
            gObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //move camera with the player
            Camera.main.transform.position =
                new Vector3(selectedPoint.x, selectedPoint.y, Camera.main.transform.position.z);
        }
        else
        {
            print("Destroy" + gObject.name);
            //check if we are not deleting weapons, it is messing with bombs
            if (!gObject.CompareTag("weapon"))
            {
                Destroy(gObject);
            }
        }
    }

    [PunRPC]
    public void deleteWeapon(int weaponID, int playerID)
    {
        GameObject player = PhotonView.Find(playerID).gameObject;
        GameObject weapon = PhotonView.Find(weaponID).gameObject;
        
        pickWeapon pickScript = player.GetComponent<pickWeapon>();
      //drop the weapon on all instances
        pickScript.dropWeapon(weaponID);
        //delete the weapon if we are the owner
        if (weapon.GetPhotonView().IsMine)
        {
            PhotonNetwork.Destroy(weapon);
        }
    }
}
