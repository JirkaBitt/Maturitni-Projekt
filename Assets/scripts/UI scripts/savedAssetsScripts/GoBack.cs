
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBack : MonoBehaviour
{
    public bool cancelEditing = false;
    public GameObject analyzeImageObj;
    public void goBacktoChooseLevel()
    {
        if (cancelEditing)
        {
            //if we are editing an asset we just want to cancel it
            cancelEditing = false;
            AnalyzeImage imageScript = analyzeImageObj.GetComponent<AnalyzeImage>();
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
            AnalyzeImage imageScript = analyzeImageObj.GetComponent<AnalyzeImage>();
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
