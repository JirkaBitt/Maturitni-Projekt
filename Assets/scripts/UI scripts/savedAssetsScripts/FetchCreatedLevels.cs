using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FetchCreatedLevels : MonoBehaviour
{
    // Start is called before the first frame update
    private List<SavedAssets> allSaves = new List<SavedAssets>();
    //this is the selected one
    public SavedAssets selectedAssets;
    //this is a reference to the prefab that is instantiated for every save
    public GameObject assetButton;
    //the scrollview to which we will place the prefabs
    public GameObject scrollview;
    //all the instatiated prefabs
    private List<GameObject> buttons = new List<GameObject>();
    public class SavedAssets
    {
        //keep track of all the assets ints
        public int[][,] assets = new int[8][,];
        public string saveName;
        public string path;
        public SavedAssets(int[][,] input, string name, string pathToFile)
        {
            assets = input;
            saveName = name;
            path = pathToFile;
        }
    }
    void Start()
    {
        FileInfo[] info;
        try
        {
            //retrieve all files that end with .brawlGame
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/");
            info = dir.GetFiles("*.brawlGame");
        }
        catch (Exception e)
        {
            Console.WriteLine("No saves created");
            return;
        }
        foreach (var path in info)
        {
            bool isCorupted = false;
            int[][,] assets = new int[8][,];
            StreamReader reader = new StreamReader(path.FullName);
            //the first line is the nae of the save
            string saveName = reader.ReadLine();
            for (int i = 0; i < 8; i++)
            {
                //every line is a hex string representation of the int array for every asset
                string assetRepresentation = reader.ReadLine();
                if (assetRepresentation == null)
                {
                    isCorupted = true;
                    break;
                }
                //decode it from hex string to 2D int arrray
                assets[i] = DecodeAsset(assetRepresentation);
            }
            reader.Close();
            if (!isCorupted)
            {
                print(path);
                //create new instance of this class that houses the assets
                SavedAssets currentSave = new SavedAssets(assets, saveName,path.FullName);
                allSaves.Add(currentSave);
                //create the button for these assets
                GameObject newButton = Instantiate(assetButton);
                buttons.Add(newButton);
                //put the button in the scrollview
                newButton.transform.parent = scrollview.transform;
                newButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().SetText(saveName);
                newButton.transform.localPosition =
                    new Vector3(-40, -30, 0) - new Vector3(0, 40, 0) * (allSaves.Count - 1);
                newButton.transform.localScale = Vector3.one;
                newButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    //we have selected these assets, load the new scene and mark these assets as the selected ones
                    selectedAssets = currentSave;
                    DontDestroyOnLoad(gameObject);
                    SceneManager.LoadScene("loadCreatedLevel");
                });
                GameObject deleteButton = newButton.transform.GetChild(1).gameObject;
                deleteButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    //we want to delete this save
                    buttons.Remove(newButton);
                    File.Delete(path.FullName);
                    Destroy(newButton);
                    //refresh the buttons position so that there isnt a gap
                    RefreshButtons();
                });
            }
        }
    }
    private void RefreshButtons()
    {
        //move all buttons when one is deleted
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.localPosition = new Vector3(buttons[i].transform.localPosition.x,-30 - 40 * i,0);
        }
    }
    private int[,] DecodeAsset(string hex)
    {
        List<int> intList = new List<int>();
        foreach (var hexChar in hex)
        {
            //every char represents 4 integers
            intList.AddRange(HexToBinary(hexChar));
        }
        int[] intArray = intList.ToArray();
        int length = intArray.Length;
        //convert it from 1D to 2D array
        int[,] final = new int[80, length / 80];
        for (int i = 0; i < length; i++)
        {
            final[i % 80, i / 80] = intArray[i];
        }
        return final;
    }
    private int[] HexToBinary(char hex)
    {
        //this should convert A as 10
        int[] binary = new int[4];
        int hexInt = 0;
        if (hex > '9')
        {
            //we have to remove A bcs the value would be offseted from it, we want to know how far the hex is from A, it is in ASCII table
            //this is in case it is a letter
            hexInt = 10 + hex - 'A';
        }
        else
        {
            //this is in case it is a number
            //find the offset from 0
            hexInt = hex - '0';
        }
        int squareTwo = 8;
        for (int i = 0; i < 4; i++)
        {
            //try to remove the current square of Two
            if (hexInt >= squareTwo)
            {
                binary[i] = 1;
                //we have to remove the square, so we can check again for the smaller one
                hexInt -= squareTwo;
            }
            else
            {
                binary[i] = 0;
            }
            //go to the lower square of two
            squareTwo /= 2;
        }
        return binary;
    }
}
