using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class bulletScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 launchVector = Vector3.zero;
    private bool alreadyAdded = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (launchVector != Vector3.zero && !alreadyAdded)
        {
            //release the bullet
            // transform.position += launchVector * Time.deltaTime;
            //we have to do it throw rb becuse otherwise it doesnt register on triggerstay for the ground
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            
            rb.gravityScale = 0;
            rb.AddForce(launchVector * 200);

            alreadyAdded = true;
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        //check if we have shot the gun
        print(col.gameObject.name);
        if (launchVector != Vector3.zero)
        {
            GameObject hit = col.gameObject;
           
            if (hit.CompareTag("Player"))
            {
                //hit the player
                PhotonView bulletPhotonView = gameObject.GetPhotonView();
                if (bulletPhotonView.IsMine)
                {
                    PhotonView enemyPhotonView = hit.GetPhotonView();
                    bulletPhotonView.RPC("addForceBullet", RpcTarget.All, enemyPhotonView.ViewID, launchVector, 80f);
                    
                    PhotonNetwork.Destroy(gameObject);
                }
            }

            if (hit.CompareTag("ground"))
            {
                //we hit the ground, delete the bullet
                PhotonView photonView = gameObject.GetPhotonView();
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
    
    [PunRPC] public void addForceBullet(int photonViewID, Vector3 launchVector, float force)
    {
        // we will run this script on all instances of this weapon, so in the instance of the enemy the weapon will launch him
        PhotonView phView = PhotonView.Find(photonViewID);
     
        GameObject enemy = phView.gameObject;

        //add the force to his percentage and launch him with his percentage
        playerStats stats = enemy.GetComponent<playerStats>();
        //add a litle bit of randomness
        float randomMultiplier = Random.Range(4, 10);
        randomMultiplier /= 10;
        force = force * randomMultiplier;
        print(force);
        stats.percentage += (int)(force/10  + force * stats.percentage/200);
      
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
   
        rb.AddForce(launchVector * (force + stats.percentage*2));
      

    }
}