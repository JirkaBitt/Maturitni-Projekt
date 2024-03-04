using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class assetInfo : MonoBehaviour
{
    public float height = 10;

    public float width = 10;

    public bool hasSprite;
    public bool hasAnimation;
    
    public Vector3 startScale;

    public int[,] pixelArray;

    public string name;
    
    // Start is called before the first frame update
    void Start()
    {
        /*
        startScale = gameObject.transform.localScale;
        if (gameObject.GetComponent<SpriteRenderer>() != null)
        {
            Sprite assetSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
            height = assetSprite.bounds.size.y;
            width = assetSprite.bounds.size.x;
            
        }*/
        calculateSize();
    }

   public void calculateSize()
    {
        startScale = gameObject.transform.localScale;
        if (hasSprite)
        {
            Sprite assetSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
            if (assetSprite != null)
            {
                height = assetSprite.bounds.size.y;
                width = assetSprite.bounds.size.x;
            }
        }
    }
   public void turnOffAnimation()
   {
       if (hasAnimation)
       {
           Animator animator = gameObject.GetComponent<Animator>();
           animator.enabled = false;
          
       }
   }

   public void turnOnAnimation()
   {
       if (hasAnimation)
       {
           Animator animator = gameObject.GetComponent<Animator>();
           animator.enabled = true;
       } 
   }
    // Update is called once per frame
    void Update()
    {
        
    }
}
