using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class GunWeapon : Weapon
{
    public abstract override void Use();
    public override void launchEnemy(GameObject enemy, Vector3 launchVector, float force)
    {
        //we want to have the function here so we dont have to write it for every Weapon type
        PhotonView weaponPhoton = gameObject.GetComponent<PhotonView>();
        PhotonView enemyPhoton = enemy.GetComponent<PhotonView>();
        //run the function on all enemy players
        weaponPhoton.RPC("addForce",RpcTarget.All,enemyPhoton.ViewID,launchVector,force);
    }

    [PunRPC] public override void addForce(int photonViewID, Vector3 launchVector, float force)
    {
        // we will run this script on all instances of this Weapon, so in the instance of the enemy the Weapon will launch him
        PhotonView phView = PhotonView.Find(photonViewID);
        GameObject enemy = phView.gameObject;
        //add the force to his percentage and launch him with his percentage
        PlayerStats stats = enemy.GetComponent<PlayerStats>();
        //add a litle bit of randomness
        float randomMultiplier = Random.Range(4, 10);
        randomMultiplier /= 10;
        force *= randomMultiplier;
        stats.percentage += (int)(force/10  + force * stats.percentage/200);
        stats.lastAttacker = gameObject.transform.parent.gameObject;
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        rb.AddForce(launchVector * (force*6 + stats.percentage*2));
        enemy.GetComponent<CreateTrail>().createTrail();
    }
}