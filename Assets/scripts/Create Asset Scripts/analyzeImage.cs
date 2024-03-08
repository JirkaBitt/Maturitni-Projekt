using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Color = UnityEngine.Color;
using GameObject = UnityEngine.GameObject;
using Screen = UnityEngine.Screen;



public class analyzeImage : MonoBehaviour
{
// the number of pixels on X axis, Y axis will be calculated based on screen height
    public int numberOfPixelsX = 72;
//current assets pixels used for raycast and recoloring
    private pixel[,] gameLevelPixels;
//current tool changed by buttons from changeTools script
    public String currentTool = "Pencil";
//sceneAssets is the parent that holds all assets
    private GameObject sceneAssets;
//finder texture is the original fullsize image from user photo library
    private Texture2D finderTexture;
//reloadThese is a list of highlighted pixels that should be reset in the next frame
    private List<pixel> reloadThese = new List<pixel>();
//this is a holder for the previous position of our mouse when we make the assets, it is used in receiveskipped
    private Vector2 previousRaycastPos = Vector2.zero;
//assetNames is an array of assets we want to create
    public string[] assetNames = new[] { "Arena","Character","Sword","Axe","Spear","Gun","Projectile","Bomb" };
//assetIndex is an int that points to the current asset
    public int assetIndex = 0;
//the pointer to the current asssets parent
    private GameObject currentParent;
//list of created assets
    private List<prefab> assetPrefabs = new List<prefab>();
    public string selectText = "";
//we want to stop the raycast if we display the assets at the end
    private bool displayed = false;
//frame prefab
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
    private changeTools toolsScript;
//photon lobby is used for joining and creating rooms
    public GameObject photonLobby;
//the radius of pencil or eraser, it is changed from changeTools script
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
    
//the prefab class houses the necessary information for each asset and manipulates(maximize, minimalize, refresh) 
    class prefab
    {
        public string name;
        //this is the object that is shown and that we will manipulate
        public GameObject objectPrefab; 
        //pixel class is a reference to the slate that we draw on, we get the info about the pixels from there
        public pixel[,] pixelClass;
        public bool isDefault;

        public Vector3 fullScreenPosition;
        public Vector3 fullScreenScale;
        
        public Vector3 smallScreenPosition;
        public Vector3 smallScreenScale;

        public GameObject DefaultGameObject;
        private Vector3 defaultSize;

        private float pixWidth = 0;
        private Vector2 firstBlackPixel = Vector2.zero;

        private GameObject slate;
        private int[,] colors;
        
