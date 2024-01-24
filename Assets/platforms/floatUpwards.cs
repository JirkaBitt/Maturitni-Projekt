using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatUpwards : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    public bool goUp = true;
    public bool hover;

    private Vector3 startPos;
    public Vector3 hoverRange = new Vector3(0,1,0);
    void Start()
    {
        startPos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (goUp)
        {
            if (gameObject.transform.position.y > 20)
            {
                Destroy(gameObject);
            }
            gameObject.transform.position += Vector3.up * speed * Time.deltaTime;
        }

    }
}
