using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class popupController : MonoBehaviour
{
    public GameObject buttons;
    public void closePopUp()
    {
        buttons.SetActive(true);
        gameObject.SetActive(false);
    }
}
