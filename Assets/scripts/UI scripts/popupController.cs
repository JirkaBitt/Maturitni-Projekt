using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class popupController : MonoBehaviour
{
    //private bool shownBefore;
    public GameObject buttons;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void closePopUp()
    {
        buttons.SetActive(true);
        gameObject.SetActive(false);
    }
}
