using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Sword : MeleeWeapon
{
    private GameObject enemyInRange;
    private GameObject attackedEnemy;
    public override void Use()
    {
        //attack
        PhotonView photonView = gameObject.GetPhotonView();
        photonView.RPC("playAnimation",RpcTarget.All);
    }
    Vector3 computeLaunchVector()
    {
        GameObject player = gameObject.transform.parent?.gameObject;
        //get vector from player to Weapon, we could use the collision point instead
        Vector3 vector = gameObject.transform.position - player.transform.position;
        float facingInt = player.transform.forward.z;
        //if we are facing right we want to multiply x*1 and  y*(-1)
        //if left then x*(-1) y*1
        //this is for when we are hitting with the axe from below
        if ((int)player.GetComponent<Rigidbody2D>().velocity.y != 0)
        {
            //player is in the air so he can launch with the sword down
            Vector3 normal = new Vector3(vector.y * -facingInt , vector.x * facingInt, 0);
            //make it jednotkovy vektor
            normal += facingInt * new Vector3(1,0,0);
            normal /= normal.magnitude;
            return normal;
        }
        //we are on the ground so launch him up
        return new Vector3(facingInt * 0.8f, 0.2f,0);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        //fetch player holding the Weapon to check that we are not hitting him
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
                if (triggerLaunch)
                {
                    print("launch enemy");
                    Vector3 launchV = computeLaunchVector();
                    launchEnemy(enemyInRange, launchV, 40);
                    triggerLaunch = false;
                }
            }
        }
    }
    IEnumerator waitForAnimationEnd()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        yield return new WaitUntil((() => animator.GetCurrentAnimatorStateInfo(0).IsName("picked")));
        triggerLaunch = false;
    }
    [PunRPC]
    void playAnimation()
    { 
        triggerLaunch = true;
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("swordAttack");
        addTrail(gameObject);
        StartCoroutine(waitForAnimationEnd());
    }
}
