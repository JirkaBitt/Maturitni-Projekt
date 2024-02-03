using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class bomb : consumableWeapons
{
    // Start is called before the first frame update
    public GameObject explosionPrefab;
    private float gravityValue = 10;
    private bool exploded = false;
    public override void Use()
    {
        //throw the weapon, this script is in consumableWeapon
       PhotonView.Get(this).RPC("useRPC",RpcTarget.All);

    }

    [PunRPC]public void useRPC()
    {
        GameObject player = transform.parent.gameObject;
        throwWeapon(200);
        addTrail(gameObject);
        //update all scripts
        playerStats stats = player.GetComponent<playerStats>();
        stats.currentWeapon = null;
        pickWeapon pickscript = player.GetComponent<pickWeapon>();
        pickscript.isHoldingWeapon = false;
        //detonate the weapon
        StartCoroutine(blowUp(3));
    }
    IEnumerator blowUp(int timeDetonate)
    {
        yield return new WaitForSeconds(timeDetonate);
        
        Collider2D coll = gameObject.GetComponent<Collider2D>();
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();

        //stop gravity, stop motion and set collider to trigger and make it bigger so we know what enemies are in range
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        //set exploded to true so we check it in ontrigger enter
        exploded = true;
        coll.isTrigger = true;
        //coll.radius = 4;
        //check for players in range of explosion, this creates a circle and returns colliders that are within that circle
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(gameObject.transform.position, 5);
        foreach (var hitted in hitColliders)
        {
            
            GameObject player = hitted.gameObject;
            print(player.name);
            if (player.CompareTag("Player"))
            {
                launchEnemy(player, computeVector(player), 100);
            }
        }
        
        if (PhotonView.Get(this).IsMine)
        {
           GameObject explode = PhotonNetwork.Instantiate(explosionPrefab.name,gameObject.transform.position,Quaternion.identity);
           //destroy the bomb

           //make the bomb invisible, we cannot destroy it because the script is tied to it, we will destroy it with the explosion
           SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
           renderer.enabled = false;

           //wait before deleting the explosion
           StartCoroutine(waitBeforeDeletion(explode));
           
        }
        
        
    }

    IEnumerator waitBeforeDeletion(GameObject deleteThis)
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.Destroy(deleteThis);
        
        //we have to destroy this gameobject as last one because it will delete this script as well
        PhotonNetwork.Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("ground"))
        {
            //we hit the ground, stop the motion
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
                rb.velocity = Vector2.zero;
            }
        }
    }
/*
    private void OnTriggerEnter2D(Collider2D col)
    {
        print("collisionEnter " + col.name);
        //check if we hit someone with the explosion
        if (exploded)
        {
            print("exploded" + col.name);
            GameObject player = col.gameObject;
            if (player.CompareTag("Player"))
            {
                print("got hit by bomb");
                //we hit a player so launch him
                launchEnemy(player,computeVector(player),300);
            }
        }
    }

*/
    Vector3 computeVector(GameObject enemy)
    {
        Vector3 vector = enemy.transform.position - gameObject.transform.position;
        //jednotkovy vektor
        vector = vector / vector.magnitude;
        return vector;
    }

   
}
