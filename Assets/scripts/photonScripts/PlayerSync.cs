using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    //List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
    public MonoBehaviour[] localScripts;
    //List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
    public GameObject[] localObjects;
    //Values that will be synced over network
    Vector3 latestPos;
    Quaternion latestRot;
    private PlayerStats stats;
    //ignore photon is for launching the player, it will remove it for a while from the updating via network
    public bool ignorePhoton = false;
    private GameObject textHolder;
    // Use this for initialization
    void Start()
    {
        stats = gameObject.GetComponent<PlayerStats>();
        gameObject.tag = "Player";
        if (!photonView.IsMine){
            //Player is Remote, deactivate the scripts and object that should only be enabled for the local player
            //we want to set collider to kinematic otherwise Vector3.Lerp keeps getting stuck
            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            textHolder = gameObject.transform.GetChild(0).gameObject;
            //disable the scripts that only the owner should have active, movement script etc
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
        //send the other players my position, rotation and percentage, network clones of my player will receive this information and apply it to their transform
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
        if (!ignorePhoton)
        {
            //we only want the non original instances of this player, the original one is controlled by the player
            if (!photonView.IsMine)
            {
                //Update remote player (smooth this, this looks good, at the cost of some accuracy)
                //Lerp is linear transformation
                //0 == transform.position and 1 is instant transformation to latestPos
                transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
                transform.rotation = latestRot;
                //we dont want to rotate the nameholder
                textHolder.transform.rotation = Quaternion.Euler(0,0,0);
            }
        }
    }

    public void launchEnemy()
    {
        //this is called when we are launching an enemy
        //make the copies ignore the original bcs there is a lot of lag
        ignorePhoton = true;
        StartCoroutine(wait());
    }
    
    IEnumerator wait()
    {
        yield return new WaitForSeconds(0.6f);
        ignorePhoton = false;
    }
}
