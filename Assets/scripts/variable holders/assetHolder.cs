
using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class assetHolder : MonoBehaviour
{
    public Dictionary<string, int[,]> assets = new Dictionary<string, int[,]>();
    public void saveAssets(string name)
    {
        //check how many saves already exist
        string[] existingAssets = Directory.GetFiles(@Application.persistentDataPath + "/");
        //we want to add a number at the end to differentiate them
        int currentIndex = existingAssets.Length;
        string path = Application.persistentDataPath + "/created"+ currentIndex +".brawlGame";
        FileStream file = File.Create(path);
        StreamWriter writer = new StreamWriter(file);
        //the fist line is the save name
        writer.WriteLine(name);
        foreach (var asset in assets)
        {
            //supply the int array and create a string hexadecimal representation to save space
            string saveThis = assetToHex(asset.Value);
            writer.WriteLine(saveThis);
        }
        writer.Close();
    }

    private string assetToHex(int[,] input)
    {
        //this takes an int and divides it into 4 space int arrays and creates a hex representation
        string hexRepresentation = "";
        int currentIndex = 0;
        int[] intArray = new int[4];
        int Xlength = 80;
        int Ylength = input.GetLength(1);
        for (int y = 0; y < Ylength; y++)
        {
            for (int x = 0; x < Xlength; x++)
            {
                intArray[currentIndex] = input[x,y];
                if (currentIndex == 3)
                {
                    //this is the forth int
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
        int hex = 0;
        for (int i = 0; i < 4; i++)
        {
            //create an int from 0 to 16 based on the binary
            //if the fisrt was 1 then after 3 iterations it will be 8
            hex *= 2;
            hex += binary[i];
        }
        if (hex > 9)
        {
            return (Convert.ToChar('A' + (hex - 10))).ToString();
        }
        else
        {
            return hex.ToString();
        }
    }
}
