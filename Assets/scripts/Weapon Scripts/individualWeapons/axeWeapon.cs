
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class axeWeapon : meleeWeapon
{
    private GameObject enemyInRange;
    private GameObject attackedEnemy;
    //trigger launch will be used to check if we called .Use() in oncollisionstay
    public override void Use()
    {
        //in here we want to attack other players
        //play animation and launch enemy if we touch them
        PhotonView photonView = gameObject.GetPhotonView();
        //we will set trigerlaunch to true in this coroutine
        photonView.RPC("playAnimation",RpcTarget.All);
        //we will switch triggerlaunch to false when animation ends in attack script
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
               
                if (triggerLaunch)
                {
                    Vector3 launchV = computeLaunchVector();
                    launchEnemy(enemyInRange, launchV, 40);
                    triggerLaunch = false;
                }

            }
        }
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
        Vector3 normal = new Vector3(vector.y * facingInt, vector.x * facingInt, 0);
        //make it jednotkovy vektor
        normal /= normal.magnitude;
        return normal;
    }

    IEnumerator waitForDamage()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        //we want to wait 0.2s bcs the axe is spinning from the back
        yield return new WaitForSeconds(0.2f);
        triggerLaunch = true;
        //wait for the end of the animation
        yield return new WaitUntil((() => animator.GetCurrentAnimatorStateInfo(0).IsName("picked")));
        triggerLaunch = false;
    }
    [PunRPC]
    void playAnimation()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("axeAttack");
        //create a trail behind the weapon as it moves
        addTrail(gameObject);
        StartCoroutine(waitForDamage());
    }
    

    
    
    
}
