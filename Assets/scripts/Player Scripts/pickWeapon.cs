using System.Collections;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;

public class pickWeapon : MonoBehaviour
{
    //check if we are in range of an weapon to pick it up
    public bool isInRange = false;
    //refrence to the weapon that is in our reach
    public GameObject weaponInRange;
    //if our player is already holding a weapon
    public bool isHoldingWeapon = false;
    //the script to use the weapon
    private weapon weaponScript;
    //keep track if we have already clicked attack to prevent spamming
    private bool alreadyUsing = false;
    //the lifebar of the picked weapon
    private GameObject lifeBar;
    //if we decide to pickup the weapon it is stored here
    public GameObject currentWeapon;
    void Update()
    {
            if (Input.GetKeyDown(KeyCode.E) && isHoldingWeapon)
            {
                 //drop the weapon
                 drop(false, currentWeapon);
                 return;
            }
            //ontriggerenter changes the isInRange and we only have to check it here
            if (isInRange)
            {
                if (Input.GetKeyDown(KeyCode.E) && !isHoldingWeapon)
                {
                    //we have to use ids because we are sending them over the network
                    int pickedWeaponID = weaponInRange.GetComponent<PhotonView>().ViewID;
                    int currentPlayerID = gameObject.GetComponent<PhotonView>().ViewID;
                    PhotonView photonView = PhotonView.Get(this);
                    //we cannot send gameobject in RPC so we have to use photonviewID
                    //it does not need to be buffered bcs all players are in room
                    photonView.RPC("AssignPlayerWeapon", RpcTarget.All, currentPlayerID, pickedWeaponID);
                }

            }
            if (isHoldingWeapon && Input.GetMouseButtonDown(0) && !alreadyUsing)
            {
                alreadyUsing = true;
                weaponScript.Use();
                //waitForIsUsing prevents the player from spamming attacks
                StartCoroutine(waitForIsUsing());

            }
    }
    //PunRPC mark is used that we can send this script over the network for other instances to call 
    [PunRPC] public void AssignPlayerWeapon(int playerID, int weaponID)
    {
        //Find the Gameobjects based on ID
        isHoldingWeapon = true;
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        GameObject playerX = PhotonView.Find(playerID).gameObject;
        //retrieve the weapon script
        weaponScript = weaponX.GetComponent<weapon>();
        //stop playing the idle animation
        Animator animator = weaponX.GetComponent<Animator>();
        animator.SetBool("isPicked", true);
        //assign player as parent so that weapon moves with him
        float facingDirection = gameObject.transform.forward.z;
        weaponX.transform.parent = playerX.transform;
        //the weapon is now child of the player, so we want the local rotation to be 0
        weaponX.transform.localRotation = Quaternion.Euler(0,0,0);
        //send info to playerStats
        playerStats stats = playerX.GetComponent<playerStats>();
        stats.currentWeapon = weaponX;
        //rotate the weapon based on the player direction
        //transform.forward gives us the facing direction of our player, only z value is changing from 1 to -1
        //move the weapon closer to the player
        weaponX.transform.position = playerX.transform.position + new Vector3(facingDirection, 0.2f, 0);
        //change the parent of the lifebar so that it does not rotate with the weapon
        changeLifeBarToPlayer(weaponX);
        if (weaponX.name.Contains("Bomb") || weaponX.name.Contains("Gun"))
        {
            
        }
        else
        {
            //rotate the melee weapon
            weaponX.transform.Rotate(0,0,-45);
        }
        //we have to switch this to false bcs otherwise we cannot drop the weapon
        weaponInRange = null;
        isInRange = false;
        currentWeapon = weaponX;
    }
    [PunRPC] public void dropWeapon(int weaponID,bool delete)
    {
        //remove the weapon from parent
        //find weapon based on photonId
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        weaponX.transform.parent = null;
        lifeBar.transform.parent = null;
        if (!delete)
        {
            StartCoroutine(changeLifeBarToWeapon(weaponX));
            //start playing the idle animation again
            Animator animator = weaponX.GetComponent<Animator>();
            animator.SetBool("isPicked", false);
        }
        else
        {
            lifeBar.transform.parent = weaponX.transform;
            Destroy(weaponX);
            lifeBar = null;
           
        }
        //in player stats remove the weapon as we are no longer holding it
        playerStats stats = gameObject.GetComponent<playerStats>();
        stats.currentWeapon = null;
        isHoldingWeapon = false;
        currentWeapon = null;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if parent is null, we dont want to rip the weapon from someones hands
        if (collision.gameObject.CompareTag("weapon") && collision.transform.parent == null)
        {
            //we are in range of an weapon
            isInRange = true;
            weaponInRange = collision.gameObject;
            
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
       //we are no longer in reach for the weapon
        if (collision.gameObject.transform.parent == null && weaponInRange == collision.gameObject)
        {
            weaponInRange = null;
            isInRange = false;
        }
    }
    IEnumerator waitForIsUsing()
    {
        yield return new WaitForSeconds(0.5f);
        alreadyUsing = false;
    }
    //this func is called when the lifetime of the weapon runs out
    public void drop(bool delete, GameObject weapon)
    {
        PhotonView photonView = PhotonView.Get(this);
        //RPC allows us to run a function on network gameobjects
        //we cannot send gameobject in RPC so we have to use photonviewID
        int weaponID = weapon.GetPhotonView().ViewID;
        photonView.RPC("dropWeapon", RpcTarget.All,weaponID,delete);
    }
    //change the parent so that the bar does not move with the weapon when attacking
    void changeLifeBarToPlayer(GameObject weapon)
    {
        //wait until it is picked, because if a player dropped the weapon and we quickly picked it up, the lifebar would not be the child of the weapon
        lifeBar = weapon.transform.GetChild(0).gameObject;
        //reset the lifebar position
        lifeBar.transform.position = weapon.transform.position + new Vector3(0, 1.2f, 0);
        lifeBar.transform.rotation = Quaternion.Euler(0, 0, 0);
        lifeBar.transform.parent = gameObject.transform;
    }
    IEnumerator changeLifeBarToWeapon(GameObject weapon)
    {
        //play the rest of the animation really fast
        Animator anim = weapon.GetComponent<Animator>();
        anim.speed = 1000;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("picked"));
        //change the lifebar back to the weapon
        anim.speed = 1;
        lifeBar.transform.position = weapon.transform.position + new Vector3(0, 1.2f, 0);
        lifeBar.transform.rotation = Quaternion.Euler(0, 0, 0);
        weapon.transform.rotation = quaternion.Euler(0,0,0);
        lifeBar.transform.parent = weapon.transform;
        lifeBar = null;
    }
    public void deleteLifeBar()
    {
        Destroy(lifeBar);
    }
   
}
