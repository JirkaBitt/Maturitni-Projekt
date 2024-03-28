using System;
using System.Collections;

using Photon.Pun;
using UnityEngine;


public class Spear : MeleeWeapon
{
    private GameObject enemyInRange;
    private GameObject attackedEnemy;
    //trigger launch will be used to check if we called .Use() in oncollisionstay
    private float forceDefault = 20;
    private float force = 20;
    private float maxForce = 60;
    public override void Use()
    {
        StartCoroutine(ChargeSpear());
    }
    Vector3 ComputeLaunchVector()
    {
        //get vector from player to Weapon, we could use the collision point instead
        Vector3 vector = enemyInRange.transform.position - gameObject.transform.position;
        vector /= vector.magnitude;
        //we want to launch him a little bit in the air as well
        vector.y += 0.2f;
        return vector;
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
                    Vector3 launchV = ComputeLaunchVector();
                    //check if we are hitting the player from the front of the spear and not the back of it
                    int launchDirection = Mathf.RoundToInt(launchV.x / Mathf.Abs(launchV.x));
                    if (Math.Abs(launchDirection - playerHoldingThisWepon.transform.right.x) < 0.1f)
                    {
                        LaunchEnemy(enemyInRange, launchV, force);
                        triggerLaunch = false;
                    }
                    //reset force
                    force = forceDefault;
                }
            }
        }
    }
    IEnumerator ChargeSpear()
    {
        bool hasReleased = false;
        GameObject player = gameObject.transform.parent.gameObject;
        PhotonView photonView = gameObject.GetPhotonView();
        //play the animation
        photonView.RPC("PlayAnimation",RpcTarget.All);
        yield return new WaitForSeconds(0.5f);
        //we want to launch after the 30 frames
        photonView.RPC("StopAnimation",RpcTarget.All);
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
        photonView.RPC("ResumeAnimation",RpcTarget.All);
        triggerLaunch = true;
        //move the player forward with the spear
        player.GetComponent<Rigidbody2D>().AddForce(player.transform.right * force*4);
    }

    [PunRPC]
    void PlayAnimation()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("chargeSpear");
        AddTrail(gameObject);
    }

    [PunRPC]
    void StopAnimation()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.speed = 0;
    }

    [PunRPC]
    void ResumeAnimation()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.speed = 1;
        //create the trail behind the spear as it charches forward
        AddTrail(gameObject);
        if (gameObject.transform.parent.gameObject.GetPhotonView().IsMine)
        {
            StartCoroutine(WaitForAnimationEnd());
        }
    }
    IEnumerator WaitForAnimationEnd()
    {
        triggerLaunch = true;
        Animator animator = gameObject.GetComponent<Animator>();
        yield return new WaitUntil((() => animator.GetCurrentAnimatorStateInfo(0).IsName("picked")));
        triggerLaunch = false;
    }
    
}
