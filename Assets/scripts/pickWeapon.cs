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
                PhotonView photonView = PhotonView.Get(this);
                //RPC allows us to run a function on network gameobjects
                //we cannot send gameobject in RPC so we have to use photonviewID
                int weaponID = gameObject.transform.GetChild(0).gameObject.GetPhotonView().ViewID;
                photonView.RPC("dropWeapon", RpcTarget.AllBuffered,weaponID);

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
        GameObject playerX = PhotonView.Find(playerID).gameObject;
        //assign values to our script
        weaponScript = weaponX.GetComponent<weapon>();
        //weaponScript.weaponGameobject = weaponX;
        //weaponScript.photonID = weaponID;
        //stop playing the idle animation
        Animator animator = weaponX.GetComponent<Animator>();
        animator.SetBool("isPicked", true);
        //assign plaer as parent so that weapon moves with him
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
        }
        if (facingDirection < 0)
        {
            //rotate right
            weaponX.transform.rotation = Quaternion.Euler(0,0,0);
        }
        //move the weapon closer to the player
        weaponX.transform.position = playerX.transform.position + new Vector3(-facingDirection, 0.2f, 0);
        //we have to switch this to false bcs otherwise we cannot drop the weapon
        weaponInRange = null;
        isInRange = false;
    }
    [PunRPC] public void dropWeapon(int weaponID)
    {
        //remove the weapon from parent
        //find weapon based on photonId
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        weaponX.transform.parent = null;
        //start playing the idle animation again
        Animator animator = weaponX.GetComponent<Animator>();
        animator.SetBool("isPicked", false);
        //in player stats remove the weapon as we are no longer holding it
        playerStats stats = gameObject.GetComponent<playerStats>();
        stats.currentWeapon = null;
        isHoldingWeapon = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if parent is null, we dont want to rip the weapon from someones hands
        
        if(collision.gameObject.CompareTag("weapon") && collision.gameObject.transform.parent == null)
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
   
}
