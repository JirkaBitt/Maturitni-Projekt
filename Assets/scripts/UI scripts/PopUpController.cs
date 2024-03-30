using UnityEngine;

public class PopUpController : MonoBehaviour
{
    public GameObject buttons;
    public void ClosePopUp()
    {
        buttons.SetActive(true);
        gameObject.SetActive(false);
    }
}
