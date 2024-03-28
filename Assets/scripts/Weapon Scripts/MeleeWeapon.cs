
using System.Collections;

using System.Diagnostics;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class MeleeWeapon : Weapon
{
   public abstract override void Use();

   public override void LaunchEnemy(GameObject enemy, Vector3 launchVector, float force)
   {
      PhotonView weaponPhoton = gameObject.GetComponent<PhotonView>();
      PhotonView enemyPhoton = enemy.GetComponent<PhotonView>();
      //run the function on all enemy players
      float RandomMult = Random.Range(5, 10) / 10f;
      weaponPhoton.RPC("AddForce",RpcTarget.All,enemyPhoton.ViewID,launchVector,force * RandomMult);
   }

   [PunRPC]public override void AddForce(int photonViewID, Vector3 launchVector, float force)
   {
      // we will run this script on all instances of this Weapon, so in the instance of the enemy the Weapon will launch him
      PhotonView phView = PhotonView.Find(photonViewID);
      GameObject enemy = phView.gameObject;
      PlayerSync playerSync = enemy.GetComponent<PlayerSync>();
      //disable the sync for 0.6s
      playerSync.LaunchEnemy();
      //add the force to his percentage and launch him with his percentage
      PlayerStats stats = enemy.GetComponent<PlayerStats>();
      //set our player as the last attacker
      stats.lastAttacker = gameObject.transform.parent.gameObject;
      stats.percentage += (int)(force/6  + force * stats.percentage/200);
      Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
      //add trail behind the enemy
      enemy.GetComponent<CreateTrail>().ShowTrail();
      rb.AddForce(launchVector * (force*10 + stats.percentage*2));
      //we want to remove gravity from the enemy as he is launched
      StartCoroutine(RemoveGravity(force + stats.percentage,enemy));
   }

   IEnumerator RemoveGravity(float totalForce,GameObject enemy)
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
