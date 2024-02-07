using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public bool onlyUpdate = false;
    // Start is called before the first frame update
    void Start()
    {
        if (onlyUpdate)
        {
            GameObject saveObj = GameObject.Find("savedAssets");
            GameObject holder = GameObject.Find("AssetHolder");
            assetHolder saveAssetHolder = holder.GetComponent<assetHolder>();
            fetchCreatedLevels dataHolder = saveObj.GetComponent<fetchCreatedLevels>();
            analyzeImage imageScript = controller.GetComponent<analyzeImage>();
            saveButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                //delete the existing file and create new one with the same name
                File.Delete(dataHolder.selectedAssets.path);
                saveAssetHolder.saveAssets(dataHolder.selectedAssets.saveName);
                //we dont need saveObj any more
                Destroy(saveObj);
                imageScript.createRoom();

            });
            notSaveButton.GetComponent<Button>().onClick.AddListener(() => { imageScript.createRoom(); });
        }
        else
        {
            GameObject holder = GameObject.Find("AssetHolder");
            assetHolder saveAssetHolder = holder.GetComponent<assetHolder>();

            analyzeImage imageScript = controller.GetComponent<analyzeImage>();

            saveButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                //check if the name is not null and then save the asset
                TMP_InputField inputText = inputName.GetComponent<TMP_InputField>();
                inputText.onValueChanged.AddListener((arg0 => { warning.SetActive(false); }));
                if (inputText.text == "")
                {
                    warning.SetActive(true);
                    return;
                }
                saveAssetHolder.saveAssets(inputText.text);
                imageScript.createRoom();

            });
            notSaveButton.GetComponent<Button>().onClick.AddListener(() => { imageScript.createRoom(); });
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
