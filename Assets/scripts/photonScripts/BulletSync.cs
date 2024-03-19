using Photon.Pun;
using UnityEngine;

public class BulletSync : MonoBehaviourPun, IPunObservable
{
    Vector3 latestPos;
    Vector3 latestScale;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.localScale);
        }
        else
        {
            //we get the data of the other player and update his position
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestScale = (Vector3)stream.ReceiveNext();
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
            transform.localScale = Vector3.Lerp(transform.localScale, latestScale, Time.deltaTime * 5);
        }
       
    }
}
