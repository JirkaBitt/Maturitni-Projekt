using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using GameObject = UnityEngine.GameObject;
using Screen = UnityEngine.Screen;



public class DisplayAI : MonoBehaviour
{
// the number of pixels on X axis, Y axis will be calculated based on screen height
    public int numberOfPixelsX = 72;
//current assets pixels used for raycast and recoloring
    private Pixel[,] gameLevelPixels;
//current tool changed by buttons from ChangeTools script
    public String currentTool = "Pencil";
//sceneAssets is the parent that holds all assets
    private GameObject sceneAssets;
//finder texture is the original fullsize image from user photo library
    private Texture2D finderTexture;
//reloadThese is a list of highlighted pixels that should be reset in the next frame
    private List<Pixel> reloadThese = new List<Pixel>();
//this is a holder for the previous position of our mouse when we make the assets, it is used in receiveskipped
    private Vector2 previousRaycastPos = Vector2.zero;
//assetNames is an array of assets we want to create
    public string[] assetNames = new[] { "Arena","Character","Sword","Axe","Spear","Gun","Projectile","Bomb" };
//assetIndex is an int that points to the current asset
    public int assetIndex = 0;
//the pointer to the current asssets parent
    private GameObject currentParent;
//list of created assets
    private List<Prefab> assetPrefabs = new List<Prefab>();
//we want to stop the raycast if we display the assets at the end
    private bool displayed = false;
//frame Prefab
    public GameObject frame;
//a list of default assets if user decides to skip, we have to keep order in this list
    public GameObject[] defaultAssets;
//list of higthligthed frames we have to reset in the next frame
    private List<SpriteRenderer> reloadFrames = new List<SpriteRenderer>();
//parent of ui buttons
    public GameObject buttonUI;
//reference to the displayes that are behind assets
    private GameObject[] frames;
//this bool serves as a check, when we maximazi asset, we dont want to recolor until the mouse is lifted
    private bool addPixels = true;
//this is a script that controls the switch between pencil and eraser and cursor size
    private ChangeTools toolsScript;
//photon lobby is used for joining and creating rooms
    public GameObject photonLobby;
//the radius of pencil or eraser, it is changed from ChangeTools script
    public float toolRadius = 1;
//slate is a reference to the drawing pixels
    public GameObject slate;
// this is the text that tells us what to draw 
    public GameObject assetDisplayText; 
//this is if we only want the character, in case we are joining a game 
    public bool onlyCharacter = false;
// this is activated when we clicked move to the next but havent drawn a single pixel, we have to draw at least 1 pixel
    public GameObject warning;
// this will be set to true if we are loading from saved, we will bypass the creation procces and show the overview right away
    public bool loadLevelsFromSaves = false;
//this is a reference to the popup object that allows us to save the created assets for later use
    public GameObject savePopup;
//we have to keep track if we have modified the assets, so that we know if it makes sense to ask the user if he wants to update the saves
    private bool hasEditedAssets = false;
// reference to the button that allows us to go back to choose level or to the overview if we are modifiing an asset
    public GameObject goBackButton;
//loading screen will be shown when we click start game when the proccessing happens
    public GameObject loadingScreen;
//this is a reference to the inputfield where players input their names
    public GameObject inputName;

    public GameObject nicknamePopUp;
    public  PathFinder pathFinder;
    public int[,] intArena;

    private string hexArena =
        "00000000000000000000000000000007FFFF8000000000000FFFFFFFFF000FFFFFFFFFFFFFFFFF801FFFFFFFFFFFFFFFFFC03FFFFFFFFFFFFFFFFFC03FFFFFFFFFFFFFFFFFC03FFFFFFFFFFFFFFFFFC03FFFFFFFFFFFFFFFFFC03FFFFFFFFFFFFFFFFFC03FFFFFFFFFFFFFFFFFC03FFFFFFFFFFF801FFF801FFFFFFFFFFF000FFF000FFFFFF8FF80000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001FF80000FFFFF000007FFFFC0001FFFFF80001FFFFFC0001FFFFF80003FFFFFC0001FFFFF80003FFFFFC0001FFFFF80003FFFFF80000FFFFF00003FFFFF0000000FFC00003FFFE0000000000000003F0000000000000000003F0000000000000000003F0000000000000000003F0000000000000000000E000000000000000000000000000000000000000000000000000000000000000000000000000000";
//the Prefab class houses the necessary information for each asset and manipulates(maximize, Minimalize, refresh) 
    class Prefab
    {
        public string name;
        //this is the object that is shown and that we will manipulate
        public GameObject objectPrefab; 
        //pixel class is a reference to the slate that we draw on, we get the info about the pixels from there
        public Pixel[,] pixelClass;
        //information for manipulating the object
        public Vector3 fullScreenScale;
        public Vector3 smallScreenPosition;
        public Vector3 smallScreenScale;
        
