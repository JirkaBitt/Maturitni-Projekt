using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class axeAttack : MonoBehaviour
{
    public float attackForce = 500f;

    public bool attacking = false;
    
    private Rigidbody2D enemyRB;

    private GameObject enemyInRange;

    public GameObject playerHoldingWeapon;

    private int facingInt;

   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (attacking && enemyInRange != null)
        {
            launchEnemy(enemyInRange);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        GameObject enemy = col.gameObject;
        //check if we hit player, if it isnt the player holding this weapon and if it is the original instance of enemy because only the original instance shares ts transform 
        if (enemy.CompareTag("Player") && enemy != playerHoldingWeapon && enemy.GetComponent<PhotonView>().IsMine)
        {
            enemyInRange = enemy;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //check if enemy is no longer in range
        if (other.gameObject == enemyInRange)
        {
            enemyInRange = null;
            
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject enemy = other.gameObject;
        //check if we hit player, if it isnt the player holding this weapon and if it is the original instance of enemy because only the original instance shares ts transform 
        if (enemy.CompareTag("Player") && enemy != playerHoldingWeapon && enemy.GetComponent<PhotonView>().IsMine)
        {
            enemyInRange = enemy;
        }
    }
    void launchEnemy(GameObject col)
    {
        //set attacking to false so that we dont launch repeatadly
        attacking = false;
        //we hit an enemy
        Rigidbody2D enemyRB = col.GetComponent<Rigidbody2D>();
        //get the collision point of the weapon, we receive the closest point to weapon that collides with the enemy
        //Vector2 collisionPoint = col.ClosestPoint(transform.position);
      
        //make vector from player to weapon
        Vector3 launchVector =  gameObject.transform.position - playerHoldingWeapon.transform.position;
        //create normal vector, 90 degrees rotated launchvector, so that when we spin the weapon we get the impact vector
        //multiply both by facing int so it works for launching left as well
        //we can replace facingint with gameobject.transform.forward
        facingInt = (int)gameObject.transform.forward.z;
        Vector3 normalVector = new Vector3(launchVector.y * facingInt, launchVector.x * facingInt, 0); 
        //launch the enemy
        enemyRB.AddForce(normalVector * attackForce);
       

    }
}
