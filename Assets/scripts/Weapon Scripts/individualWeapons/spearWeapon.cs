using System;
using System.Collections;

using Photon.Pun;
using UnityEngine;


public class spearWeapon : meleeWeapon
{
    private GameObject enemyInRange;
    private GameObject attackedEnemy;
    //trigger launch will be used to check if we called .Use() in oncollisionstay
    private float forceDefault = 20;
    private float force = 20;
    private float maxForce = 60;
    public override void Use()
    {
        StartCoroutine(chargeSpear());
    }
    Vector3 computeLaunchVector()
    {
        //get vector from player to weapon, we could use the collision point instead
        Vector3 vector = enemyInRange.transform.position - gameObject.transform.position;
        vector /= vector.magnitude;
        //we want to launch him a little bit in the air as well
        vector.y += 0.2f;
        return vector;
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
                    //check if we are hitting the player from the front of the spear and not the back of it
                    int launchDirection = Mathf.RoundToInt(launchV.x / Mathf.Abs(launchV.x));
                    if (Math.Abs(launchDirection - playerHoldingThisWepon.transform.right.x) < 0.1f)
                    {
                        launchEnemy(enemyInRange, launchV, force);
                        triggerLaunch = false;
                    }
                    //reset force
                    force = forceDefault;
                }
            }
        }
    }
    IEnumerator chargeSpear()
    {
        bool hasReleased = false;
        GameObject player = gameObject.transform.parent.gameObject;
        PhotonView photonView = gameObject.GetPhotonView();
        //play the animation
        photonView.RPC("playAnimation",RpcTarget.All);
        yield return new WaitForSeconds(0.5f);
        //we want to launch after the 30 frames
        photonView.RPC("stopAnimation",RpcTarget.All);
        while (Input.GetMouseButton(0) && !hasReleased)
        {
            //player is charging the spear
            if (force < maxForce)
            {
                //Time.DeltaTime is a  very small value
                force += 0.8f * Time.deltaTime * 400;
            }
            yield return null;
        }
        //resume animation, we have stopped charging and want to release the spear
        photonView.RPC("resumeAnimation",RpcTarget.All);
        triggerLaunch = true;
        //move the player forward with the spear
        player.GetComponent<Rigidbody2D>().AddForce(player.transform.right * force*4);
    }

    [PunRPC]
    void playAnimation()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("chargeSpear");
        addTrail(gameObject);
    }

    [PunRPC]
    void stopAnimation()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.speed = 0;
    }

    [PunRPC]
    void resumeAnimation()
    {
        triggerLaunch = true;
        Animator animator = gameObject.GetComponent<Animator>();
        animator.speed = 1;
        //create the trail behind the spear as it charches forward
        addTrail(gameObject);
        StartCoroutine(waitForAnimationEnd());
    }
    IEnumerator waitForAnimationEnd()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        yield return new WaitUntil((() => animator.GetCurrentAnimatorStateInfo(0).IsName("picked")));
        triggerLaunch = false;
    }
    
}
