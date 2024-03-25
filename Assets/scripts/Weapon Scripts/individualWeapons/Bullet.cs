
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
            GameObject hit = col.gameObject;
            if (hit.CompareTag("Player"))
            {
                //hit the player
                PhotonView bulletPhotonView = gameObject.GetPhotonView();
                if (bulletPhotonView.IsMine)
                {
                    PhotonView enemyPhotonView = hit.GetPhotonView();
                    bulletPhotonView.RPC("addForceBullet", RpcTarget.AllViaServer, enemyPhotonView.ViewID, launchVector, 60f);
                }
            }
            if (hit.CompareTag("ground"))
            {
                //we hit the ground, delete the bullet
                PhotonView photonView = gameObject.GetPhotonView();
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
    [PunRPC] public void addForceBullet(int photonViewID, Vector3 launchVector, float force)
    {
        // we will run this script on all instances of this Weapon, so in the instance of the enemy the Weapon will launch him
        PhotonView phView = PhotonView.Find(photonViewID);
        GameObject enemy = phView.gameObject;
        //add the force to his percentage and launch him with his percentage
        PlayerStats stats = enemy.GetComponent<PlayerStats>();
        //add a little bit of randomness
        float randomMultiplier = Random.Range(4, 10);
        randomMultiplier /= 10;
        force *= randomMultiplier;
        stats.percentage += (int)(force/10  + force * stats.percentage/200);
        stats.lastAttacker = player;
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        rb.AddForce(launchVector * (force + stats.percentage*2));
        enemy.GetComponent<CreateTrail>().createTrail();
        if (gameObject.GetPhotonView().IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
