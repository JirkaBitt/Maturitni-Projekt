using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class savePopUpButtons : MonoBehaviour
{
    //button for saving the assets
    public GameObject saveButton;
    //dont save them and start the game
    public GameObject notSaveButton;
    //the object that houses the analyze image script
    public GameObject controller;
    //the inputfield for the save name
    public GameObject inputName;
    //warning thext if the name is empty
    public GameObject warning;
    //if this is true we already have a save and want to update it
    public bool onlyUpdate = false;
    public GameObject loadingScreen;
    // Start is called before the first frame update
    void Start()
    {
        if (onlyUpdate)
        {
            //destroy the save and create a new one
            //saved assets houses the original save that was modified
            GameObject saveObj = GameObject.Find("savedAssets");
            GameObject holder = GameObject.Find("AssetHolder");
            assetHolder saveAssetHolder = holder.GetComponent<assetHolder>();
            fetchCreatedLevels dataHolder = saveObj.GetComponent<fetchCreatedLevels>();
            analyzeImage imageScript = controller.GetComponent<analyzeImage>();
            saveButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                loadingScreen.SetActive(true);
                //delete the existing file and create new one with the same name
                File.Delete(dataHolder.selectedAssets.path);
                saveAssetHolder.saveAssets(dataHolder.selectedAssets.saveName);
                //we dont need saveObj any more
                Destroy(saveObj);
                imageScript.createRoom();

            });
            notSaveButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                loadingScreen.SetActive(true);
                Destroy(saveObj);
                imageScript.createRoom();
            });
        }
        else
        {
            //we want to create a new save
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
                loadingScreen.SetActive(true);
                saveAssetHolder.saveAssets(inputText.text);
                imageScript.createRoom();
            });
            notSaveButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                loadingScreen.SetActive(true);
                imageScript.createRoom();
            });
        }
    }
}
