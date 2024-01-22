using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class weapon_Sync :  MonoBehaviourPun, IPunObservable
{


    private Vector3 weaponPosition;
    private Quaternion weaponRotation;

    private int LegacyPlayerID;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (photonView.IsMine)
        {
            //original weapon
        }
        else
        {
            //network weapon, update transform
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,weaponPosition,Time.deltaTime * 5);
            
            //we want to use Lerp here because of the animations
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, weaponRotation, Time.deltaTime * 5);
            
        }
        */
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /*
        if (stream.IsWriting)
        {
            //original weapon, send info about position and rotation
            stream.SendNext(gameObject.transform.position);
            stream.SendNext(gameObject.transform.rotation);
         
        }
        else
        {
            weaponPosition = (Vector3)stream.ReceiveNext();
            weaponRotation = (Quaternion)stream.ReceiveNext();

        }
        */
    }
    
}
