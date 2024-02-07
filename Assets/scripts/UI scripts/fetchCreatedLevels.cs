using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class fetchCreatedLevels : MonoBehaviour
{
    // Start is called before the first frame update
    private List<savedAssets> allSaves = new List<savedAssets>();

    public savedAssets selectedAssets;

    public GameObject assetButton;

    public GameObject scrollview;
    public class savedAssets
    {
        public int[][,] assets = new int[8][,];
        public string saveName;
        
        public savedAssets(int[][,] input, string name)
        {
            assets = input;
            saveName = name;
        }
    }
    void Start()
    {
        FileInfo[] info;
        try
        {
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/");
            info = dir.GetFiles("*.brawlGame");
        }
        catch (Exception e)
        {
            Console.WriteLine("No saves created");
            return;
        }
        //string[] existingAssets = info.Directory.GetFiles(@Application.persistentDataPath + "/");
        foreach (var path in info)
        {
            bool isCorupted = false;
            int[][,] assets = new int[8][,];
            StreamReader reader = new StreamReader(path.FullName);
            string saveName = reader.ReadLine();
            print(saveName);
            for (int i = 0; i < 8; i++)
            {
                string assetRepresentation = reader.ReadLine();
                if (assetRepresentation == null)
                {
                    isCorupted = true;
                    break;
                }
                assets[i] = decodeAsset(assetRepresentation);
            }
            reader.Close();
            if (!isCorupted)
            {
                savedAssets currentSave = new savedAssets(assets, saveName);
                allSaves.Add(currentSave);

                GameObject newButton = Instantiate(assetButton);
                newButton.transform.parent = scrollview.transform;
                newButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().SetText(saveName);
                newButton.transform.localPosition =
                    new Vector3(0, -60, 0) - new Vector3(0, 100, 0) * (allSaves.Count - 1);
                newButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    selectedAssets = currentSave;
                    DontDestroyOnLoad(gameObject);
                    SceneManager.LoadScene("loadCreatedLevel");

                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private int[,] decodeAsset(string hex)
    {
        List<int> intList = new List<int>();
        foreach (var hexChar in hex)
        {
            intList.AddRange(HexToBinary(hexChar));
        }

        int[] intArray = intList.ToArray();
        int length = intArray.Length;

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
            hexInt = 10 + hex - 'A';
        }
        else
        {
            hexInt = hex - '0';
        }
        if (hexInt >= 8)
        {
            binary[0] = 1;
            hexInt -= 8;
        }
        else
        {
            binary[0] = 0;
        }
        if (hexInt >= 4)
        {
            binary[1] = 1;
            hexInt -= 4;
        }
        else
        {
            binary[1] = 0;
        }
        if (hexInt >= 2)
        {
            binary[2] = 1;
            hexInt -= 2;
        }
        else
        {
            binary[2] = 0;
        }
        if (hexInt >= 1)
        {
            binary[3] = 1;
            hexInt -= 1;
        }
        else
        {
            binary[3] = 0;
        }

        if (hexInt != 0)
        {
            print("isnt equal to zero!!!!!!!!!!!!!!!!");
        }
        return binary;
    }
    
}
