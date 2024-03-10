
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreateTrail : MonoBehaviour
{
    //white sprite is the copy of the original sprite with changed color
    public Sprite whiteSprite;
    //how long should one copy live
    public float oneCopyDuration;
    //the number of copies that will be created
    public int numberOfCopies;
    //if the copies should have decaying alpha
    public bool fadeSprite = true;
    //the color of the trail, player has red weapons white
    public Color trailColor = Color.white;
    //list of the instantiated copies 
    private List<GameObject> createdCopies = new List<GameObject>();
    public void createTexture()
    {
        SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
        Texture2D tex = rend.sprite.texture;
        Color[] texColors = tex.GetPixels();
        float texLength = texColors.Length;
        //change every black pixel to the corresponding color
        for (int i = 0; i < texLength; i++)
        {
            if (texColors[i] == Color.black)
            {
                texColors[i] = trailColor;
                if (fadeSprite)
                {
                    texColors[i].a = i * (1 / texLength);
                }
            }
        }
        Sprite originalSprite = rend.sprite;
        Texture2D newTex = new Texture2D(tex.width,tex.height);
        newTex.SetPixels(texColors);
        newTex.Apply();
        whiteSprite = Sprite.Create(newTex, originalSprite.rect, new Vector2(0.5f, 0.5f));
    }
    IEnumerator createCopy(Sprite sprite)
    {
        //create one instance of the copy
        GameObject copy = new GameObject("copy");
        createdCopies.Add(copy);
        SpriteRenderer rend = copy.AddComponent<SpriteRenderer>();
        rend.sprite = sprite;
        copy.transform.parent = transform.parent;
        //it should be behind at the z axis 
        copy.transform.position = transform.position + new Vector3(0,0,1);
        copy.transform.rotation = transform.rotation;
        copy.transform.localScale = transform.localScale;
        //we are reassigning them because it is faster, the script does not have to search for the global variable
        float lifetime = 0;
        float lifetimeDuration = oneCopyDuration;
        while (lifetime < lifetimeDuration)
        {
            //decrease the opacity
            //frameduration has to be float because 1/int will be 1 or 0, we have to divide by float
            rend.color = new Color(1f,1f,1f,1 - (lifetime * (1 / lifetimeDuration)));
            lifetime += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }
        createdCopies.Remove(copy);
        Destroy(copy);
    }

    IEnumerator createTrailCoroutine()
    {
        for (int i = 0; i < numberOfCopies; i++)
        {
            StartCoroutine(createCopy(whiteSprite));
            yield return new WaitForSeconds(0.02f);
        }
        createdCopies.Clear();
    }

    public void createTrail()
    {
        StartCoroutine(createTrailCoroutine());
    }

    private void OnDisable()
    {
        //if the weapon gets deleted while we are attacking we have to delete the trail afterwards
        foreach (var copy in createdCopies)
        {
            Destroy(copy);
        }
    }
    private void OnDestroy()
    {
        //if the weapon gets deleted while we are attacking we have to delete the trail afterwards
        foreach (var copy in createdCopies)
        {
            Destroy(copy);
        }
    }
}
