using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    //abstract class is used because every script(Axe, Sword) of a Weapon(gameobject) is of type Weapon(this script) and we then assign it the specific script of the right Weapon type
    //we can only attack when triggerlaunch is set to true
    public bool triggerLaunch;

    public GameObject lifeBar;
    //abstract Use is here that we can call it not matter the type of Weapon, it will run the override use on the lowest level of the script chain(Axe)
    public abstract void Use();
    public abstract void LaunchEnemy(GameObject enemy, Vector3 launchVector, float force);
    //in launch enemy we will call RPC for addforce
    public abstract void AddForce(int photonViewID, Vector3 launchVector, float force);
    public void AddTrail(GameObject weapon)
    {
        CreateTrail trailScript = weapon.GetComponent<CreateTrail>();
        trailScript.ShowTrail();
    }
    private void Start()
    {
        StartCoroutine(FallDown());
        lifeBar = gameObject.transform.GetChild(0).gameObject;
    }
    IEnumerator FallDown()
    {
        //add a rigidbody that will make the weapon fall due to gravity
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.2f;
        PolygonCollider2D coll = gameObject.GetComponent<PolygonCollider2D>();
        PolygonCollider2D ground = GameObject.FindWithTag("ground").GetComponent<PolygonCollider2D>();
        //wait until the weapon hits the ground or some player picks it up
        yield return new WaitUntil(() => coll.IsTouching(ground) || gameObject.transform.parent != null);
        //destroy the rb and stop the gravity
        Destroy(rb);
    }
}
