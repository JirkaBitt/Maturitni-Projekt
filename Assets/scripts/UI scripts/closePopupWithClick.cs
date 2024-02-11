using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class closePopupWithClick : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject popup;
    private bool isActive = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startPopup()
    {
       
            StartCoroutine(showPopup());
        
       
        
    }
    IEnumerator showPopup()
    {
        if (!isActive)
        {
            popup.SetActive(true);
            isActive = true;
            //close the popup when we click the mouse
            
            yield return new WaitUntil(() => Input.GetMouseButton(0));
            popup.SetActive(false);

            //there has to be this second wait for the button to be released bcs otherwise if we click the button the popup would disappear and show again
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
            isActive = false;
        }
        else
        {
            popup.SetActive(false);
            isActive = false;
        }
    }
}
