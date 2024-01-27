using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class decreaseWeaponLife : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject lifeBar;
   
    private float WidthX;
    
    private Vector2 currentSize;
    private float startScaleX;

    public float ratePerSecond;
    public float lifePercentage;
    private SpriteRenderer rend;
    public float interval;

    public GameObject weapon;

    void Start()
    {
        rend = lifeBar.GetComponent<SpriteRenderer>();
       
        WidthX = rend.sprite.bounds.size.x;
       
        startScaleX = lifeBar.transform.localScale.x;

        //weapon = gameObject.transform.parent.gameObject;

        StartCoroutine(decrease());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator decrease()
    {
        float rate = ratePerSecond * interval;
        while (lifePercentage > 0)
        {
            lifePercentage -= rate;
            lifeBar.transform.localScale -= new Vector3((startScaleX / 100) * rate, 0, 0);
            lifeBar.transform.position -= new Vector3((WidthX / 100) * rate, 0, 0) * -lifeBar.transform.forward.z;
            float colorValue = lifePercentage / 100;
            
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
                //GameObject player = gameObject.transform.parent.transform.parent.gameObject;
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
