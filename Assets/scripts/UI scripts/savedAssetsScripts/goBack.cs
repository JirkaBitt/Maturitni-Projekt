using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class goBack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void goBacktoChooseLevel()
    {
        SceneManager.LoadScene("ChooseLevel");
    }
    public void goBacktoSaved()
    {
        GameObject holder = GameObject.Find("savedAssets");
        Destroy(holder);
        SceneManager.LoadScene("savedLevels");
    }
}
