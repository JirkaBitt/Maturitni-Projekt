using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class playerAttack : MonoBehaviour
{
    public bool isHoldingWeapon;
    public GameObject currentWeapon;
    public float attackForce = 400f;
    // 1->right, -1->left
    //receiving facing int from playerMovement
    public int facingInt;
    private bool alreadyAttacking = false;
    //alreadylaunching gets reset in coroutine at the end of the attack
    private bool alreadyLaunching = false;
    private axeAttack _axeAttack;
    
    // Start is called before the first frame update
    
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    { 
        
        if (isHoldingWeapon)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !alreadyAttacking)
            {
                print("play attack animation");
               // var Animator = currentWeapon.GetComponent<Animator>();
               //run weaponAnimation on network players as well
               int weaponID = currentWeapon.GetComponent<PhotonView>().ViewID;
               PhotonView photonView = PhotonView.Get(this);
               //we dont want it buffered because it would spin all the time when player joins
               photonView.RPC("weaponAnimation", RpcTarget.All, "Axe",weaponID);

               
               // Animator.Play("axe360");


            }
        }

        


    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //we only want to hit the local player, because he is the only one that can send transform to other clients, so only locl photonview
        if (col.gameObject.CompareTag("Player") && col.gameObject.GetComponent<PhotonView>().IsMine && col.gameObject != gameObject && alreadyAttacking && !alreadyLaunching)
        {
          // launchEnemy(col);
        }
    }
    

    private void OnTriggerStay2D(Collider2D other)
    {
        //check if we are not hitting our own player
        if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponent<PhotonView>().IsMine && other.gameObject != gameObject && alreadyAttacking && !alreadyLaunching)
        {
          // launchEnemy(other);

        }
    }

    void launchEnemy(Collider2D col)
    {
        alreadyLaunching = true;
        //we hit an enemy
        Rigidbody2D enemyRB = col.gameObject.GetComponent<Rigidbody2D>();
        //get the collision point of the weapon, we receive the closest point to weapon that collides with the enemy
        //Vector2 collisionPoint = col.ClosestPoint(transform.position);
      
        //make vector from player to weapon
        Vector3 launchVector =  currentWeapon.transform.position - gameObject.transform.position;
        //create normal vector, 90 degrees rotated launchvector, so that when we spin the weapon we get the impact vector
        //multiply both by facing int so it works for launching left as well
        //we can replace facingint with gameobject.transform.forward
        facingInt = (int)gameObject.transform.forward.z;
        Vector3 normalVector = new Vector3(launchVector.y * facingInt, launchVector.x * facingInt, 0);
            
        //launch the enemy
        enemyRB.AddForce(normalVector * attackForce);
        //PUN2_PlayerSync enemySyncScript = col.gameObject.GetComponent<PUN2_PlayerSync>();
        //send info about the launch to sync script
        //enemySyncScript.launchVector = normalVector * attackForce;

    }
    
    
    IEnumerator axeAnimation()
    {
        //already attacking so that we dont spam it
        alreadyAttacking = true;
        //send info to axeAttack
        _axeAttack = currentWeapon.GetComponent<axeAttack>();
        _axeAttack.attacking = true;
        //find weapon with photonId
        
        Transform weaponTransform = currentWeapon.transform;
        //weaponTransform.rotation = Quaternion.Euler(0, -180, -50);
        for (int i = 0; i < 40; i++)
        {
            //rotate 360 degrees
            //multiply it by facingInt so we update based on facing direction
            print(gameObject.transform.forward);
            //gameobject.transfor.forward changes the z axis based on direction 1 is right, -1 is left
            weaponTransform.RotateAround(gameObject.transform.position, gameObject.transform.forward, -9);
            //wait one frame
            yield return null;

        }
        
        //reset the weapon rotation if it has been glitching
      
        //we can attack again
        alreadyAttacking = false;
        //reset alreadylaunching
        alreadyLaunching = false;
        
       
        _axeAttack.attacking = false;
    }

    [PunRPC] public void weaponAnimation(string weapon, int weaponID)
    {
       
        
        GameObject weaponGameObject = PhotonView.Find(weaponID).gameObject;
        currentWeapon = weaponGameObject;
        
        StartCoroutine("axeAnimation");
        print(currentWeapon);
        print("received animation weapon");
    }
    
    

    

    
}