        public prefab(GameObject slatee,GameObject objectt, pixel[,] array, string n,bool defaultAsset,GameObject defObj, Vector3 defSize)
        {

            this.objectPrefab = objectt;
            this.pixelClass = array;//array.Clone() as pixel[,];
            this.name = n;
            this.isDefault = defaultAsset;
            this.DefaultGameObject = defObj;
            defaultSize = defSize;
            this.slate = slatee;
            //we have to instantiate it to get the size
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

        public void fitTheScreen()
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
                    pixelClass[x,y].reloadColor();
                }
            }
        }
        public void refreshAsset()
        {
            //remake the texture and update the int array
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

        public void addSavedAsset(int[,] savedInts)
        {
            //we have loaded assets from saves, we want to display them right away
            colors = savedInts;
            CombineSpriteArray(objectPrefab, savedInts);
        }
        public void minimalize()
        {
            // show the asset in the overview
            objectPrefab.SetActive(true);
            objectPrefab.transform.position = smallScreenPosition;
            objectPrefab.transform.localScale = smallScreenScale;
            
        }
        //update values takes the current transform values and saves it
        public void updateSmallValues(GameObject update)
        {
            smallScreenPosition = update.transform.position;
            smallScreenScale = update.transform.lossyScale;
        }
        public void computeMinimalize(float screenWidth,int assetCount, int index)
        {
            //compute where this asset should be placed in the overview
            float assetWidth = screenWidth / assetCount;
            smallScreenPosition = new Vector3(assetWidth * index - screenWidth/2 + assetWidth/2, 0, 0);
            smallScreenScale = fullScreenScale / assetCount;
            
            minimalize();
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
                pixWidth = combinedTexture.width / texturePixelSize.x;
               //go from the middle of the sprite to the bottom corner and from there navigate with firstblackPixel to the first corner of the real sprite, that is the starting position
              
               firstBlackPixel = (Vector2)final.bounds.center -
                   new Vector2(combinedTexture.width / 2, combinedTexture.height / 2) * 0.01f + firstBlackPixel;
            
        }
        //find next corner works with direction, pixel index and corner index to determine the next index of the corner and the direction we want to continue
        public void finalize(assetHolder holder)
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
        toolsScript = buttonUI.GetComponent<changeTools>();
       
        if (loadLevelsFromSaves)
        {
            loadSavedAssets();
        }
        else
        {
            createSlate();
        }
    }
    void Update()
    {
        //firstly reload pixels and then highlight the selected one
        //we will reload only the pixels we marked as red
        reloadSpecificPixels();
        //refresh frames
        recolorFrames();
        //highligth pixels or frames
        castRaycast();
    }
    

    public void startGame()
    {
        //create room and return the room name
        Destroy(slate);
        GameObject assetHolderObj = new GameObject();
        assetHolderObj.name = "AssetHolder";
        assetHolder holder = assetHolderObj.AddComponent<assetHolder>();
        foreach (var pref in assetPrefabs)
        {
            pref.finalize(holder);
        }
        //this ensures that the assets stay there when we start the gam
        DontDestroyOnLoad(assetHolderObj);
        if (onlyCharacter)
        {
            loadingScreen.SetActive(true);
            toolsScript.startGameButton.SetActive(false);
            goBackButton.SetActive(false);
            createRoom();
        }
        else
        {
            //we should show the popup and wait for user to decide if they want to save the assets
            if (loadLevelsFromSaves && !hasEditedAssets)
            {
                //if we havent edited anything, there is no need to show the update popup
                loadingScreen.SetActive(true);
                toolsScript.startGameButton.SetActive(false);
                goBackButton.SetActive(false);
                createRoom();
                return;
            }
            savePopup.SetActive(true);
            //disable the UI
            goBackButton.SetActive(false);
            toolsScript.startGameButton.SetActive(false);
            foreach (var display in frames)
            {
                display.SetActive(false);
            }
        }
    }

    public void createRoom()
    {
        Destroy(sceneAssets);
        PUN2_GameLobby lobby = photonLobby.GetComponent<PUN2_GameLobby>();
        //this is the player name
        string pName = inputName.GetComponent<TMP_InputField>().text;
        if (pName != "")
        {
            lobby.playerName = pName;
        }
        else
        {
            lobby.playerName = "Player";
        }
        //this function creates a room and returns its name
        lobby.createRoomWithoutUI();
    }
    public void loadSavedAssets()
    {
        GameObject holder = GameObject.Find("savedAssets");
        fetchCreatedLevels dataHolder = holder.GetComponent<fetchCreatedLevels>();

        fetchCreatedLevels.savedAssets selectedLevel = dataHolder.selectedAssets;
        createSlateForSaved(selectedLevel.assets[0].GetLength(1));
        gameLevelPixels[0,0].finalColor = Color.black;
        for (int i = 0; i < 8; i++)
        {
            //create new class that holds the object and the pixels so we can delete the  white ones at the end
            //acces the default object size
            assetInfo info = defaultAssets[i].GetComponent<assetInfo>();
            info.calculateSize();
            Vector3 defSize = new Vector3(info.width, info.height, 1);
            //Destroy(temp);
            GameObject parentAsset = new GameObject();
            parentAsset.transform.parent = sceneAssets.transform;
            prefab asset = new prefab( slate,parentAsset, gameLevelPixels, assetNames[i],false,defaultAssets[i],defSize);
            asset.fullScreenPosition = currentParent.transform.position;
            asset.fullScreenScale = currentParent.transform.localScale;
            asset.addSavedAsset(selectedLevel.assets[i]);
            assetPrefabs.Add(asset);
        }
        finalizeAssets();
    }
    public void joinGame()
    {
        //we have to check if the player has drawn something
        if (checkIfBlank())
        {
            return;
        }
        Destroy(slate);
        GameObject assetHolderObj = new GameObject();
        assetHolderObj.name = "AssetHolder";
        assetHolder holder = assetHolderObj.AddComponent<assetHolder>();
        foreach (var pref in assetPrefabs)
        {
            //remove white pixels
            pref.finalize(holder);
            
        }
        //this ensures that the assets stay there when we start the game
        DontDestroyOnLoad(assetHolderObj);
        Destroy(sceneAssets);
        PUN2_GameLobby lobby = photonLobby.GetComponent<PUN2_GameLobby>();
        string pName = inputName.GetComponent<TMP_InputField>().text;
        if (pName != "")
        {
             lobby.playerName = pName;
        }
        else
        {
            lobby.playerName = "Player";
        }
        //this function creates a room and returns its name
        GameObject roomName = GameObject.FindWithTag("roomName");
        
        bool result = lobby.joinRoomWithName(roomName.name);

        if (!result)
        {
           // SceneManager.LoadScene("ChooseLevel");
           print("failed to join!!!!!!!!");
        }
       
    }

    public void createSlateForSaved(int height)
    {
        //this function is different from normal create slate bcs we coukd have had different height, and we want to match the original height
        GameObject parent = new GameObject();
        parent.name = "Slate";
        int pixelWidth = (int)(Screen.width * (1 - 0.2f)) / numberOfPixelsX;
        //big pixels in y
        int pixelsY = height;

        gameLevelPixels = new pixel[numberOfPixelsX, pixelsY];
        for (int y = 0; y < pixelsY; y++)
        {
            for (int x = 0; x < numberOfPixelsX; x++)
            {
                gameLevelPixels[x,y] = new pixel(x, y, pixelWidth, Color.white, parent);
            }
        }
        //make it fit the screen
        fitTheScreen(parent, numberOfPixelsX, pixelsY);
        //add it to the scene parent
        parent.transform.parent = sceneAssets.transform;
        currentParent = parent;
        slate = parent;
    }
    //create slate is called at the start and creates a canvas of pixels to draw to
    public void createSlate()
    {
        GameObject parent = new GameObject();
        parent.name = "Slate";
        int pixelWidth = (int)(Screen.width * (1 - 0.2f)) / numberOfPixelsX;
        //big pixels in y
        int pixelsY = (int)(Screen.height * (1 - 0.2f)) / pixelWidth;

        gameLevelPixels = new pixel[numberOfPixelsX, pixelsY];
        for (int y = 0; y < pixelsY; y++)
        {
            for (int x = 0; x < numberOfPixelsX; x++)
            {
                gameLevelPixels[x,y] = new pixel(x, y, pixelWidth, Color.white, parent);
            }
        }
        //make it fit the screen
        fitTheScreen(parent, numberOfPixelsX, pixelsY);
        //add it to the scene parent
        parent.transform.parent = sceneAssets.transform;
        currentParent = parent;
        slate = parent;
        
    }

    public bool checkIfBlank()
    {
        //check if the player has drawn something
        foreach (var pixel in gameLevelPixels)
        {
            if (pixel.finalColor == Color.black)
            {
                return false;
            }
        }

        StartCoroutine(showWarning());
        return true;
    }
    IEnumerator showWarning()
    {
        warning.SetActive(true);
        yield return new WaitForSeconds(1f);
        warning.SetActive(false);
    }
    //refresh slate makes every pixel in the canvas white again
    public void refreshSlate(string assetName)
    {
        previousRaycastPos = Vector2.zero;
        foreach (var pix in gameLevelPixels)
        {
            pix.finalColor = Color.white;
            pix.reloadColor();
        }

        TextMeshProUGUI text = assetDisplayText.GetComponent<TextMeshProUGUI>();
        text.SetText("Please draw the " + assetName);
    }
    //remake allows the user to refresh the slate at the current asset and draw again
    public void remake()
    {
        refreshSlate(assetNames[assetIndex]);
    }
    public void remakeClicked()
    {
        int index = assetIndex;
        prefab pref = assetPrefabs[index];
        assetIndex = index;
        refreshSlate(pref.name);
    }
    //submit change is called when we click the submit button when editing an asset
    public void submitChange()
    {
        if (checkIfBlank())
        {
            return;
        }
        goBackButton.GetComponent<goBack>().cancelEditing = false;
        //update prefab
        hasEditedAssets = true;
        int index = assetIndex;
        prefab pref = assetPrefabs[index];
        pixel[,] pixArray = gameLevelPixels;
        
        pref.pixelClass = pixArray;
        pref.refreshAsset();
        minimalizeAsset(pref);
    }
    //recolor frames refreshes the frames from highlighted to white
    void recolorFrames()
    {
        //refresh highligthed frames
        foreach (var ren in reloadFrames)
        {
            ren.material.color = Color.white;
        }

        reloadFrames.Clear();
    }
    //finalize assets is called when we create all assets and we want to display them
    void finalizeAssets()
    {
        refreshSlate("");
        slate.SetActive(false);
        //now we have the final assets prepared, without white background and we can use them
        displayAssets();
    }
    //display assets creates frames for every asset and frames them
    void displayAssets()
    {
        //we want to change the buttons functions to adress the changes
       toolsScript.changeFunctions();
       toolsScript.deactivateButtons();
       toolsScript.enableStartGame();
       //dont show the text
       assetDisplayText.SetActive(false);
     
        //displayed set to true stops the raycast
        displayed = true;
        int assetCount = assetPrefabs.Count;
        frames = new GameObject[assetCount];
        Vector2 screenSize = calculateSceneSize();
        float screenWidth = screenSize.x;
        float assetWidth = screenWidth / assetCount;
        for (int i = 0; i < assetCount; i++)
        {
            //create a frame for the asset
            GameObject assetFrame = Instantiate(frame);
            //set the index as a name so we can receive it later 
            assetFrame.name = i.ToString();
            Vector2 frameSize = assetFrame.GetComponent<BoxCollider2D>().size;
            float scaleFactor =  assetWidth / frameSize.x;
            assetFrame.transform.localScale = new Vector3(scaleFactor , scaleFactor, 1);
            //acces the text display and set the text to the name of the asset
            GameObject textDisplay = assetFrame.GetComponent<frameScript>().textDisplay;
            TextMeshProUGUI meshPro = textDisplay.GetComponent<TextMeshProUGUI>();
            prefab pref = assetPrefabs[i];
            meshPro.SetText(pref.name);
            //change the scale to fit every asset, +1 so it is a little smaller
            pref.objectPrefab.transform.localScale /= assetCount;
            //move the assets, we need to have -screenwidth/2 because the middle is in 0,0,0 and we want to start from the left
            Vector3 pos = new Vector3(assetWidth * i - screenWidth/2 + assetWidth/2, 0, 0);
            pref.objectPrefab.transform.position = pos;
            //we have to offset the frame to move it back so it doesnt collide with the asset
            Vector3 frameOffset = new Vector3(0,0, 1);
            assetFrame.transform.position = pos + frameOffset;
            pref.objectPrefab.SetActive(true);
            //save information for later
            pref.smallScreenPosition = pos;
            pref.smallScreenScale = pref.objectPrefab.transform.localScale;
            pref.computeMinimalize(calculateSceneSize().x,assetPrefabs.Count,i);
            pref.minimalize();
            frames[i] = assetFrame;
        }
    }
    //move to the next is called when user clicks next and it saves the asset in a new instance of the class prefab and refreshes the slate
    public void moveToTheNext()
    {
        if (checkIfBlank())
        {
            return;
        }
        //create new class that holds the object and the pixels so we can delete the  white ones at the end
        //acces the default object size
        assetInfo info = defaultAssets[assetIndex].GetComponent<assetInfo>();
        info.calculateSize();
        Vector3 defSize = new Vector3(info.width, info.height, 1);
        //Destroy(temp);
        GameObject parentAsset = new GameObject();
        parentAsset.transform.parent = sceneAssets.transform;
        prefab asset = new prefab( slate,parentAsset, gameLevelPixels, assetNames[assetIndex],false,defaultAssets[assetIndex],defSize);
        asset.fullScreenPosition = currentParent.transform.position;
        asset.fullScreenScale = currentParent.transform.localScale;
        assetPrefabs.Add(asset);
        assetIndex++;
        if (assetIndex < assetNames.Length)
        {
            refreshSlate(assetNames[assetIndex]);
        }
        else
        {
            print("we have created all assets");
            //all assets have been created
            //now show the overview
            if (onlyCharacter)
            {
                loadingScreen.SetActive(true);
                toolsScript.startGameButton.SetActive(false);
                goBackButton.SetActive(false);
                joinGame();
            }
            else
            {
                finalizeAssets();
            }
            
        }
    }
    void castRaycast()
    {
        //cast raycast from camera to mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        //we have to use GetRayIntersection for 2DColliders
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
        
        if (hit2D.collider != null)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //we are over a button or ther GUI element
                return;
            }
            GameObject hitPixel = hit2D.collider.gameObject;
            if (hitPixel.CompareTag("pixel") && !displayed && addPixels)
            {
                List<RaycastHit2D> allHits = new List<RaycastHit2D>();
                //if we move too fast we skip the pixels, so we retrieve them using this function
                if (previousRaycastPos != Vector2.zero && previousRaycastPos != (Vector2)hitPixel.transform.position)
                {
                    //we are moving the cursor
                    allHits.AddRange(addSkippedPixels(previousRaycastPos, hitPixel.transform.position));
                }
                else
                {
                    //we have to add the pixels if we are stationary and not moving the cursor
                    allHits.AddRange(Physics2D.CircleCastAll(hitPixel.transform.position, Mathf.Abs(toolRadius / 2), Vector2.zero));
                }
                //save the previous pos
                previousRaycastPos = hitPixel.transform.position;
                //we have to add hit2d because if toolradius is 0 then we dont have any pixels, so add the default one
                allHits.Add(hit2D);
                
                foreach (var hitPix in allHits)
                {
                    String pixelName = hitPix.collider.gameObject.name;
                    //get coordinates from gameobjectname
                    String[] coordinates = pixelName.Split(":");
                    int x = Convert.ToInt32(coordinates[0]);
                    int y = Convert.ToInt32(coordinates[1]);
                    //find the class and highligth the pixel
                    pixel pixelClass = gameLevelPixels[x, y];
                    pixelClass.changeColor(Color.red);
                    //add these to a list so we can reload them
                    reloadThese.Add(pixelClass);
                    if (Input.GetMouseButton(0))
                    {
                        //we have selected this pixel, change it color
                        if (currentTool == "Eraser")
                        {
                            //change the color to white
                            pixelClass.finalColor = Color.white;
                            pixelClass.reloadColor();
                        }
    
                        if (currentTool == "Pencil")
                        {
                            //change the color to black
                            pixelClass.finalColor = Color.black;
                            pixelClass.reloadColor();
                        }
                    }
                }
               
            }

            //check if we displayed the assets and are aiming at them
            if (hitPixel.CompareTag("frame") && displayed)
            {
                SpriteRenderer ren =  hitPixel.GetComponent<SpriteRenderer>();
                ren.material.color = Color.yellow;
                //add these to the list of frames to refresh its color
                reloadFrames.Add(ren);

                if (Input.GetMouseButtonDown(0))
                {
                    //we have clicked this frame, load the asset for editing
                    int index = Convert.ToInt32(hitPixel.name);
                    prefab pref = assetPrefabs[index];
                    //get the size of pixels inside the class
                    maximazeAsset(pref);
                    assetIndex = index;
                    //we want to skip the first time
                    previousRaycastPos = Vector2.zero;
                    //dont start drwaning until the mouse is lifted
                    StartCoroutine(waitForMouseUp());
                }
            }
        }
    }

    IEnumerator waitForMouseUp()
    {
        //pause the recoloring of pixels until the mouse is lifted, so we dont make a spot when opening the asset
        addPixels = false;
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
        addPixels = true;
    }
    RaycastHit2D[] addSkippedPixels(Vector2 origin, Vector2 end)
    {
        //get the pixels in line from where we were and where we are now
        RaycastHit2D[] skipped = Physics2D.LinecastAll(origin, end);
        Vector2[] skippedPositions = new Vector2[skipped.Length];
        //retrieve pixels position
        for(int i = 0; i < skipped.Length; i++)
        {
            skippedPositions[i] = skipped[i].collider.gameObject.transform.position;
        }
        //cast the raycast around the pixels 
        List<RaycastHit2D> addThese = new List<RaycastHit2D>();
        
        addThese.AddRange(skipped);
        foreach (var hit in skippedPositions)
        {
            //RaycastHit2D[] allHits = Physics2D.CircleCastAll(hit, Mathf.Abs(toolRadius / 2), Vector2.zero);
            addThese.AddRange(Physics2D.CircleCastAll(hit, Mathf.Abs(toolRadius / 2), Vector2.zero));
            //do not check for duplicates, it gets laggy and much slower
           
        }

        return addThese.ToArray();
    }
    //reload specific pixels will reset the color of supplied list of pixels in the slate
    void reloadSpecificPixels()
    {
        foreach (var pix in reloadThese)
        {
            if (pix.isActive)
            {
                pix.reloadColor();
            }
        }
        reloadThese.Clear();
    }
    //maximaze is called when we click on an asset to remake it, it will display the slate with the asset
    void maximazeAsset(prefab pref)
    {
        goBackButton.GetComponent<goBack>().cancelEditing = true;
        //deactivate assets and frames
        toolsScript.disableStartGame();
        //pref.updateSmallValues(pref.objectPrefab);
        foreach (var preff in assetPrefabs)
        {
            preff.objectPrefab.SetActive(false);
        }
        
        foreach (var frame in frames)
        {
            frame.SetActive(false);
        }
        //activate the selected asset
       
            foreach (var pix in pref.pixelClass)
            {
                pix.reactivate();
            }
        
        toolsScript.activateButtons();
        pref.fitTheScreen();
        //we have to reasign the gameLevelpixels because they are used castraycast
        //gameLevelPixels = pref.pixelClass;
        //currentParent = pref.objectPrefab;
        displayed = false;
        
        assetDisplayText.SetActive(true);
        TextMeshProUGUI text = assetDisplayText.GetComponent<TextMeshProUGUI>();
        text.SetText("Please draw the " + pref.name);
    }
    void minimalizeAsset(prefab pref)
    {
        //update the position and scale
        toolsScript.enableStartGame();
       //pref.updateFullValues(pref.objectPrefab);
       pref.computeMinimalize(calculateSceneSize().x,assetPrefabs.Count,assetIndex);
        foreach (var preff in assetPrefabs)
        {
            preff.objectPrefab.SetActive(true);
        }
        foreach (var frame in frames)
        {
            frame.SetActive(true);
        }
        slate.SetActive(false);
        toolsScript.deactivateButtons();
        pref.minimalize();
        displayed = true;
        
        assetDisplayText.SetActive(false);
    }

    public void returnFromEditing()
    {
        minimalizeAsset(assetPrefabs[assetIndex]);
    }
    //tuple allows to return two or more variables
    void fitTheScreen(GameObject parentObject, float numberX, float numberY)
   {
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
       
        Vector2 screenSize = calculateSceneSize();
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
   Vector2 calculateSceneSize()
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
   class pixel
    {
        //we will use 80:Y pixels
        public int Size;
        public int startX;
        public int startY;
        public Color finalColor;

        public GameObject pixelObject;
        public bool isActive = true;
        public pixel(int x,int y,int width,Color color, GameObject parent)
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
           // pixelObject.tag = "ground";
            //create texture and add it to a new sprite
            Texture2D pixelTexture = new Texture2D(Size, Size);
            //create e new color array we will supply to the texture
            Color[] colors = new Color[Size * Size];
            for (int i = 0; i < Size*Size; i++)
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

            float sizeMultiplier = Size * 0.01f;
            pixelObject.transform.position = new Vector3(startY * -sizeMultiplier, startX * sizeMultiplier, 0);
            pixelObject.transform.parent = parent.transform;
        }
        public void changeColor(Color update)
        {
            if (isActive)
            {
                Color[] colors = new Color[Size * Size];
                for (int i = 0; i < Size * Size; i++)
                {
                    colors[i] = update;
                }

                //without .Apply it doesnt work
                Texture2D texture = pixelObject.GetComponent<SpriteRenderer>().sprite.texture;
                texture.SetPixels(colors);
                texture.Apply();
            }
        }
        public void reloadColor()
        {
            //set the color to finalColor
            //we can use this function after calling an raycast and highlighting this pixel
            Color[] colors = new Color[Size * Size];
            for (int i = 0; i < Size*Size; i++)
            {
                colors[i] = finalColor;
            }
            //without .Apply it doesnt work
            Texture2D texture =  pixelObject.GetComponent<SpriteRenderer>().sprite.texture;
            texture.SetPixels(colors);
            texture.Apply();
        }
        public void reactivate()
        {
            if (!isActive)
            {
                isActive = true;
                pixelObject.SetActive(true);
            }
        }
    }
}


