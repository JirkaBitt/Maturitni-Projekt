using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class gunPlasmaBall : gunWeapons
{
    public GameObject bulletPrefab;

    public float maxScaleMagnitude = 1.2f;
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
            gameObject.GetPhotonView().RPC("addParent",RpcTarget.All,playerID,bulletID);
            StartCoroutine(chargingGun(bullet));
        
    }

    IEnumerator chargingGun(GameObject bullet)
    {
        //check if we are still charging
        
        Vector3 scale = bullet.transform.localScale;
        
        while (Input.GetMouseButton(0))
        {
            if (scale.magnitude < maxScaleMagnitude)
            {
                scale += new Vector3(1, 1, 0) * 0.5f * Time.deltaTime;
                bullet.transform.localScale = scale;
            }

            yield return null;
        }
        //now release the bullet
        GameObject player = transform.parent.gameObject;

        bulletScript bullScript = bullet.GetComponent<bulletScript>();

        Vector3 launchVector = gameObject.transform.right * 2;

        //with this we will set the bullet flying, we can adjust the speed adding scale to the the vector
        int bulletID = bullet.GetPhotonView().ViewID;
        gameObject.GetPhotonView().RPC("removeParent",RpcTarget.All,bulletID);
        launchVector *= scale.magnitude * 1.5f;
        bullScript.launchVector = launchVector;
    }

    [PunRPC]
    public void addParent(int parentID, int bulletID)
    {
        GameObject bullet = PhotonView.Find(bulletID).gameObject;
        GameObject parent = PhotonView.Find(parentID).gameObject;
        bullet.transform.parent = parent.transform;
        
        //assign the player so that we know who hit the enemy
        bullet.GetComponent<bulletScript>().player = parent;
    }

    [PunRPC]
    public void removeParent(int bulletID)
    { 
        GameObject bullet = PhotonView.Find(bulletID).gameObject;
       bullet.transform.parent = null;
    }
}
