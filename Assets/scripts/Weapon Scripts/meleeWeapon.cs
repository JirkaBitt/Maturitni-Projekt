
using System.Collections;

using System.Diagnostics;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class meleeWeapon : weapon
{
   public abstract override void Use();
   private Vector3[] previousPositions;

   public override void launchEnemy(GameObject enemy, Vector3 launchVector, float force)
   {
      PhotonView weaponPhoton = gameObject.GetComponent<PhotonView>();
      PhotonView enemyPhoton = enemy.GetComponent<PhotonView>();
      //run the function on all enemy players
      weaponPhoton.RPC("addForce",RpcTarget.All,enemyPhoton.ViewID,launchVector,force);
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
      force *= randomMultiplier;
      stats.percentage += (int)(force/6  + force * stats.percentage/200);
      Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
      //add trail behind the enemy
      enemy.GetComponent<CreateTrail>().createTrail();
      rb.AddForce(launchVector * (force*10 + stats.percentage*2));
      //we want to remove gravity from the enemy as he is launched
      StartCoroutine(removeGravity(force + stats.percentage,enemy));
   }

   IEnumerator removeGravity(float totalForce,GameObject enemy)
   {
      Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
      PolygonCollider2D coll = enemy.GetComponent<PolygonCollider2D>();
      rb.gravityScale = 0;
      //only target non trigger objects
      ContactFilter2D filter2D = new ContactFilter2D();
      filter2D.useTriggers = false;
      //wait until we hit an object
      float waitTime = totalForce / 50;
      Stopwatch timer = new Stopwatch();
      timer.Start();
      //remove gravity from the launched enemy for set time or until he hits a solid object
      yield return new WaitUntil(() => coll.IsTouching(filter2D) || timer.Elapsed.Seconds > waitTime);
      timer.Stop();
      rb.gravityScale = 1;
   }
   
}
