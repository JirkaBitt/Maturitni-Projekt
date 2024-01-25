using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnPlatforms : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] platforms;
    public float minX;
    public float maxX;
    public bool spawning = true;
    void Start()
    {
        StartCoroutine(spawnPlatform());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator spawnPlatform()
    {
        while (true)
        {
            if (spawning)
            {
                int randomIndex = Random.Range(0, platforms.Length - 1);

                float randomX = Random.Range(minX, maxX);
                GameObject platform = Instantiate(platforms[randomIndex]);
                platform.SetActive(true);
                
                float randomSpeed = Random.Range(0.4f, 1f);
                platform.transform.localScale *= randomSpeed;
                platform.GetComponent<floatUpwards>().speed = randomSpeed;

                Vector3 pos = platform.transform.position;
                platform.transform.position = new Vector3(randomX, pos.y, pos.z);
            }
            int randomWait = Random.Range(6, 12);
            yield return new WaitForSeconds(randomWait);
        }
    }
}
