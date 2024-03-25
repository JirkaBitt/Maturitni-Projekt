using System.Collections;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;

public class PickWeapon : MonoBehaviour
{
    //check if we are in range of an Weapon to pick it up
    public bool isInRange = false;
    //refrence to the Weapon that is in our reach
    public GameObject weaponInRange;
    //if our player is already holding a Weapon
    public bool isHoldingWeapon = false;
    //the script to use the Weapon
    private Weapon _weaponScript;
    //keep track if we have already clicked attack to prevent spamming
    private bool alreadyUsing = false;
    //the lifebar of the picked Weapon
    private GameObject lifeBar;
    //if we decide to pickup the Weapon it is stored here
    public GameObject currentWeapon;
    void Update()
    {
            if (Input.GetKeyDown(KeyCode.E) && isHoldingWeapon)
            {
                 //drop the Weapon
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
                    photonView.RPC("AssignPlayerWeapon", RpcTarget.AllViaServer, currentPlayerID, pickedWeaponID);
                }

            }
            if (isHoldingWeapon && Input.GetMouseButtonDown(0) && !alreadyUsing)
            {
                alreadyUsing = true;
                _weaponScript.Use();
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
        //retrieve the Weapon script
        _weaponScript = weaponX.GetComponent<Weapon>();
        //stop playing the idle animation
        Animator animator = weaponX.GetComponent<Animator>();
        animator.SetBool("isPicked", true);
        //assign player as parent so that Weapon moves with him
        float facingDirection = gameObject.transform.forward.z;
        weaponX.transform.parent = playerX.transform;
        //the Weapon is now child of the player, so we want the local rotation to be 0
        weaponX.transform.localRotation = Quaternion.Euler(0,0,0);
        //send info to PlayerStats
        PlayerStats stats = playerX.GetComponent<PlayerStats>();
        stats.currentWeapon = weaponX;
        //rotate the Weapon based on the player direction
        //transform.forward gives us the facing direction of our player, only z value is changing from 1 to -1
        //move the Weapon closer to the player
        weaponX.transform.position = playerX.transform.position + new Vector3(facingDirection, 0.2f, 0);
        //change the parent of the lifebar so that it does not rotate with the Weapon
        changeLifeBarToPlayer(weaponX);
        if (weaponX.name.Contains("Bomb") || weaponX.name.Contains("Gun"))
        {
            
        }
        else
        {
            //rotate the melee Weapon
            weaponX.transform.Rotate(0,0,-45);
        }
        //we have to switch this to false bcs otherwise we cannot drop the Weapon
        weaponInRange = null;
        isInRange = false;
        currentWeapon = weaponX;
    }
    [PunRPC] public void dropWeapon(int weaponID,bool delete)
    {
        //remove the Weapon from parent
        //find Weapon based on photonId
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        if (!delete)
        {
            weaponX.transform.parent = null;
            lifeBar.transform.parent = null;
            StartCoroutine(changeLifeBarToWeapon(weaponX));
            //start playing the idle animation again
            Animator animator = weaponX.GetComponent<Animator>();
            animator.SetBool("isPicked", false);
            //make the weapon fall down to the ground
            weaponX.GetComponent<Weapon>().StartCoroutine("fallDown");
        }
        else
        {
            lifeBar.transform.parent = weaponX.transform;
            Destroy(weaponX);
            lifeBar = null;
        }
        //in player stats remove the Weapon as we are no longer holding it
        PlayerStats stats = gameObject.GetComponent<PlayerStats>();
        stats.currentWeapon = null;
        isHoldingWeapon = false;
        currentWeapon = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if parent is null, we dont want to rip the Weapon from someones hands
        if (collision.gameObject.CompareTag("weapon") && collision.transform.parent == null)
        {
            //we are in range of an Weapon
            isInRange = true;
            weaponInRange = collision.gameObject;
            
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
       //we are no longer in reach for the Weapon
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
    //this func is called when the lifetime of the Weapon runs out
    public void drop(bool delete, GameObject weapon)
    {
        if (weapon == null)
        {
            return;
        }
        PhotonView photonView = PhotonView.Get(this);
        //RPC allows us to run a function on network gameobjects
        //we cannot send gameobject in RPC so we have to use photonviewID
        int weaponID = weapon.GetPhotonView().ViewID;
        photonView.RPC("dropWeapon", RpcTarget.AllViaServer,weaponID,delete);
    }
    //change the parent so that the bar does not move with the Weapon when attacking
    void changeLifeBarToPlayer(GameObject weapon)
    {
        //wait until it is picked, because if a player dropped the Weapon and we quickly picked it up, the lifebar would not be the child of the Weapon
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
        //change the lifebar back to the Weapon
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
