using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class pickWeapon : MonoBehaviour
{

    public bool isInRange = false;
    public GameObject weaponInRange;
    
    public bool isHoldingWeapon = false;
    
    private weapon weaponScript;

    private bool alreadyUsing = false;

    private GameObject lifeBar;

    public GameObject currentWeapon;
    //private weapon_Sync _weaponSync;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
                    photonView.RPC("AssignPlayerWeapon", RpcTarget.AllBuffered, currentPlayerID, pickedWeaponID);
                }

            }
            if (isHoldingWeapon && Input.GetMouseButtonDown(0) && !alreadyUsing)
            {
                alreadyUsing = true;
                weaponScript.Use();
                //waitforisusing prevents the player from spamming attacks
                StartCoroutine(waitForIsUsing());

            }
    }
    //PunRPC mark is used that we can send this script over the network for other instances to call 
    [PunRPC] public void AssignPlayerWeapon(int playerID, int weaponID)
    {
        //Find the Gameobjects based on ID
        isHoldingWeapon = true;
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        //GameObject housing = weaponX.transform.parent.gameObject;
        GameObject playerX = PhotonView.Find(playerID).gameObject;
       
        //assign values to our script
        weaponScript = weaponX.GetComponent<weapon>();
        //weaponScript.weaponGameobject = weaponX;
        //weaponScript.photonID = weaponID;
        //stop playing the idle animation
        Animator animator = weaponX.GetComponent<Animator>();
        animator.SetBool("isPicked", true);
        //assign plaer as parent so that weapon moves with him
       // weaponX.transform.parent = playerX.transform;
        weaponX.transform.parent = playerX.transform;
        //send info to playerStats
        playerStats stats = playerX.GetComponent<playerStats>();
        stats.currentWeapon = weaponX;
        //rotate the weapon based on the player direction
        //if facing left then rotate the weapon on y=180 else y = 0
        //transform.forward gives us the facing direction of our player, only z value is changing from 1 to -1
        float facingDirection = gameObject.transform.forward.z;
        if (facingDirection > 0)
        {
            //rotate left
            weaponX.transform.rotation = Quaternion.Euler(0,180,0);
            //housing.transform.rotation = Quaternion.Euler(0,180,0);
        }
        if (facingDirection < 0)
        {
            //rotate right
            weaponX.transform.rotation = Quaternion.Euler(0,0,0);
            //housing.transform.rotation = Quaternion.Euler(0,0,0);
        }
        //move the weapon closer to the player
        //weaponX.transform.position = playerX.transform.position + new Vector3(-facingDirection, 0.2f, 0);
        weaponX.transform.position = playerX.transform.position + new Vector3(-facingDirection, 0.2f, 0);
        changeLifeBarToPlayer(weaponX);
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
        //GameObject weapon = gameObject.transform.GetChild(0).gameObject;
        int weaponID = weapon.GetPhotonView().ViewID;
        photonView.RPC("dropWeapon", RpcTarget.AllBuffered,weaponID,delete);
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
       
        Animator anim = weapon.GetComponent<Animator>();
        anim.speed = 1000;
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("picked"));
        //check if it is not null, we might have deleted it when the time run out
        anim.speed = 1;
        lifeBar.transform.position = weapon.transform.position + new Vector3(0, 1.2f, 0);
        lifeBar.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        lifeBar.transform.parent = weapon.transform;
        lifeBar = null;
    }

    public void deleteLifeBar()
    {
        Destroy(lifeBar);
    }
   
}
