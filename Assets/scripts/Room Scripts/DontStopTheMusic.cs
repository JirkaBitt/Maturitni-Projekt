using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontStopTheMusic : MonoBehaviour
{
    // Start is called before the first frame update
    public bool alreadyExists = false;
    void Start()
    {
        if (!alreadyExists)
        {
             DontDestroyOnLoad(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