        private Vector2 firstBlackPixel = Vector2.zero;
        //reference to the drawing object
        private GameObject slate;
        private int[,] colors;
        public Prefab(GameObject slatee,GameObject objectt, Pixel[,] array, string n)
        {
            objectPrefab = objectt;
            pixelClass = array;
            name = n;
            slate = slatee;
            objectPrefab.SetActive(false);
            int width = pixelClass.GetLength(0);
            int height = pixelClass.GetLength(1);
            colors = new int[width,height];
            // we only need the ints from pixels, 1 is black, 0 is white
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixelClass[x, y].finalColor == Color.black)
                    {
                        colors[x, y] = 1;
                    }
                    else
                    {
                        colors[x, y] = 0;
                    }
                }
            }
            //create the texture from supplied array of pixels
            CombineSpriteArray(objectPrefab, colors);
        }

        public void FitTheScreen()
        {
            int width = pixelClass.GetLength(0);
            int height = pixelClass.GetLength(1);
            //set the drawing space to active and show what we have drawn
            objectPrefab.SetActive(false);
            slate.SetActive(true);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixelClass[x, y].finalColor = colors[x, y] == 1 ? Color.black : Color.white;
                    pixelClass[x,y].ReloadColor();
                    pixelClass[x,y].RemoveWhite();
                }
            }
        }
        public void RefreshAsset()
        {
            //Remake the texture and update the int array
            int xLength = pixelClass.GetLength(0);
            int yLength = pixelClass.GetLength(1);
            for (int y = 0; y < yLength; y++)
            {
                for (int x = 0; x < xLength; x++)
                {
                    colors[x, y] = pixelClass[x, y].finalColor == Color.black ? 1 : 0;
                }
            }
            CombineSpriteArray(objectPrefab, colors);
        }

        public void AddSavedAsset(int[,] savedInts)
        {
            //we have loaded assets from saves, we want to display them right away
            colors = savedInts;
            CombineSpriteArray(objectPrefab, savedInts);
        }
        public void Minimalize()
        {
            // show the asset in the overview
            objectPrefab.SetActive(true);
            objectPrefab.transform.position = smallScreenPosition;
            objectPrefab.transform.localScale = smallScreenScale;
            
        }
        public void ComputeMinimalize(float screenWidth,int assetCount, int index)
        {
            //compute where this asset should be placed in the overview
            float assetWidth = screenWidth / assetCount;
            smallScreenPosition = new Vector3(assetWidth * index - screenWidth/2 + assetWidth/2, 0, 0);
            smallScreenScale = fullScreenScale / assetCount;
            Minimalize();
        }
        //combine sprite array creates an texture from the supplied array of color 
        public void CombineSpriteArray(GameObject attach, int[,] colorsRPC)
        {
            //this function creates a texture from the supplied ints
                //find the first and last black pixel, so that we know how wide and tall the texture should be
                int firstX = colors.GetLength(0);
                int lastX = 0;
                int firstY = colors.GetLength(1);
                int lastY = 0;

                int width = firstX;
                int height = firstY;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (colors[x,y] == 1)
                        {
                            if (y < firstY)
                            {
                                firstY = y;
                            }
                            if (y > lastY)
                            {
                                lastY = y;
                            }
                            if (x < firstX)
                            {
                                firstX = x;
                            }
                            if (x > lastX)
                            {
                                lastX = x;
                            }
                        }
                    }
                }
                //we have add 1 bcs otherwise the sprite is shortened
                lastY++;
                lastX++;
                firstBlackPixel = Vector2.zero;
                bool foundCorner = false;
                int spritesWidth = pixelClass[0, 0].Size;
                int spritesHeight = spritesWidth;
                //create the texture size
                Vector2 texturePixelSize = new Vector2(lastX - firstX, lastY - firstY);
                Texture2D combinedTexture = new Texture2D(spritesWidth * (int)texturePixelSize.x,
                    spritesHeight * (int)texturePixelSize.y);
                //create the texture components
                Color32[] black = new Color32[spritesHeight * spritesWidth];
                Color32[] transparent = new Color32[spritesHeight * spritesWidth];
                for (int i = 0; i < black.Length; i++)
                {
                    black[i] = Color.black;
                    transparent[i] = Color.clear;
                }
                
                for (int y = firstY; y < lastY; y++)
                {
                    for (int x = firstX; x < lastX; x++)
                    {
                        if (colorsRPC[x, y] == 1)
                        {
                            //we want to know the coordinations of the first black pixel from left bottom corner
                            if (!foundCorner)
                            {
                                //find the first corner pixel in the array
                                foundCorner = true;
                                firstBlackPixel = new Vector2((x-firstX) * spritesWidth * 0.01f, (y-firstY) * spritesHeight * 0.01f);
                            }
                            combinedTexture.SetPixels32(x * spritesWidth - firstX * spritesWidth, y * spritesHeight - firstY *spritesHeight, spritesWidth,
                                spritesHeight,
                                black);
                        }
                        else
                        {
                          //  set it transparent
                            combinedTexture.SetPixels32(x * spritesWidth- firstX * spritesWidth, y * spritesHeight - firstY *spritesHeight, spritesWidth,
                                spritesHeight,
                                transparent);
                        }
                    }
                }

                combinedTexture.Apply();

                Sprite final = Sprite.Create(combinedTexture,
                    new Rect(0.0f, 0.0f, combinedTexture.width, combinedTexture.height), new Vector2(0.5f, 0.5f));
                final.name = name + "Sprite";

                if (attach.GetComponent<SpriteRenderer>() != null)
                {
                    attach.GetComponent<SpriteRenderer>().sprite = final;
                }
                else
                {
                    SpriteRenderer ren = attach.AddComponent<SpriteRenderer>();
                    ren.sprite = final;
                }
                //go from the middle of the sprite to the bottom corner and from there navigate with firstblackPixel to the first corner of the real sprite, that is the starting position
              
               firstBlackPixel = (Vector2)final.bounds.center -
                   new Vector2(combinedTexture.width / 2, combinedTexture.height / 2) * 0.01f + firstBlackPixel;
            
        }
        //find next corner works with direction, pixel index and corner index to determine the next index of the corner and the direction we want to continue
        public void Finalize(AssetHolder holder)
        {
            // destroy the object, we dont need it anymore
            Destroy(objectPrefab);
            //add the int array with the name to the object that will go to the next scene where it will create the final asset
            holder.assets.Add(name,colors);
        }
    }
    void Start()
    {
        //create the parent
        sceneAssets = new GameObject();
        sceneAssets.name = "Scene Assets";
        sceneAssets.tag = "created";
        CreateSlate();
    }
    //create slate is called at the start and creates a canvas of pixels to draw to
    private void CreateSlate()
    {
        GameObject parent = new GameObject();
        parent.name = "Slate";
        int pixelWidth = (int)(Screen.width * (1 - 0.2f)) / numberOfPixelsX;
        //big pixels in y
        intArena = DecodeAsset(hexArena);
        int pixelsY = intArena.GetLength(1);
        gameLevelPixels = new Pixel[numberOfPixelsX, pixelsY];
        for (int y = 0; y < pixelsY; y++)
        {
            for (int x = 0; x < numberOfPixelsX; x++)
            {
                Color pixColor = intArena[x,y] == 1 ? Color.black : Color.white;
                gameLevelPixels[x,y] = new Pixel(x, y, pixelWidth, pixColor, parent);
                
                gameLevelPixels[x,y].ReloadColor();
                gameLevelPixels[x,y].RemoveWhite();
            }
        }
        //make it fit the screen
        FitTheScreen(parent, numberOfPixelsX, pixelsY);
        //add it to the scene parent
        parent.transform.parent = sceneAssets.transform;
        currentParent = parent;
        slate = parent;
        pathFinder.AddBarier(5,intArena);
        pathFinder.StartCoroutine("Follow");
    }

    public void ChangePixel(Color newColor, int x, int y)
    {
        if (x < gameLevelPixels.GetLength(0) && y < gameLevelPixels.GetLength(1))
        {
            gameLevelPixels[x,y].pixelObject.SetActive(true);
            Destroy(gameLevelPixels[x,y].pixelObject.GetComponent<Collider2D>());
            gameLevelPixels[x,y].ChangeColor(newColor);
        }
    }
    //refresh slate makes every pixel in the canvas white again
    private void refreshSlate(string assetName)
    {
        previousRaycastPos = Vector2.zero;
        foreach (var pix in gameLevelPixels)
        {
            pix.finalColor = Color.white;
            pix.ReloadColor();
        }
        TextMeshProUGUI text = assetDisplayText.GetComponent<TextMeshProUGUI>();
        text.SetText("Please draw the " + assetName);
    }
    //Remake allows the user to refresh the slate at the current asset and draw again
    //submit change is called when we click the submit button when editing an asset
    void FitTheScreen(GameObject parentObject, float numberX, float numberY)
   {
        //Fit the slate to the screensize
        //calculate the scale to fit the whole screen
        if (parentObject.transform.childCount == 0)
        {
            //if we dont have any children in parent then we dont have anything to allign
            return;
        }
        GameObject pixelObject = parentObject.transform.GetChild(0).gameObject;
        BoxCollider2D pixelColl = pixelObject.GetComponent<BoxCollider2D>();
        //find the middle point of parentObject
        //move it on the x in positive and on y negative
        parentObject.transform.rotation = Quaternion.Euler(0,0,-90);
        float coordinateX = numberX * 0.5f * pixelColl.size.x;
        float coordinateY = numberY * 0.5f * pixelColl.size.x;
        //calculate scale to fit the screen
       
        Vector2 screenSize = CalculateSceneSize();
        float scaleX = screenSize.x / (coordinateX * 2);
        float scaleY = screenSize.y / (coordinateY * 2);
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
        //make it smaller so that the ui fits
        scale *= 0.9f;
        //we want to move only the parent so we move it and then we move every children back so that the parent is in the middle
        //move the parent to the middle of the gameobject
        Vector3 offset = new Vector3(coordinateX, coordinateY, 0);
        parentObject.transform.position += offset;
        //now we can allign the parent to the middle
        foreach (var pix in gameLevelPixels)
        {
            pix.pixelObject.transform.position -= offset;
        }
        //parentObject.transform.parent = middleParent.transform;
        parentObject.transform.position = Vector3.zero;
        //scale the level to fit the screen
        parentObject.transform.localScale = Vector3.one * scale;
   }
   Vector2 CalculateSceneSize()
   {
       //some code is from this website
       https://www.loekvandenouweland.com/content/stretch-unity-sprite-to-fill-the-screen.html
       //with FOV and the distance of camera from the level we can calculate the width that the camera sees
       Camera mainCam = Camera.main;
       //get the world coordinates of the top rigth corner
       //bottom left corner is -toprigthcorner
       var topRightCorner = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCam.transform.position.z));
       var worldSpaceWidth = topRightCorner.x * 2;
       var worldSpaceHeight = topRightCorner.y * 2;
       return new Vector2(worldSpaceWidth, worldSpaceHeight);
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
   class Pixel
    {
        //we will use 80:Y pixels
        public int Size;
        public int startX;
        public int startY;
        public Color finalColor;
        public GameObject pixelObject;
        public Pixel(int x,int y,int width,Color color, GameObject parent)
        {
            //constructor
            Size = width;
            startX = x;
            startY = y;
            finalColor = color;
            //create gameobject for scene
            pixelObject = new GameObject();
            pixelObject.name =  startX +":"+ startY;
            pixelObject.tag = "pixel";
            //create texture and add it to a new sprite
            Texture2D pixelTexture = new Texture2D(Size, Size);
            //create e new color array we will supply to the texture
            int arraySize = Size * Size;
            Color[] colors = new Color[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                colors[i] = finalColor;
            }
            pixelTexture.SetPixels(colors);
            pixelTexture.Apply();
            Sprite sprite = Sprite.Create(pixelTexture,new Rect(0, 0, Size, Size), new Vector2());
            //add sprite to the gameobject
            SpriteRenderer renderer =  pixelObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            BoxCollider2D coll = pixelObject.AddComponent<BoxCollider2D>();
            //100 pixels is 1 unity unit
            float sizeMultiplier = Size * 0.01f;
            pixelObject.transform.position = new Vector3(startY * -sizeMultiplier, startX * sizeMultiplier, 0);
            pixelObject.transform.parent = parent.transform;
        }
        public void ChangeColor(Color update)
        {
            
            int arraySize = Size * Size;
            Color[] colors = new Color[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                colors[i] = update;
            }
            //without .Apply it doesnt work
            Texture2D texture = pixelObject.GetComponent<SpriteRenderer>().sprite.texture;
            texture.SetPixels(colors);
            texture.Apply();
        }
        public void ReloadColor()
        {
            //set the color to finalColor
            //we can use this function after calling an raycast and highlighting this pixel
            ChangeColor(finalColor);
        }
        public void Reactivate()
        {
            pixelObject.SetActive(true);
        }

        public void RemoveWhite()
        {
            if (finalColor == Color.white)
            {
                pixelObject.SetActive(false);
            }
        }
    }
}


