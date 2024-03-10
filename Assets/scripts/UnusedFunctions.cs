using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnusedFunctions : MonoBehaviour
{
    /*
    Color[,] blackAndWhite(Color[,] source, float midValue)
    {
        //override source and return it in black and white
        //.getLength(0) returns size of X
        //.getLength(1) returns size of Y
        for (int x = 0; x < source.GetLength(0); x++)
        {
            for (int y = 0; y < source.GetLength(1); y++)
            {
                //now compare values
                Color pixel = source[x, y];
                float value = pixel.r + pixel.g + pixel.b;
               

                if (value > midValue)
                {
                    //set pixel to white
                    source[x,y] = Color.white;
                }
                if(value < midValue)
                {
                    //set color to black
                    source[x,y] = Color.black;
                   
                }
                
            }
        }
        print("finished Converting");
        return source;
    }
    Color[] convertToArray(Color[,] source)
    {
        int XLength = source.GetLength(0);
        int YLength = source.GetLength(1);
        //create new array we will store pixels to
        //size should be x*y
        Color[] newArray = new Color[XLength * YLength];
        for (int y = 0; y < YLength; y++)
        {
            for (int x = 0; x < XLength; x++)
            {
                int index = y * XLength + x;
                newArray[index] = source[x, y];
            }
        }
        print(newArray.Length);
        
        return newArray;
    }
    Color[,] reconstructTexture(pixel[,] pixelArray)
    {
        
        int pixelSizeX = pixelArray.GetLength(0);
        int pixelSizeY = pixelArray.GetLength(1);

        //get the number of real pixels inside one pixel class
        int pixelCount = pixelArray[0,0].Size;

        Color[,] returnValue = new Color[pixelSizeX * pixelCount, pixelSizeY * pixelCount];
        for (int y = 0; y < pixelSizeY; y++)
        {
            for (int x = 0; x < pixelSizeX; x++)
            {
                //now we iterate on all members of the array
                pixel currentPixel = pixelArray[x, y];
                //fill in all the pixels
                int startingX = x * pixelCount;
                int startingY = y * pixelCount;
                Color pixelColor = currentPixel.finalColor;
                for (int y2 = 0; y2 < pixelCount; y2++)
                {
                    for (int x2 = 0; x2 < pixelCount; x2++)
                    {
                        returnValue[startingX + x2, startingY + y2] = pixelColor;
                    }
                }
            }
        }

        return returnValue;
    }
    [CanBeNull]
    public Texture2D OpenFinder()
    {
        Texture2D returnValue = null;
        
        return returnValue;
    }
    void destroyPixels(pixel[,] destroyArray)
    {
        foreach (var pix in destroyArray)
        {
            pix.delete();
        }
    }
    IEnumerator waitForCropper(Texture2D source, Action<Texture2D,string> callback, string assetName)
    {
        //callback is a function we want to call on cropped image
        //coroutine so it doesnt block the main thread
        showCropper(source,callback,assetName);
        yield break;
    }
    void generateAsset(Texture2D sourceTexture, string assetName)
    {
        //get pixels from image
        Color[,] pixelArray = getPixels(sourceTexture);
        //pixelcount on X
        int x = pixelArray.GetLength(0);
        //how many pixels are in one big pixel
        int pixelCountInBigPixel = x / numberOfPixelsX;
        //get the middle value of color from this texture
        float mid = getMiddleValue(pixelArray,pixelCountInBigPixel,6);
        //create a gameobject asset
        GameObject parent = new GameObject();
        parent.name = assetName;
        gameLevelPixels = pixelate(pixelArray, pixelCountInBigPixel, mid, parent);
        //make it fit the screen
        fitTheScreen(parent, gameLevelPixels.GetLength(0), gameLevelPixels.GetLength(1));
        //add it to the scene parent
        parent.transform.parent = sceneAssets.transform;
        if (assetName != "Character" && assetName != "Arena" && assetName != "Projectile")
        {
            //this is a weapon, change its tag
            parent.tag = "weapon";
        }
        currentParent = parent;
    }
    void reloadEveryPixel()
    {
        foreach (var pix in gameLevelPixels)
        {
            //we dont want to reload destroyed pixels
            if (pix.isActive)
            {
                pix.reloadColor();
            }
        }
    }
    void destroyWhitePixels(pixel[,] array)
    {
        foreach (var pix in array)
        {
            pix.removeWhite();
        }
    }
     Color[,] getPixels(Texture2D source)
    {
       
        Color[] pixels = source.GetPixels();

        int sourceHeight = source.height;
        int sourceWidth = source.width;
        
        print(sourceHeight + " Heigth");
        print(sourceWidth);
        Color[,] pixel2dArray = new Color[sourceWidth, sourceHeight];

        //swap source height with width because it is rotated 90 degrees
        for (int y = 0; y < sourceHeight; y++)
        {
           
            for (int x = 0; x < sourceWidth; x++)
            {
                //multiply the sourceWidth with height to get to the right row and the check width to determine on which pixel we are
               
                int currentPixelIndex = y * sourceWidth + x;//x * sourceWidth + y;
                pixel2dArray[x, y] = pixels[currentPixelIndex];
                
            }
        }
        
        return pixel2dArray;
    }
    float getMiddleValue(Color[,] colorArray, int pixelSize, int numberOfPixelsX)
    {
        //set minValue to max so that foreach changes it, same for max value
        //maxvalue = 0 so that a bigger one overrides it
        //min value = 0 pitch black
        float minValue = 3*1;
        //max value is 1 for all 3 colors
        float maxValue = 0;
        
        foreach (var pixel in colorArray)
        {
            //add all 3 color values, the bigger the brighter
            //update we inly want the red part because shadows are mostly red and this should eliminate them
            float value = pixel.r;// + pixel.g + pixel.b;
            if (value > maxValue)
            {
                maxValue = value;
            }

            if (value < minValue)
            {
                minValue = value;
            }
        }
        //return middle point
        return (minValue + maxValue) / 2;
    }
    void showCropper(Texture2D cropThis, Action<Texture2D,string> callback, string assetName)
    {
        //show the cropper
       
        bool finished = false;
        string text = "Please select the " + assetNames[assetIndex];
        ImageCropper.Instance.Show(cropThis, text,(bool result, Texture originalImage, Texture2D croppedImage) =>
        {
            //this is the result of the crop
            if (result)
            {
                //we have succesfully cropped an image
                //call back will be getlevel
                if (croppedImage != null)
                {
                    callback(croppedImage, assetName);
                }
                else
                {
                    //we want to show the default asset here
                    addDefaulAsset(assetIndex);
                }

            }
            else
            {
                //we have canceled the cropping
                //set the previous gameobject active
                int childCount = sceneAssets.transform.childCount;
                if (childCount > 0)
                {
                    assetIndex--;
                    sceneAssets.transform.GetChild(childCount - 1).gameObject.SetActive(true);
                }
                
            }

            finished = true;
        });
        
    }
    //we need this to return two values from showCropper function
    
    void fitTheScreenDefault(GameObject parentObject)
    {
        assetInfo info = parentObject.GetComponent<assetInfo>();
        info.turnOffAnimation();
        info.calculateSize();
        Vector3 assetScale = info.startScale;
        //we want to divide by scale to get the dimensions if scale was 1
        float height = info.height ;
        float width = info.width ;
        
        Vector2 screenSize = calculateSceneSize();
        float scaleX = screenSize.x / width;
        float scaleY = screenSize.y / height;
        float scale = 1;
        //we want to use the smaller value so that we hit the screen
        if (scaleX > scaleY)
        {
            scale = scaleY;
        }
        else
        {
            scale = scaleX;
        }

    
        
        parentObject.transform.position = Vector3.zero;
        //scale the level to fit the screen
        parentObject.transform.localScale = Vector3.one * scale;
    }
    void addDefaulAsset(int index)
    {
        gameLevelPixels = null;
        GameObject asset = Instantiate(defaultAssets[assetIndex], new Vector3(0, 0, 0), Quaternion.identity);
        currentParent = asset;
        fitTheScreenDefault(asset);
        asset.transform.parent = sceneAssets.transform;
        asset.name = assetNames[assetIndex];
    }
     pixel[,] pixelate(Color[,] input, int pixelSize, float mid, GameObject parentObject)
    {
        //size of individual pixels
        int arraySizeX = input.GetLength(0);
        int arraySizeY = input.GetLength(1);

        //number of big pixels on each axis
        int pixelsX = arraySizeX / pixelSize;
        int pixelsY = arraySizeY / pixelSize;
        pixel[,] ClassArray = new pixel[pixelsX, pixelsY];
        //get hpw many pixels are in one midValue area
        //int areaX = midValues.GetLength(0);
        //areaX is total number of areas on x, if we divide totalpixels with this we get pixels per area on x
        //int pixelsInArea = pixelsX / areaX;
        print("pixelSize: " + pixelSize);
        for (int startY = 0; startY < pixelsY; startY++)
        {
            for (int startX = 0; startX < pixelsX; startX++)
            {
                
                //first count the pixels in the big pixel
                int numberOfBlackPixels = 0;
                //create a new instance of the class
                //mid = midValues[startX / pixelsInArea, startY / pixelsInArea];
               // mid -= mid / 4;
                pixel thisPixel = new pixel(startX, startY, pixelSize, Color.white, parentObject);
                for (int y = 0; y < pixelSize; y++)
                {
                    for (int x = 0; x < pixelSize; x++)
                    {
                        //get the current index of an area
                        //divide without leftover.... 2/3 = 0, 4/3 = 1 ...
                        // dividing two int gives an int
                        
                        if (mid <= 0)
                        {
                            mid = 0.1f;
                        }
                        //we have to check that we are not outside of the bounds
                        int indexX = startX * pixelSize + x;
                        int indexY = startY * pixelSize + y;
                       
                        if (indexX < arraySizeX && indexY < arraySizeY)
                        {
                            Color currentPixel = input[indexX, indexY];
                            float value = currentPixel.r;//+ currentPixel.g + currentPixel.b;
                            if (value < mid)
                            {
                                //blackPixel
                                numberOfBlackPixels++;
                                
                                //set the pixel to white so that if we dont have enough black pixels we dont have to go throw the array again and set every pixel to white
                            }
                           
                        }
                    }
                }
                //specify the number of black pixels the big pixel must have to turn black
                if (numberOfBlackPixels > pixelSize )
                {
                    //change the value of final color
                    if (numberOfBlackPixels < pixelSize * pixelSize)
                    {
                        thisPixel.finalColor = Color.black;
                        thisPixel.changeColor(Color.black);
                    }
                  
                }
                //add the pixel to an array
                ClassArray[startX, startY] = thisPixel;
                
            }
        }

        return ClassArray;
    }
*/
}
