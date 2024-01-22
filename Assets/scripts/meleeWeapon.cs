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

      //add the force to his percentage and launch him with his percentage
      playerStats stats = enemy.GetComponent<playerStats>();
      //add a litle bit of randomness
      float randomMultiplier = Random.Range(4, 10);
      randomMultiplier /= 10;
      force = force * randomMultiplier;
      print(force);
      stats.percentage += (int)(force/6  + force * stats.percentage/200);
      
      Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
   
      rb.AddForce(launchVector * (force*6 + stats.percentage*2));
   }

   IEnumerator trackPreviousPositions()
   {
      bool play = true;
      //tracked is for knowning if we waited enough and are tracking all 5 positions
      int tracked = 0;
      while (play)
      {
         //we need this placeholder to save the information of previous positions to move them up
         Vector3[] placeHolders = previousPositions;
         if (tracked < 5)
         {
            //wait two frames
            yield return null;
            yield return null;
            previousPositions[tracked] = weap.transform.position;
            tracked++;
         }
         //track the new position and reassign the previous ones
         previousPositions[0] = weap.transform.position;
         for (int i = 1; i < 5; i++)
         {
            previousPositions[i] = previousPositions[i - 1];
         }
      }
   }
}
