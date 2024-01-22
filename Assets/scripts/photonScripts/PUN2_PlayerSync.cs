using UnityEngine;
using Photon.Pun;

//https://sharpcoderblog.com/blog/make-a-multiplayer-game-in-unity-3d-using-pun-2
//from this tutorial
public class PUN2_PlayerSync : MonoBehaviourPun, IPunObservable
{

    private Rigidbody2D playerRB;
    //List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
    public MonoBehaviour[] localScripts;
    //List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
    public GameObject[] localObjects;
    //assign pickedweapon from other script
   
    //launch vector is used when we are hit by enemy 
   
    //Values that will be synced over network
    Vector3 latestPos;
    Quaternion latestRot;
    private int receivedWeaponID;
    private int weaponID;
    private int droppedWeaponID;
    private int legacyWeaponID;
    private Vector3 receivedLaunchVector;
    private playerStats stats;

    // Use this for initialization
    void Start()
    {
        stats = gameObject.GetComponent<playerStats>();
        playerRB = gameObject.GetComponent<Rigidbody2D>();
        if (photonView.IsMine)
        {
            //Player is local
            gameObject.tag = "Player";
        }
        else
        {
            //Player is Remote, deactivate the scripts and object that should only be enabled for the local player
         
            gameObject.tag = "Player";
            //we want to set collider to trigger otherwise Vector3.Lerp keeps getting stuck

            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

            for (int i = 0; i < localScripts.Length; i++)
            {
                localScripts[i].enabled = false;
            }
            for (int i = 0; i < localObjects.Length; i++)
            {
                localObjects[i].SetActive(false);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(stats.percentage);
           
        }
        else
        {
            //we get the data of the other player and update his position
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot = (Quaternion)stream.ReceiveNext();
            stats.percentage = (int)stream.ReceiveNext();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            //Lerp is linear transformation
            //0 == transform.position and 1 is instant transformation to latestPos
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
            //transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
            transform.rotation = latestRot;

            //launch network players
        }
        else
        {
            //this is my player
           
        }
    }
}
