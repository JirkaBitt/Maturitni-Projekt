using System.Collections;

using Photon.Pun;
using UnityEngine;

public class decreaseWeaponLife : MonoBehaviour
{
    //reference to the prefab
    public GameObject lifeBar;
   //width of the lifebar
    private float WidthX;
    //size of the lifebar
    private Vector2 currentSize;
    private float startScaleX;
    //how fast it is going to decay
    public float ratePerSecond;
    public float lifePercentage;
    private SpriteRenderer rend;
    //how often to remove from it
    public float interval;
    public GameObject weapon;
    void Start()
    {
        rend = lifeBar.GetComponent<SpriteRenderer>();
        WidthX = rend.sprite.bounds.size.x;
        startScaleX = lifeBar.transform.localScale.x;
        StartCoroutine(decrease());
    }
    IEnumerator decrease()
    {
        float rate = ratePerSecond * interval;
        while (lifePercentage > 0)
        {
            lifePercentage -= rate;
            //we have to shorten the lifebar and move it to the side so it looks like it is shortening just from one side
            lifeBar.transform.localScale -= new Vector3((startScaleX / 100) * rate, 0, 0);
            lifeBar.transform.position -= new Vector3((WidthX / 100) * rate, 0, 0) * -lifeBar.transform.forward.z;
            float colorValue = lifePercentage / 100;
            //change the color from green to red
            rend.color = new Color(1 - colorValue, colorValue, 0, 1);
            yield return new WaitForSeconds(interval);
        }
        RemoveWeapon();
    }

    void RemoveWeapon()
    {
        if (weapon.GetPhotonView().IsMine)
        {
            if (weapon.transform.parent != null)
            {
                //remove the weapon from player
                GameObject player = weapon.transform.parent.gameObject;
                pickWeapon pickScript = player.GetComponent<pickWeapon>();
                pickScript.drop(true,weapon);
            }
            else
            {
                PhotonNetwork.Destroy(weapon);
            }
        }
    }
}
