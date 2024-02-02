using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class meleeWeapon : weapon
{
   public abstract override void Use();
   public GameObject weap;
   private Vector3[] previousPositions;

   public override void launchEnemy(GameObject enemy, Vector3 launchVector, float force)
   {
      //we want to have the function here so we dont have to write it for every weapon type
      PhotonView weaponPhoton = gameObject.GetComponent<PhotonView>();
      PhotonView enemyPhoton = enemy.GetComponent<PhotonView>();
      //run the function on all enemy players
      weaponPhoton.RPC("addForce",RpcTarget.All,enemyPhoton.ViewID,launchVector,force);
      //we can add an animation of the player being launched
      
   }

   [PunRPC]public override void addForce(int photonViewID, Vector3 launchVector, float force)
   {
      // we will run this script on all instances of this weapon, so in the instance of the enemy the weapon will launch him
      PhotonView phView = PhotonView.Find(photonViewID);
     
      GameObject enemy = phView.gameObject;
      PUN2_PlayerSync playerSync = enemy.GetComponent<PUN2_PlayerSync>();
      //disable the sync for 0.6s
      playerSync.launchEnemy();
      //add the force to his percentage and launch him with his percentage
      playerStats stats = enemy.GetComponent<playerStats>();
      //set our player as the last attacker
      stats.lastAttacker = gameObject.transform.parent.gameObject;
      //add a litle bit of randomness
      float randomMultiplier = Random.Range(4, 10);
      randomMultiplier /= 10;
      force = force * randomMultiplier;
      
      stats.percentage += (int)(force/6  + force * stats.percentage/200);
      
      Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
      //add trail behind the enemy
      enemy.GetComponent<CreateTrail>().createTrail();
      rb.AddForce(launchVector * (force*10 + stats.percentage*2));
   }
   
}
