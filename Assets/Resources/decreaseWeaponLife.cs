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
    void Start()
    {
        rend = lifeBar.GetComponent<SpriteRenderer>();
       
        WidthX = rend.sprite.bounds.size.x;
       
        startScaleX = lifeBar.transform.localScale.x;

        //adjustToAsset();
        
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
            lifeBar.transform.position -= new Vector3((WidthX / 100) * rate, 0, 0);
            float colorValue = lifePercentage / 100;
            
            rend.color = new Color(1 - colorValue, colorValue, 0, 1);
            yield return new WaitForSeconds(interval);
        }
        RemoveWeapon();
    }

    void RemoveWeapon()
    {
        GameObject weapon = gameObject.transform.parent.gameObject;
        if (weapon.GetPhotonView().IsMine)
        {
            if (weapon.transform.parent != null)
            {
                //remove the weapon from player
                GameObject player = weapon.transform.parent.gameObject;
                pickWeapon pickScript = player.GetComponent<pickWeapon>();
                pickScript.drop(true);
            }
            else
            {
                PhotonNetwork.Destroy(weapon);
            }
            
        }
    }

    void adjustToAsset()
    {
        GameObject weapon = gameObject.transform.parent.gameObject;
        SpriteRenderer weaponRend = weapon.GetComponent<SpriteRenderer>();
        Vector2 assetSize = weaponRend.sprite.bounds.size;

        float multiplier = assetSize.x / WidthX;

       // gameObject.transform.position = weapon.transform.position + new Vector3(0, assetSize.y / 2 + 1,0);
        gameObject.transform.localScale = Vector3.one * multiplier;
    }
}
