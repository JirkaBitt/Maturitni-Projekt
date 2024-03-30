using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBack : MonoBehaviour
{
    public bool cancelEditing = false;
    public GameObject analyzeImageObj;
    public void GoBacktoChooseLevel()
    {
        if (cancelEditing)
        {
            //if we are editing an asset we just want to cancel it
            cancelEditing = false;
            AnalyzeImage imageScript = analyzeImageObj.GetComponent<AnalyzeImage>();
            imageScript.ReturnFromEditing();
        }
        else
        {
            //return back to lobby
            SceneManager.LoadScene("ChooseLevel");
        }
    }
    public void GoBacktoSaved()
    {
        if (cancelEditing)
        {
            //if we are editing an asset we just want to cancel it
            cancelEditing = false;
            AnalyzeImage imageScript = analyzeImageObj.GetComponent<AnalyzeImage>();
            imageScript.ReturnFromEditing();
        }
        else
        {
            GameObject holder = GameObject.Find("savedAssets");
            Destroy(holder);
            SceneManager.LoadScene("savedLevels");
        }
       
    }
}
