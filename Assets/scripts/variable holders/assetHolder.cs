using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class assetHolder : MonoBehaviour
{
    // Start is called before the first frame update
    public Dictionary<string, int[,]> assets = new Dictionary<string, int[,]>();
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void saveAssets(string name)
    {
        //check if we have valid name
        
        //now we know we have a valid name
        string[] existingAssets = Directory.GetFiles(@Application.persistentDataPath + "/");
        print(existingAssets.Length + "all paths!!!!!!!!");
        int currentIndex = existingAssets.Length;
        string path = Application.persistentDataPath + "/created"+ currentIndex +".brawlGame";
        print(path);
        FileStream file = File.Create(path);
        
        StreamWriter writer = new StreamWriter(file);
      
        writer.WriteLine(name);
        print(assets.Count + "aseets count");
        foreach (var asset in assets)
        {
            string saveThis = assetToHex(asset.Value);
            writer.WriteLine(saveThis);
            print("saved-" + asset.Key);
        }
        writer.Close();
        //now we start the game
       
    }

    private string assetToHex(int[,] input)
    {
        //this takes an int and divides it into 4 int arrays and creates a hex representation
        string hexRepresentation = "";
        int currentIndex = 0;
        int[] intArray = new int[4];
        int Xlength = 80;
        int Ylength = input.GetLength(1);
        /*
        foreach (var pixel in input)
        {
            intArray[currentIndex] = pixel;
            if (currentIndex == 3)
            {
                hexRepresentation += convertToHex(intArray);
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
        }
        */
        for (int y = 0; y < Ylength; y++)
        {
            for (int x = 0; x < Xlength; x++)
            {
                intArray[currentIndex] = input[x,y];
                if (currentIndex == 3)
                {
                    hexRepresentation += convertToHex(intArray);
                    currentIndex = 0;
                }
                else
                {
                    currentIndex++;
                }
            }
        }
        
        return hexRepresentation;
    }
    private string convertToHex(int[] binary)
    {
        //this func takes 4 ints and converts them into hex number
        string returnValue = "0";
        int hex = 0;
        for (int i = 0; i < 4; i++)
        {
            hex *= 2;
            hex += binary[i];
        }

        if (hex > 9)
        {
            //returnValue = ('A' + hex - 10).ToString();
            
            switch (hex)
            {
                case 10: returnValue = "A";
                    break;
                case 11: returnValue = "B";
                    break;
                case 12: returnValue = "C";
                    break;
                case 13: returnValue = "D";
                    break;
                case 14: returnValue = "E";
                    break;
                case 15: returnValue = "F";
                    break;
            }
            
        }
        else
        {
            returnValue = hex.ToString();
        }
        return returnValue;
    }
}
