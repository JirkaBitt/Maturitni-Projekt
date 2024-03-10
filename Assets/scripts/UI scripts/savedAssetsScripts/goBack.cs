
using UnityEngine;
using UnityEngine.SceneManagement;

public class goBack : MonoBehaviour
{
    // Start is called before the first frame update
    public bool cancelEditing = false;
    public GameObject analyzeImageObj;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void goBacktoChooseLevel()
    {
        if (cancelEditing)
        {
            //if we are editing an asset we just want to cancel it
            cancelEditing = false;
            analyzeImage imageScript = analyzeImageObj.GetComponent<analyzeImage>();
            imageScript.returnFromEditing();
        }
        else
        {
            //return back to lobby
            SceneManager.LoadScene("ChooseLevel");
        }
    }
    public void goBacktoSaved()
    {
        if (cancelEditing)
        {
            //if we are editing an asset we just want to cancel it
            cancelEditing = false;
            analyzeImage imageScript = analyzeImageObj.GetComponent<analyzeImage>();
            imageScript.returnFromEditing();
        }
        else
        {
            GameObject holder = GameObject.Find("savedAssets");
            Destroy(holder);
            SceneManager.LoadScene("savedLevels");
        }
       
    }
}
