
using UnityEngine;

public class assetInfo : MonoBehaviour
{
    //the width and height of the sprite
    public float height = 10;
    public float width = 10;
    public bool hasSprite;
    public bool hasAnimation;
    void Start()
    {
        calculateSize();
    }
    public void calculateSize()
    {
        //calculate the width and height so we can resize the new created assets
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
}
