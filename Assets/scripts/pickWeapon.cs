using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class pickWeapon : MonoBehaviour
{

    public bool isInRange = false;
    public GameObject weaponInRange;
    
    public bool isHoldingWeapon = false;
   
    public GameObject currentWeapon;
   
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
       
            //this is our character
            if (isInRange)
            {
                if (Input.GetKeyDown(KeyCode.E) && !isHoldingWeapon)
                {
                    print("pick weapon");
                    
                    //we have to use ids because we are sending them over the network
                    int pickedWeaponID = weaponInRange.GetComponent<PhotonView>().ViewID;
                    int currentPlayerID = gameObject.GetComponent<PhotonView>().ViewID;
                    
                   // AssignPlayerWeapon(currentPlayerID, pickedWeaponID);
                    
                    PhotonView photonView = PhotonView.Get(this);
                    //we cannot send gameobject in RPC so we have to use photonviewID
                    photonView.RPC("AssignPlayerWeapon", RpcTarget.AllBuffered, currentPlayerID, pickedWeaponID);
                    
                    StartCoroutine(waitForIsHolding());
                   

                }

            }
            if (Input.GetKeyDown(KeyCode.E) && isHoldingWeapon)
            {
                //drop the weapon
                
                PhotonView photonView = PhotonView.Get(this);
                //RPC allows us to run a function on network gameobjects
                //we cannot send gameobject in RPC so we have to use photonviewID
                photonView.RPC("dropWeapon", RpcTarget.AllBuffered,weaponScript.photonID);

            }

            if (isHoldingWeapon && Input.GetMouseButtonDown(0) && !alreadyUsing)
            {
                alreadyUsing = true;
                weaponScript.Use();
                StartCoroutine(waitForIsUsing());

            }
            
            //we can call weaponScript.Use() to attack
    }

    //PunRPC mark is used that we can send this script over the network for other instances to call 
    [PunRPC] public void AssignPlayerWeapon(int playerID, int weaponID)
    {
        
       
        
        //Find the Gameobjects based on ID
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        GameObject playerX = PhotonView.Find(playerID).gameObject;

        //assign values to our script
        getWeaponScript(weaponX);
        weaponScript.weaponGameobject = weaponX;
        weaponScript.photonID = weaponID;
        
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

    }

    [PunRPC] public void dropWeapon(int weaponID)
    {
        //remove the weapon from parent
        //find weapon based on photonId
        GameObject weaponX = PhotonView.Find(weaponID).gameObject;
        weaponX.transform.parent = null;
        
        Animator animator = weaponX.GetComponent<Animator>();
        animator.SetBool("isPicked", false);
        
        playerStats stats = gameObject.GetComponent<playerStats>();
        stats.currentWeapon = null;
        
        currentWeapon = null;
        isHoldingWeapon = false;

        weaponScript.weaponGameobject = null;
        weaponScript.photonID = 0;


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if parent is null, we dont want to rip the weapon from someones hands
        GameObject parent = null;
        if (collision.gameObject.transform.parent != null)
        {
            parent = collision.gameObject.transform.parent.gameObject;
        }
        if(collision.gameObject.CompareTag("weapon"))
        {
            //we are in range of an weapon
            if (parent != null)
            {
              
                if (parent.CompareTag("Player"))
                {
                      //we cannot pick it up, other player has it
                }
                else
                {
                    isInRange = true;
                    //the parent houses the script atd
                    weaponInRange = parent;
                }
            }
            else
            {
                isInRange = true;
                weaponInRange = collision.gameObject;
            }
           

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject parent = null;
        if (collision.gameObject.transform.parent != null)
        {
            parent = collision.gameObject.transform.parent.gameObject;
        }
        //created asset
        if(parent == weaponInRange)
        {
            //reset the in range
            weaponInRange = null;
            isInRange = false;
        }
        //default asset
        if (parent == null && weaponInRange == collision.gameObject)
        {
            weaponInRange = null;
            isInRange = false;
        }
    }

    IEnumerator waitForIsHolding()
    {
        //wait one frame so it doesnt drop in the same update
        yield return null;
        //switch the values
        if (isHoldingWeapon)
        {
            isHoldingWeapon = false;
        }
        else
        {
            isHoldingWeapon = true;
        }
    }

    void getWeaponScript(GameObject weaponn)
    {
       
        weaponScript = weaponn.GetComponent<weapon>();
        
    }

    IEnumerator waitForIsUsing()
    {
        yield return new WaitForSeconds(0.5f);
        alreadyUsing = false;
    }
   
}
