using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class swordWeapon : meleeWeapon
{
    private GameObject enemyInRange;
    private GameObject attackedEnemy;
   
    // Start is called before the first frame update
    public override void Use()
    {
        //attack
        PhotonView photonView = gameObject.GetPhotonView();
        photonView.RPC("playAnimation",RpcTarget.All);
    }
    Vector3 computeLaunchVector()
    {
        GameObject player = gameObject.transform.parent?.gameObject;
        //get vector from player to weapon, we could use the collision point instead
        Vector3 vector = gameObject.transform.position - player.transform.position;
        float facingInt = player.transform.forward.z;
        //if we are facing right we want to multiply x*1 and  y*(-1)
        //if left then x*(-1) y*1
        //this is for when we are hitting with the axe from below
        Vector3 normal = new Vector3(vector.y * -facingInt , vector.x * facingInt, 0);
        //make it jednotkovy vektor
        normal = normal / normal.magnitude;
        normal += transform.right;
        print(normal);
        return normal;
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        //fetch player holding the weapon to check that we are not hitting him
        GameObject playerHoldingThisWepon = gameObject.transform.parent?.gameObject;
        //check if we are picked up
        if (playerHoldingThisWepon != null)
        {
            //check if we are in range of an an enemy
            GameObject possibleEnemy = other.gameObject;

            if (possibleEnemy.CompareTag("Player") && possibleEnemy != playerHoldingThisWepon)
            {
                //we have confirmed that this is an enemy
                enemyInRange = possibleEnemy;
                
                //just for testing
                if (triggerLaunch)
                {
                    print("launch enemy");
                    Vector3 launchV = computeLaunchVector();
                    this.launchEnemy(enemyInRange, launchV, 40);
                    triggerLaunch = false;
                }

            }
        }
    }
    [PunRPC]
    void playAnimation()
    {
        triggerLaunch = true;
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("swordAttack");
        addTrail(gameObject);
    }
}
