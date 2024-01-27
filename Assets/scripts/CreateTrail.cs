using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CreateTrail : MonoBehaviour
{
    private Sprite whiteSprite;

    public float oneCopyDuration;

    public int numberOfCopies;

    public bool fadeSprite = true;

    public Color trailColor = Color.white;
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
        Texture2D tex = rend.sprite.texture;
        
        Color[] texColors = tex.GetPixels();
        float texLength = texColors.Length;
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

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator createCopy(Sprite sprite)
    {
        GameObject copy = new GameObject("copy");
        SpriteRenderer rend = copy.AddComponent<SpriteRenderer>();
        rend.sprite = sprite;
        copy.transform.parent = transform.parent;
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
        
        Destroy(copy);
        
    }

    IEnumerator createTrailCoroutine()
    {
        for (int i = 0; i < numberOfCopies; i++)
        {
            StartCoroutine(createCopy(whiteSprite));
            yield return new WaitForSeconds(0.02f);
        }
        /*
        Queue<GameObject> copies = new Queue<GameObject>();
        float interval = 0.1f;
        int numberOfCycles = (int)(lifetimeDuration / interval);
        float duartion = 0;
        while (true)
        {
            copies.Enqueue(createCopy(whiteSprite,original.transform));
            
            yield return new WaitForSeconds(interval);
            duartion += interval;
            
            GameObject[] adjustAlpha = copies.ToArray();
           
            for (int i = 0; i < adjustAlpha.Length; i++)
            {
                SpriteRenderer rendCopy = adjustAlpha[i].GetComponent<SpriteRenderer>();
                float alpha = 1 - i * (1 / numberOfCycles);
                rendCopy.color = new Color(1, 1, 1, alpha);
                if (i >= numberOfCycles)
                {
                    GameObject destroy = copies.Dequeue();
                    Destroy(destroy);
                }
            }

            if (duartion >= totalDuration)
            {
                GameObject[] delete = copies.ToArray();
                for (int i = 0; i < delete.Length; i++)
                {
                    Destroy(delete[i]);
                }
                break;
            }
        }
        */
    }

    public void createTrail()
    {
        StartCoroutine(createTrailCoroutine());
    }
}
