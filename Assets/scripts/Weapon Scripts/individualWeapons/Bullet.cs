
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    public Vector3 launchVector = Vector3.zero;
    private bool alreadyAdded = false;
    public GameObject player;
    void Update()
    {
        if (launchVector != Vector3.zero && !alreadyAdded)
        {
            //release the bullet
            //we have to do it throw rb becuse otherwise it doesnt register on triggerstay for the ground
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.AddForce(launchVector * 200);
            alreadyAdded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //check if we have shot the gun
        if (launchVector != Vector3.zero)
        {
            PhotonView photonView = gameObject.GetPhotonView();
            if (!photonView.IsMine)
            {
                return;
            }
            GameObject hit = col.gameObject;
            if (hit.CompareTag("Player"))
            {
                //hit the player
                PhotonView enemyPhotonView = hit.GetPhotonView();
                float RandomMult = Random.Range(5, 10) / 10f;
                photonView.RPC("AddForceBullet", RpcTarget.AllViaServer, enemyPhotonView.ViewID, launchVector, 30f * RandomMult);
                
            }
            if (hit.CompareTag("ground"))
            {
                //we hit the ground, delete the bullet
                //only set the bullet to false, bcs it will trigger the checkgamebounds to delete it
                gameObject.SetActive(false);
            }
        }
    }
    [PunRPC] public void AddForceBullet(int photonViewID, Vector3 launchVector, float force)
    {
        // we will run this script on all instances of this Weapon, so in the instance of the enemy the Weapon will launch him
        PhotonView phView = PhotonView.Find(photonViewID);
        GameObject enemy = phView.gameObject;
        //add the force to his percentage and launch him with his percentage
        PlayerStats stats = enemy.GetComponent<PlayerStats>();
        stats.percentage += (int)(force/10  + force * stats.percentage/200);
        stats.lastAttacker = player;
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        rb.AddForce(launchVector * (force * 2 + stats.percentage * 2));
        enemy.GetComponent<CreateTrail>().ShowTrail(); 
        if (gameObject.GetPhotonView().IsMine)
        {
            //this will ensure that CheckGameBounds deletes the bullet
            gameObject.SetActive(false);
        }
    }
}
