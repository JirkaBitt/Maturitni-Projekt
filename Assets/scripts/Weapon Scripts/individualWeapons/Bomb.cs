
using System.Collections;

using Photon.Pun;
using UnityEngine;

public class Bomb : ConsumableWeaponses
{
    public GameObject explosionPrefab;
    public override void Use()
    {
        //throw the Weapon, this script is in consumableWeapon
       PhotonView.Get(this).RPC("useRPC",RpcTarget.All);
    }
    [PunRPC]public void useRPC()
    {
        GameObject player = transform.parent.gameObject;
        throwWeapon(200);
        addTrail(gameObject);
        //update all scripts
        PlayerStats stats = player.GetComponent<PlayerStats>();
        stats.currentWeapon = null;
        PickWeapon pickscript = player.GetComponent<PickWeapon>();
        pickscript.isHoldingWeapon = false;
        pickscript.currentWeapon = null;
        pickscript.deleteLifeBar();
        //detonate the Weapon
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
       
        coll.isTrigger = true;
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
           //make the Bomb invisible, we cannot destroy it because the script is tied to it, we will destroy it with the explosion
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
    Vector3 computeVector(GameObject enemy)
    {
        Vector3 vector = enemy.transform.position - gameObject.transform.position;
        //jednotkovy vektor
        vector /= vector.magnitude;
        return vector;
    }

   
}