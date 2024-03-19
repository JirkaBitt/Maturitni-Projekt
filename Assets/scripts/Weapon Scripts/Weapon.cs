
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    //abstract class is used because every script(Axe, Sword) of a Weapon(gameobject) is of type Weapon(this script) and we then assign it the specific script of the right Weapon type
    //we can only attack when triggerlaunch is set to true
    public bool triggerLaunch;
    //abstract Use is here that we can call it not matter the type of Weapon, it will run the override use on the lowest level of the script chain(Axe)
    public abstract void Use();
    public abstract void launchEnemy(GameObject enemy, Vector3 launchVector, float force);
    //in launch enemy we will call RPC for addforce
    public abstract void addForce(int photonViewID, Vector3 launchVector, float force);
    public void addTrail(GameObject weapon)
    {
        CreateTrail trailScript = weapon.GetComponent<CreateTrail>();
        trailScript.createTrail();
    }
}
