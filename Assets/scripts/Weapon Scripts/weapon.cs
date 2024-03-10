
using UnityEngine;

public abstract class weapon : MonoBehaviour
{
    //abstract class is used because every script(axeWeapon, swordWeapon) of a weapon(gameobject) is of type weapon(this script) and we then assign it the specific script of the right weapon type
    //we can only attack when triggerlaunch is set to true
    public bool triggerLaunch;
    //abstract Use is here that we can call it not matter the type of weapon, it will run the override use on the lowest level of the script chain(axeWeapon)
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
