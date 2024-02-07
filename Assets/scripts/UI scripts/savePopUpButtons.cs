using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class savePopUpButtons : MonoBehaviour
{
    public GameObject saveButton;

    public GameObject notSaveButton;

    public GameObject controller;

    public GameObject inputName;

    public GameObject warning;
    // Start is called before the first frame update
    void Start()
    {
        GameObject holder = GameObject.Find("AssetHolder");
        assetHolder saveAssetHolder = holder.GetComponent<assetHolder>();

        analyzeImage imageScript = controller.GetComponent<analyzeImage>();
        
        saveButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            TMP_InputField inputText = inputName.GetComponent<TMP_InputField>();
            inputText.onValueChanged.AddListener((arg0 =>
            {
                warning.SetActive(false);
            }));
            if (inputText.text == "")
            {
                warning.SetActive(true);
                return;
            }
            saveAssetHolder.saveAssets(inputText.text);
            imageScript.createRoom();
          
        });
        
        notSaveButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            imageScript.createRoom();
        });
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
