using System.Collections;

using Photon.Pun;
using UnityEngine;

public class Gun : GunWeapon
{
    public float maxScaleMagnitude = 1.6f;
    // Start is called before the first frame update
    public override void Use()
    {
            //shoot the gun
            Vector3 spawnPosition = gameObject.transform.position + 1.6f * gameObject.transform.right;
            //create a new instance of the bullet
            GameObject bullet = PhotonNetwork.Instantiate("Projectile", spawnPosition, Quaternion.identity);
            //make the bullet move the gun
            int playerID = gameObject.transform.parent.gameObject.GetPhotonView().ViewID;
            //we have to add the bullet as a parent because localScale is relative to parent
            int bulletID = bullet.GetPhotonView().ViewID;
            gameObject.GetPhotonView().RPC("AddParent",RpcTarget.All,playerID,bulletID);
            StartCoroutine(ChargingGun(bullet));
    }

    IEnumerator ChargingGun(GameObject bullet)
    {
        //check if we are still charging
        Vector3 scale = new Vector3(0.5f,0.5f,0.5f);
        while (Input.GetMouseButton(0))
        {
            if (scale.magnitude < maxScaleMagnitude)
            {
                Vector3 step = new Vector3(1, 1, 0) * 0.5f * Time.deltaTime;
                scale += step;
                bullet.transform.localScale += step;
            }
            yield return null;
        }
        //now release the bullet
        Bullet bull = bullet.GetComponent<Bullet>();
        Vector3 launchVector = gameObject.transform.right * 2;
        //with this we will set the bullet flying, we can adjust the speed adding scale to the the vector
        int bulletID = bullet.GetPhotonView().ViewID;
        gameObject.GetPhotonView().RPC("RemoveParent",RpcTarget.All,bulletID);
        launchVector *= scale.magnitude * 1.5f;
        bull.launchVector = launchVector;
    }
    [PunRPC]
    public void AddParent(int parentID, int bulletID)
    {
        GameObject bullet = PhotonView.Find(bulletID).gameObject;
        GameObject parent = PhotonView.Find(parentID).gameObject;
        bullet.transform.parent = gameObject.transform;
        //assign the player so that we know who hit the enemy
        bullet.GetComponent<Bullet>().player = parent;
    }

    [PunRPC]
    public void RemoveParent(int bulletID)
    { 
        GameObject bullet = PhotonView.Find(bulletID).gameObject;
        bullet.transform.parent = null;
    }
}
