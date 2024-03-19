using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class CreatePrefab : MonoBehaviourPunCallbacks, IOnEventCallback
{
    //serializable variables that are assigned in unity
    public GameObject[] defaults;
    public string[] defaultsNames;
    //this dictionary is created from the two previous arrays
    public Dictionary<string,GameObject> assetData = new Dictionary<string, GameObject>();
    //array of all spawn locations calculated at the start of the game
    private Vector3[] spawns;
    //reference too the gameobject that has the controller script
    public GameObject controller;
    //reference to the object that detects when we are launched outside the map
    public GameObject scenebounds;
    //reference to the prefab that is assigned to the weapons
    public GameObject lifeBar;
    //reference to the prefab that is assigned to the player
    public GameObject playerName;
    //Photon normally uses the Resource folder to spawn the weapons, but we are making the objects in runtime, so we add them to the defaultpool from where photon can spawn it
    DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
    //we will calculate the number of created assets, there is a waituntil that checks that numberOfCreated == the number we want to create
    private int numberOfCreated = 0;
    //reference to the loading screen that is active until we have created all assets
    public GameObject loadingScreen;
    //the size of the player, we have to remember it bcs we will be resizing the other players when theirs texture is changed
    private Vector2 characterSize;
  
    void Start()
    {
        //we want to reset the dictionaries, photon remembers them when we load the scene again
        numberOfCreated = 0;
        pool.ResourceCache.Clear();
        assetData.Clear();
        
        for (int i = 0; i < defaults.Length; i++)
        {
            assetData.Add(defaultsNames[i],defaults[i]);
        }
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //if we are the master client, that means we have drawn the assets and we will send the info to the other players
            GameObject sceneAssets = GameObject.Find("AssetHolder");
            AssetHolder holder = sceneAssets.GetComponent<AssetHolder>();
            foreach (var AssetName in defaultsNames)
            {
                Create(holder.assets[AssetName],AssetName);
            }
            Destroy(sceneAssets);
        }
        //wait until we have created all assets, then start the game
        StartCoroutine(waitForAsets());
    }
    public void renamePlayer(string Name, int id, string nickname)
    {
        //rename player renames the player on all clients to the users Id
        photonView.RPC("namePlayer",RpcTarget.AllBuffered,id,Name,nickname);
    }

    public void changePlayer(GameObject player)
    {
        //change the texture of my player on all clients
        GameObject sceneAssets = GameObject.Find("AssetHolder");
        AssetHolder holder = sceneAssets.GetComponent<AssetHolder>();

        int[,] colors = holder.assets["Character"];
        List<int> color1D = new List<int>();
        //bcs photon RPC cannot send 2D array, we have to convert it to 1D and on the clients remake it to 2D with the width
        int arrayWidth = colors.GetLength(0);
        int yHeight = colors.GetLength(1);
        for (int y = 0; y < yHeight; y++)
        {
            for (int x = 0; x < arrayWidth; x++)
            {
                color1D.Add(colors[x,y]);
            }
        }

        int photonID = player.GetPhotonView().ViewID;
        photonView.RPC("changePlayerTexture",RpcTarget.AllBuffered,photonID,color1D.ToArray(),arrayWidth);
        Destroy(sceneAssets);
    }
    IEnumerator waitForAsets()
    {
        print("start wait");
        yield return new WaitUntil(() => numberOfCreated == defaults.Length);
        print("end wait");
        //when we activate the controller it handles the rest of the work 
        controller.SetActive(true);
        scenebounds.SetActive(true);
        loadingScreen.SetActive(false);
    }
    public void Create(int [,] colors, string name)
    {
        List<int> color1D = new List<int>();
        int arrayWidth = colors.GetLength(0);
        for (int y = 0; y < colors.GetLength(1); y++)
        {
            for (int x = 0; x < arrayWidth; x++)
            {
                color1D.Add(colors[x,y]);
            }
        }
        object[] content = new object[] {color1D.ToArray(),arrayWidth,name};
        //if we set the caching option to roomcacheglobal it will remain in the room even if the master client leaves
        //raise Event is an alternative to RPC, why I use it here is bcs it will stay in the room even if the master client leaves, so that other players are able to create the assets as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All,CachingOption = EventCaching.AddToRoomCacheGlobal};
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
    }
    public void OnEvent(EventData photonEvent)
    {
        //the raise events are resulted here
        byte eventCode = photonEvent.Code;
        if (eventCode == 1)
        {
            object[] data = (object[])photonEvent.CustomData;
            //deserialize the info about the assets
            int[] intArray = (int[])data[0];
            int width = (int)data[1];
            string assetName = (string)data[2];
            //create a copy of the asset on the clients computer
            copyComponents(intArray,width,assetName);
        }
    }
    [PunRPC]public void copyComponents(int[] colors1D,int width, string nameRPC)
    {
        if (numberOfCreated == assetData.Count)
        {
            //we have already created all assets
            return;
        }
        //decode the 1D array back to 2D
        int height = colors1D.Length / width;
        int[,] colorsRPC = new int[width, height];
        for (int i = 0; i < colors1D.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            colorsRPC[x, y] = colors1D[i];
        }
        //get the prefab of this asset that already has all the scripts on it
        GameObject newAsset = assetData[nameRPC];
        newAsset = Instantiate(newAsset);
        if (newAsset.CompareTag("Player"))
        {
            newAsset.GetComponent<PlayerSync>().enabled = true;
        }
        AssetInfo info = newAsset.GetComponent<AssetInfo>();
        newAsset.name = nameRPC;
        //create and assign the texture to this asset, it will also create a new polygon collider
        CombineSpriteArray(newAsset, colorsRPC);
        //resize the asset to the size of the original set size
        Vector2 defSize = new Vector2(info.width, info.height);
        ResizeAssets(newAsset,defSize);
        if (newAsset.CompareTag("Player"))
        {
            //assign the nickname holder to the player
            characterSize = defSize;
            GameObject nameHolder = Instantiate(playerName);
            nameHolder.transform.position = newAsset.transform.position + new Vector3(0, 1.4f, 0);
            nameHolder.transform.rotation = Quaternion.Euler(0,0,0);
            nameHolder.transform.parent = newAsset.transform;
            //set the nickname
            nameHolder.GetComponent<TMP_Text>().text = PhotonNetwork.LocalPlayer.NickName;
        }
        if (newAsset.CompareTag("weapon") && nameRPC != "Projectile")
        {
            //spawn the life bar and move it to the weapon
            GameObject bar = Instantiate(lifeBar);
            bar.transform.position = newAsset.transform.position + new Vector3(0, 1.2f, 0);
            bar.transform.parent = newAsset.transform;
            bar.GetComponent<DecreaseWeaponLife>().weapon = newAsset;
        }
        //check if the object has the ability to create a trail
        if (newAsset.TryGetComponent<CreateTrail>(out var component))
        {
            //create a texture for the trail
            component.createTexture();
        }
        //set the hasSprite to false, so that when a new instance is created, it will not calculate the sprite size
        info.hasSprite = false;
        //finally add the asset to the default pool
        pool.ResourceCache.Add(nameRPC, newAsset);
        newAsset.SetActive(false);
        numberOfCreated++;
    }
    public void CombineSpriteArray(GameObject attach, int[,] colorsRPC)
    {
        //find the first and last black pixel
            int length = colorsRPC.GetLength(0);
            int height = colorsRPC.GetLength(1);
            int firstX = length;
            int lastX = 0;
            int firstY = height;
            int lastY = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (colorsRPC[x,y] == 1)
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
           Vector2 firstBlackPixelCur = Vector2.zero;
            bool foundCorner = false;
            int spritesWidth = 100;
            int spritesHeight = spritesWidth;
           
            Vector2 texturePixelSize = new Vector2(lastX - firstX, lastY - firstY);
            Texture2D combinedTexture = new Texture2D(spritesWidth * (int)texturePixelSize.x,
                spritesHeight * (int)texturePixelSize.y);

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
                        if (!foundCorner)
                        {
                            //find the first corner pixel in the array
                            foundCorner = true;
                            firstBlackPixelCur = new Vector2((x-firstX) * spritesWidth * 0.01f, (y-firstY) * spritesHeight * 0.01f);
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
            //destroy any colliders the object might have, we will create new ones
            Collider2D[] colls = attach.GetComponents<Collider2D>();
            if (colls != null)
            {
                foreach (var coll in colls)
                {
                    Destroy(coll);
                }
            }
            //this is the width of one pixel in pixels(not unity units)
            float pixWidth = combinedTexture.width / texturePixelSize.x;
           //go from the middle of the sprite to the bottom corner and from there navigate with firstblackPixel to the first corner of the real sprite, that is the starting position
           //find the offset of the texture and the coordinates of the first black pixel, so that we can allign the collider
           firstBlackPixelCur = (Vector2)final.bounds.center -
               new Vector2(combinedTexture.width / 2, combinedTexture.height / 2) * 0.01f + firstBlackPixelCur;
         
           createPollygonCollider(pixWidth,firstBlackPixelCur,attach, colorsRPC);
    }
    public Vector2[]getColliderPoints(ref int[,] array)
    {
        List<Vector2> corners = new List<Vector2>();
        //we want to get edges of colliders to create one polygon collider
        Vector2 startPixelIndex = findStartingIndex(array);
        if (startPixelIndex == Vector2.positiveInfinity)
        {
            return null;
        }
        //this will return the first black pixel from the bottom left corner
        //so we can start on the bottom left corner of this pixel
       // pixel startPixel = pixelClass[(int)startPixelIndex.x,(int)startPixelIndex.y];
        Vector2 startCorner = startPixelIndex + new Vector2(-0.5f, -0.5f);
        corners.Add(startCorner);
        //we can go up, right, left, down
        //check if we have pixels on the sides of this corner
        Vector2 curPixel = startPixelIndex;
        Vector2 curDirection = Vector2.right;
        Vector2 curCorner = startCorner;
        //save the legacy direction so that we can determine when it changes and save that corner
        Vector2 legacyDirection = curDirection;
        //we have to save the legacy corner because when we change direction we are already on the new corner
        Vector2 legacyCorner = curCorner;
        Vector2 legacyPixel = startPixelIndex;
        while (true)
        {
            legacyCorner = curCorner;
            legacyPixel = curPixel;
            Tuple<Vector2, Vector2, Vector2> coordinates = findNextCorner(curPixel, curDirection, curCorner, array);
            curCorner = coordinates.Item1;
            curPixel = coordinates.Item2;
            curDirection = coordinates.Item3;
            //do this until we find an existing corner in the array, the starter corner will always be at the change of a direction
            if (curCorner == startCorner)
            {
                //we are back at the start
                //we have completed the circle and the collider is complete
                break;
            }
            else
            {
                //add it and continue
                if (legacyDirection != curDirection)
                {
                     corners.Add(legacyCorner);
                    
                     legacyDirection = curDirection;
                }
            }
        }
        //delete the pixels we have already completed
        //ref is a reference to the array so that it actually removes the ints from it
        deleteCompletedPixels(startPixelIndex, ref array);
        return corners.ToArray();
    }
    //delete completed pixels asks for a starting pixel and deletes every pixel that is in touch with the starting one or any other in this chain
    //it does not delete a corner touch pixel
    public void deleteCompletedPixels(Vector2 startIndex, ref int[,] array)
    {
        //this will delete every pixel it touches and chain it
        //we supply it with the original pixel and all pixels that have noncorner touch with it will be destroyed
        Vector2 curIndex = startIndex;
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        int curPixel =  array[(int)curIndex.x, (int)curIndex.y];
        if (curPixel == 1)
        {
            array[(int)curIndex.x, (int)curIndex.y] = 0;
        }
        else
        {
            //we have already been here, or the pixel is white
            return;
        }
        Vector2[] paths = new Vector2[4];
        paths[0] = curIndex + Vector2.right;
        paths[1] = curIndex + Vector2.left;
        paths[2] = curIndex + Vector2.up;
        paths[3] = curIndex + Vector2.down;
        //start new instance of this func with every path and delete current pixel
        for (int i = 0; i < 4; i++)
        {
            if (isInBounds(paths[i], width, height))
            {
                //recursevely call this function
                deleteCompletedPixels(paths[i], ref array);
            }
        }
    }
    //is in bounds checks if the supplied indexes are inside the bounds of the wanted array width and height
    bool isInBounds(Vector2 index, int width, int height)
    {
        //check if we are within the boundries of an array
        bool returnValue = false;
        if (index.x >= 0 && index.x < width && index.y >= 0 && index.y < height)
        {
            returnValue = true;
        }
        return returnValue;
    }
    //find next corner works with direction, pixel index and corner index to determine the next index of the corner and the direction we want to continue
    public Tuple<Vector2,Vector2,Vector2> findNextCorner(Vector2 pixelIndex, Vector2 direction, Vector2 cornerPosition, int[,] array)
    {
        //first vector is the corner, second one is the pixel, third one is the direction
        Tuple<Vector2, Vector2,Vector2> returnValue = new Tuple<Vector2, Vector2,Vector2>(Vector2.zero,Vector2.zero,Vector2.zero);
        List<Vector2> touchingPixels = new List<Vector2>();
        Vector2[] touchingPixelsIndexes = new Vector2[4];
       
        //find the touching pixels with the corner offset multiplied by 2
        touchingPixelsIndexes[0] = cornerPosition + new Vector2(0.5f, 0.5f);
        touchingPixelsIndexes[1] = cornerPosition + new Vector2(-0.5f, 0.5f);
        touchingPixelsIndexes[2] = cornerPosition + new Vector2(0.5f, -0.5f);
        touchingPixelsIndexes[3] = cornerPosition + new Vector2(-0.5f, -0.5f);
     
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        //check how many of the surrounding pixels are black
        for (int i = 0; i < 4; i++)
        {
            //check if we are inside the bounds of the pixelArray
            if (isInBounds(touchingPixelsIndexes[i],width,height))
            {
                //we are inside the bounds
                int possiblePixel = array[(int)touchingPixelsIndexes[i].x, (int)touchingPixelsIndexes[i].y];
                if (possiblePixel == 1)
                {
                    //add this to the touching pixels, save the index as well
                    touchingPixels.Add(touchingPixelsIndexes[i]);
                }
            }
        }
        //we have four options
        //1) we are touching one pixel so we go straight
        //2) touching two pixels, we go to the one that is oposite to ours
        //3) we are touching none, go to the next corner of the same pixel
        //4) we are touching only the pixel oposite to ours, so the touch is only in the corner, ignore this and act like we are touching none
        
        //1 && 4)
        if (touchingPixels.Count == 2)
        {
            if (touchingPixels.Contains(pixelIndex + direction))
            {
                //we have checked that this is the following pixel in line
                returnValue =
                    new Tuple<Vector2, Vector2, Vector2>(cornerPosition + direction, pixelIndex + direction, direction);
            }
            else
            {
                //it is a corner touch
                //clear the array and add the current pixel, the code bellow for count==1 would trigger
                touchingPixels.Clear();
                touchingPixels.Add(pixelIndex);
            }
        }
        if (touchingPixels.Count == 3)
        {
            Vector2[] normals = new Vector2[2];
            //create normals on the direction
            normals[0] = new Vector2(-direction.y, direction.x);
            normals[1] = new Vector2(direction.y, -direction.x);
            for (int i = 0; i < 3; i++)
            {
               //check which one of the pixels is the middle one, it will have a pixel in the direction of the normal and the -direction as well
                if (touchingPixels.Contains(touchingPixels[i] + normals[0]) && touchingPixels[i] -direction == pixelIndex)
                {
                    //we have found the right way to go
                    returnValue = new Tuple<Vector2, Vector2, Vector2>(cornerPosition + normals[0], touchingPixels[i] + normals[0],normals[0]);
                    break;
                }
                if (touchingPixels.Contains(touchingPixels[i] + normals[1]) && touchingPixels[i] -direction == pixelIndex)
                {
                    //we have found the right way to go
                    returnValue = new Tuple<Vector2, Vector2, Vector2>(cornerPosition + normals[1], touchingPixels[i] + normals[1],normals[1]);
                    break;
                }
            }
        }
        //we are not touching any pixels, so choose a different corner from the same pixel
        if (touchingPixels.Count == 1)
        {
            //move to the next corner, we will figure it out based on the drection and current offset
            Vector2 cornerOffset = cornerPosition - pixelIndex;
            //we want to flip the corner around the direction vector
            if (direction.x != 0)
            {
                //we want to flip the Y coordinate of the corner!!!!
                cornerOffset *= new Vector2(1, -1);
            }
            if (direction.y != 0)
            {
                //flip the X of the corner!!!
                cornerOffset *= new Vector2( -1,1);
            }
            //now we have new offset by which we can determine the position of the new corner
            Vector2 dir = (pixelIndex + cornerOffset) - cornerPosition;
            returnValue = new Tuple<Vector2, Vector2, Vector2>(pixelIndex + cornerOffset, pixelIndex, dir);
        }
        return returnValue;
    }
    //find starting findex goes throw the array from the bottom left corner and checks for the first black pixel, if it founds it then it returns it
    public Vector2 findStartingIndex(int[,] array)
    {
        //find the first black pixel from bottom left corner
        Vector2 coordinates = Vector2.positiveInfinity;
        bool breakOut = false;
        int height = array.GetLength(1);
        int width = array.GetLength(0);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int possiblePixel = array[x, y];
                if (possiblePixel == 1)
                {
                    coordinates = new Vector2(x, y);
                    breakOut = true;
                    break;
                }
            }
            //keep the variable so we can exit the second loop
            if (breakOut)
            {
                break;
            }
        }
        return coordinates;
    }
    public void createPollygonCollider(float pixWidth,Vector2 spriteCorner, GameObject DefaultGameObject, int[,] array)
    {
        PolygonCollider2D coll = DefaultGameObject.AddComponent<PolygonCollider2D>();
        //index keeps track of the current path of the polygon collider
        int index = 0;
        Vector2 startCorner = Vector2.zero;
        bool foundStartCorner = false;
        List<Vector2> collPoints = new List<Vector2>();
        //conversion from pixels to units is that we divide it by 100
        float factor = pixWidth * 0.01f;
        while (ArePixelsLeft(array))
        {
            collPoints.AddRange(getColliderPoints(ref array));
            coll.pathCount = index + 1;
            for (int i = 0; i < collPoints.Count; i++)
            {
                //we have to resize the vectors to the real size of the sprite, bcs it odes not match with unity units
                collPoints[i] *= factor;
            }
            //start corner is the most bottom left corner of the collider
            if (!foundStartCorner)
            {
                //once we have found it we dont want to go here anymore
                startCorner = collPoints.First();
                foundStartCorner = true;
            }
            coll.SetPath(index, collPoints.ToArray());
            //jump to the next path
            index++;
            //reset the array
            collPoints.Clear();
        }
        //compute the vector to the corner of the sprite, so we line up the collider
        Vector2 collOffset = spriteCorner - startCorner;//spriteRenderer.bounds.center - coll.bounds.center;
        coll.offset = collOffset;
        if (!DefaultGameObject.CompareTag("Player") && !DefaultGameObject.CompareTag("ground"))
        {
             coll.isTrigger = true;
        }
    }
    //are pixels left checks if the are some black pixels left, so we can determine if to create another part of the collider
    bool ArePixelsLeft(int[,] array)
    {
        //check if we have deleted all pixels or some are left
        foreach (var pix in array)
        {
            if (pix != 0)
            {
                return true;
            }
        }
        return false;
    }
    public void ResizeAssets(GameObject resizeThis, Vector2 defaultSize)
    {
        //get the size of the asset that was created and is the bad size
        Vector3 createdSize = resizeThis.GetComponent<SpriteRenderer>().sprite.bounds.size;
        //get x and y scale
        Vector2 scale = new Vector2(defaultSize.x / createdSize.x, defaultSize.y / createdSize.y);

        //get which scale is bigger, we want to use the smaller scale
        if (scale.x < scale.y)
        {
            //we want to multiply both sides with one scale to have the same aspect ratio
            resizeThis.transform.localScale *= scale.x;
        }
        else
        {
            //y scale is smaller
            resizeThis.transform.localScale *= scale.y;
        }
    }
    [PunRPC] public void namePlayer(int gameobjectID, string nameID,string nickname)
    {
       //this rpc is called on all clients to rename the supplied player to supplied name
        GameObject player = PhotonView.Find(gameobjectID).gameObject;
        player.name = nameID;
        //also rename the nickname on the holder
        GameObject nameHolder = player.transform.GetChild(0).gameObject;
        nameHolder.GetComponent<TMP_Text>().text = nickname;
    }

    [PunRPC] public void changePlayerTexture(int gameobjectID, int[] colors1D, int width)
    {
        //we want to change the texture to the one the player has drawn
        GameObject player = PhotonView.Find(gameobjectID).gameObject;
        //we have to set the size to the default before resizing it
        player.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        //convert to 2d array
        int length = colors1D.Length;
        int height =  length/ width;
        int[,] colorsRPC = new int[width, height];
        for (int i = 0; i < length; i++)
        {
            int x = i % width;
            int y = i / width;
            colorsRPC[x, y] = colors1D[i];
        }
        //create new texture and new collider, the old collider will be deleted
        CombineSpriteArray(player, colorsRPC);
        ResizeAssets(player,characterSize);
        //reset the nickname holder
        GameObject nameHolder = player.transform.GetChild(0).gameObject;
        nameHolder.transform.localScale = new Vector3(1, 1, 1);
        nameHolder.transform.localPosition = new Vector3(0, 1.4f, 0);
        nameHolder.transform.rotation = Quaternion.Euler(0,0,0);
        //we have to change the trail texture, bcs otherwise it would use the master clients texture
        player.GetComponent<CreateTrail>().createTexture();
    }
}
