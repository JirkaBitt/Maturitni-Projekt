using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PUN2_weaponSync : MonoBehaviourPun, IPunObservable
{
    Vector3 latestPos;
    Quaternion latestRot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!photonView.IsMine && transform.parent == null)
        {
            //if this isnt mine instance and we are not picked up then update the transform
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
        }
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
          //we only need to send position because rotation is the same
            stream.SendNext(transform.position);
            
        }
        else
        {
            //receive data
            latestPos = (Vector3)stream.ReceiveNext();
            
        }
    }
}
