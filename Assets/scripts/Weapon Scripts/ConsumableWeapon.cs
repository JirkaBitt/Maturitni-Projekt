
using Photon.Pun;
using UnityEngine;

public abstract class ConsumableWeapon : Weapon
{
    // Start is called before the first frame update
    public abstract override void Use();
    public override void LaunchEnemy(GameObject enemy, Vector3 launchVector, float force)
    {
        //we want to have the function here so we dont have to write it for every Weapon type
        PhotonView weaponPhoton = gameObject.GetComponent<PhotonView>();
        PhotonView enemyPhoton = enemy.GetComponent<PhotonView>();
        //run the function on all enemy players
        float RandomMult = Random.Range(5, 10) / 10f;
        weaponPhoton.RPC("AddForce",RpcTarget.All,enemyPhoton.ViewID,launchVector,force * RandomMult);
    }

    [PunRPC] public override void AddForce(int photonViewID, Vector3 launchVector, float force)
    {
        // we will run this script on all instances of this Weapon, so in the instance of the enemy the Weapon will launch him
        PhotonView phView = PhotonView.Find(photonViewID);
        GameObject enemy = phView.gameObject;
        //add the force to his percentage and launch him with his percentage
        PlayerStats stats = enemy.GetComponent<PlayerStats>();
        stats.percentage += (int)(force/10  + force * stats.percentage/200);
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        rb.AddForce(launchVector * (force*6 + stats.percentage*2));
        enemy.GetComponent<CreateTrail>().ShowTrail();
    }

    public void ThrowWeapon(float distance)
    {
        //remove parent so we can throw it
        gameObject.transform.parent = null;
        //we have to destroy animator because Animator.apply root motion causes the gameobject to fly upwards!!!
        Animator animator = gameObject.GetComponent<Animator>();
        Destroy(animator);
        Rigidbody2D weaponRB = null;
        if(gameObject.TryGetComponent<Rigidbody2D>(out var bombRB))
        {
            weaponRB = bombRB;
        }
        else
        {
            weaponRB = gameObject.AddComponent<Rigidbody2D>();
        }//gameObject.AddComponent<Rigidbody2D>(); 
        //throw it in 45 degrees up, x value changes but y is always 1
        Vector3 throwVector = new Vector3(gameObject.transform.right.x, 2, 0);
        weaponRB.AddForce(throwVector * distance);
        //make the gameobject non trigger so it stops when it hits the ground
        gameObject.GetComponent<PolygonCollider2D>().isTrigger = false;
        
    }
    
}
