
using UnityEngine;

public class playerIcon : MonoBehaviour
{
    public GameObject playerCharacter;
    public GameObject background;
    void Start()
    {
        //create an icon with the player texture
        GameObject icon = new GameObject();
        SpriteRenderer rend = icon.AddComponent<SpriteRenderer>();
        rend.sprite = playerCharacter.GetComponent<SpriteRenderer>().sprite;
        SpriteRenderer backgroundRend = background.GetComponent<SpriteRenderer>();
        Vector2 iconSize = backgroundRend.bounds.size;
        ResizeAssets(icon, iconSize * (Mathf.Sqrt(2)/2));
    }
    public void ResizeAssets(GameObject resizeThis, Vector2 iconSize)
    {
        //create the default asset and fetch its size
        Vector3 createdSize = resizeThis.GetComponent<SpriteRenderer>().sprite.bounds.size;
        //get x and y scale
        Vector2 scale = new Vector2(iconSize.x / createdSize.x, iconSize.y / createdSize.y);
        //get which scale is bigger
        if (scale.x < scale.y)
        {
            //use the smaller scale
            //we want to multiply both sides with one scale to have the same aspect ratio
            resizeThis.transform.localScale *= scale.x;
        }
        else
        {
            //y scale is smaller
            resizeThis.transform.localScale *= scale.y;
        }
    }
}
