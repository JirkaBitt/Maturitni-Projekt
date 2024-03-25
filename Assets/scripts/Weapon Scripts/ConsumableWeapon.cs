
using Photon.Pun;
using UnityEngine;

public abstract class ConsumableWeapon : Weapon
{
    // Start is called before the first frame update
    public abstract override void Use();
    public override void launchEnemy(GameObject enemy, Vector3 launchVector, float force)
    {
        //we want to have the function here so we dont have to write it for every Weapon type
        PhotonView weaponPhoton = gameObject.GetComponent<PhotonView>();
        PhotonView enemyPhoton = enemy.GetComponent<PhotonView>();
        //run the function on all enemy players
        weaponPhoton.RPC("addForce",RpcTarget.All,enemyPhoton.ViewID,launchVector,force);
    }

    [PunRPC] public override void addForce(int photonViewID, Vector3 launchVector, float force)
    {
        // we will run this script on all instances of this Weapon, so in the instance of the enemy the Weapon will launch him
        PhotonView phView = PhotonView.Find(photonViewID);
        GameObject enemy = phView.gameObject;
        //add the force to his percentage and launch him with his percentage
        PlayerStats stats = enemy.GetComponent<PlayerStats>();
        //add a litle bit of randomness
        float randomMultiplier = Random.Range(4, 10);
        randomMultiplier /= 10;
        force *= randomMultiplier;
        print(force);
        stats.percentage += (int)(force/10  + force * stats.percentage/200);
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        rb.AddForce(launchVector * (force*6 + stats.percentage*2));
    }

    public void throwWeapon(float distance)
    {
        //remove parent so we can throw it
        gameObject.transform.parent = null;
        //we have to destroy animator because Animator.apply root motion causes the gameobject to fly upwards!!!
        Animator animator = gameObject.GetComponent<Animator>();
        Destroy(animator);
        Rigidbody2D weaponRB = gameObject.AddComponent<Rigidbody2D>(); 
        //throw it in 45 degrees up, x value changes but y is always 1
        Vector3 throwVector = new Vector3(gameObject.transform.right.x, 2, 0);
        weaponRB.AddForce(throwVector * distance);
        //make the gameobject non trigger so it stops when it hits the ground
        if (gameObject.GetComponent<CircleCollider2D>() != null)
        {
            gameObject.GetComponent<CircleCollider2D>().isTrigger = false;
        }
        if (gameObject.GetComponent<PolygonCollider2D>() != null)
        {
            gameObject.GetComponent<PolygonCollider2D>().isTrigger = false;
        }
    }
    
}
