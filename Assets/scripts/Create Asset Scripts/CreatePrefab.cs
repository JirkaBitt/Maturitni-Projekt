using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class CreatePrefab : MonoBehaviourPunCallbacks
{
    public GameObject[] defaults;
    public string[] defaultsNames;
    public string[] PhotonNames;
    public Dictionary<string,GameObject> assetData = new Dictionary<string, GameObject>();
    private Vector3[] spawns;
    public GameObject controller;
   
    public GameObject scenebounds;
    public GameObject lifeBar;
    public GameObject playerName;
    DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;

    public int numberOfCreated = 0;

    public GameObject loadingScreen;
    
    
    //private float[] pixWidth;
    // Start is called before the first frame update
    void Start()
    {
        pool.ResourceCache.Clear();
        assetData.Clear();
        for (int i = 0; i < defaults.Length; i++)
        {
            assetData.Add(defaultsNames[i],defaults[i]);
        }

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            GameObject sceneAssets = GameObject.Find("AssetHolder");
            assetHolder holder = sceneAssets.GetComponent<assetHolder>();
            foreach (var AssetName in defaultsNames)
            {
                Create(holder.assets[AssetName],AssetName);
            }
            Destroy(sceneAssets);
        }
        else
        {
            
        }
      
        //add callback to spawn the player and room
        StartCoroutine(waitForAsets());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void renamePlayer(string Name, int id)
    {
        photonView.RPC("namePlayer",RpcTarget.AllBuffered,id,Name);
    }

    public void changePlayer(GameObject player)
    {
        //change the texture of my player on all clients
        GameObject sceneAssets = GameObject.Find("AssetHolder");
        assetHolder holder = sceneAssets.GetComponent<assetHolder>();

        int[,] colors = holder.assets["Character"];
        List<int> color1D = new List<int>();
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
        //sceneAssets.SetActive(false);
    }
    IEnumerator waitForAsets()
    {
        
        print("start wait");
        yield return new WaitUntil(() => numberOfCreated == defaults.Length);
        print("end wait");
        PhotonNetwork.Instantiate("Arena", Vector3.zero, Quaternion.identity);
        controller.SetActive(true);
        scenebounds.SetActive(true);
        loadingScreen.SetActive(false);
        
       // controller.GetComponent<PUN2_RoomController>().spawnPoint = spawns;
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            controller.GetComponent<PUN2_RoomController>().joinedFromCreate();
        }
        else
        {
            
        }
    }
    public void Create(int [,] colors, string name)
    {
        //call RPC, it has to be buffered so that players that join later will create the assets as well
        List<int> color1D = new List<int>();
        int arrayWidth = colors.GetLength(0);
        for (int y = 0; y < colors.GetLength(1); y++)
        {
            for (int x = 0; x < arrayWidth; x++)
            {
                color1D.Add(colors[x,y]);
            }
        }
        gameObject.GetPhotonView().RPC("copyComponents",RpcTarget.AllBuffered,color1D.ToArray(),arrayWidth,name);
    }
    [PunRPC]public void copyComponents(int[] colors1D,int width, string nameRPC)
    {
       
        int height = colors1D.Length / width;
        int[,] colorsRPC = new int[width, height];
        for (int i = 0; i < colors1D.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            colorsRPC[x, y] = colors1D[i];
        }
        
        GameObject newAsset = assetData[nameRPC];
        //assign the default object to the sceneAssets
        newAsset = Instantiate(newAsset);
        if (newAsset.CompareTag("Player"))
        {
            newAsset.GetComponent<PUN2_PlayerSync>().enabled = true;
        }

        assetInfo info = newAsset.GetComponent<assetInfo>();
       
        newAsset.name = nameRPC;
        //defaultParent.transform.parent = objectPrefab.transform.parent;
        //check if default parent has children, if yes then destroy them
        /*
        int childCount = DefaultGameObject.transform.childCount;

        if (childCount != 0)
        {
            for (int j = 0; j < childCount; j++)
            {
                //destroy children of the parent object
                Destroy(DefaultGameObject.transform.GetChild(j).gameObject);
            }
        }
*/
        //remove collider and SpriteRenderer

        if (newAsset.GetComponent<PolygonCollider2D>() != null)
        {
            Destroy(newAsset.GetComponent<PolygonCollider2D>());
        }
        
        //Destroy(defaultParent.GetComponent<SpriteRenderer>());
       // defaultParent.transform.position = objectPrefab.transform.position;
       //CombineSpriteArray();
        // Sprite finalSprite = objectPrefab.GetComponent<SpriteRenderer>().sprite;
        //  DefaultGameObject.GetComponent<SpriteRenderer>().sprite = finalSprite;
        CombineSpriteArray(newAsset, colorsRPC);
      
        Vector2 defSize = new Vector2(info.width, info.height);
        ResizeAssets(newAsset,defSize);
        
        //player is facing the wrong direction
        if (newAsset.CompareTag("Player"))
        {
           // newAsset.transform.Rotate(0,180,0);
            GameObject nameHolder = Instantiate(playerName);
            nameHolder.transform.position = newAsset.transform.position + new Vector3(0, 1.4f, 0);
            nameHolder.transform.rotation = Quaternion.Euler(0,0,0);
            nameHolder.transform.parent = newAsset.transform;
            nameHolder.transform.localScale = Vector3.one * 3;
            nameHolder.GetComponent<TMP_Text>().text = PhotonNetwork.LocalPlayer.NickName;
        }
        //parentHousing connects the life bar with the weapon
      
        if (newAsset.CompareTag("weapon") && nameRPC != "Projectile")
        {
            //spawn the life bar and move it to the weapon
            GameObject bar = Instantiate(lifeBar);
            bar.transform.position = newAsset.transform.position + new Vector3(0, 1.2f, 0);
           
            bar.transform.parent = newAsset.transform;
            bar.GetComponent<decreaseWeaponLife>().weapon = newAsset;
           
        }

        if (newAsset.TryGetComponent<CreateTrail>(out var component))
        {
            component.createTexture();
        }
        info.hasSprite = false;
        pool.ResourceCache.Add(nameRPC, newAsset);
            // DontDestroyOnLoad(DefaultGameObject);
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
                    //combinedTexture.SetPixels(x * spritesArray.Length, y * spritesArray[0].Length, spritesWidth, spritesHeight, spritesArray[x][y].texture.GetPixels((int)spritesArray[x][y].textureRect.x, (int)spritesArray[x][y].textureRect.y, (int)spritesArray[x][y].textureRect.width, (int)spritesArray[x][y].textureRect.height));
                    // For a working script, use:
                    if (colorsRPC[x, y] == 1)
                    {
                        //set the pixel black
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
                    
                   // pixelClass[x,y].deactivate();
                    
                   
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
            Collider2D[] colls = attach.GetComponents<Collider2D>();
            if (colls != null)
            {
                foreach (var coll in colls)
                {
                    Destroy(coll);
                }
            }
            float pixWidth = combinedTexture.width / texturePixelSize.x;
           //go from the middle of the sprite to the bootom corner and from there navigate with firstblackPixel to the first corner of the real sprite, that is the starting position
          
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
        /*
        Vector2[] cornerOffset = new Vector2[4];
        //bottom left
        cornerOffset[0] = new Vector2(-0.5f, -0.5f);
        //bottom right
        cornerOffset[1] = new Vector2(0.5f, -0.5f);
        //top left 
        cornerOffset[2] = new Vector2(-0.5f, 0.5f);
        //top Right
        cornerOffset[3] = new Vector2(0.5f, 0.5f);
        */
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
        //ref is a reference to the rray so that i
        deleteCompletedPixels(startPixelIndex, ref array);
        return corners.ToArray();
    }
    //delete completed pixels asks for a starting pixel and deletes every pixel that is in touch with the starting one or any other in this chain
    //it does not delete a corner touch pixel
    public void deleteCompletedPixels(Vector2 startIndex, ref int[,] array)
    {
        //this will delete every pixel it touches and chain it
        Vector2 curIndex = startIndex;
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        int curPixel =  array[(int)curIndex.x, (int)curIndex.y];
        if (curPixel == 1)
        {
            //curPixel.delete();
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
        Vector2 cornerOff = cornerPosition - pixelIndex;
        touchingPixelsIndexes[0] = cornerPosition + new Vector2(0.5f, 0.5f);// new Vector2(pixelIndex.x + cornerOff.x * 2, pixelIndex.y);
        touchingPixelsIndexes[1] = cornerPosition + new Vector2(-0.5f, 0.5f);//new Vector2(pixelIndex.x, pixelIndex.y +cornerOff.y*2);
        touchingPixelsIndexes[2] = cornerPosition + new Vector2(0.5f, -0.5f);//new Vector2(pixelIndex.x + cornerOff.x * 2, pixelIndex.y+ cornerOff.y*2);
        touchingPixelsIndexes[3] = cornerPosition + new Vector2(-0.5f, -0.5f);//new Vector2(pixelIndex.x + cornerOff.x * 2, pixelIndex.y+ cornerOff.y*2);
     
        int width = array.GetLength(0);
        int height = array.GetLength(1);
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
            else
            {
                //we are putside the bounds
               
            }
        }
       
        //we have four options
        //1) we are touching one pixel so we go straight
        //2) touching two pixels, we go to the one that is oposite to ours
        //3) we are touching none, go to the next corner of the same pixel
        //4) we are touching only the pixel oposite to ours, so the touch is only in the corner, ignore tjis and act like we are touching none
        
        //1 && 4)
        if (touchingPixels.Count == 2)
        {
            /*
            Vector2 diff = touchingPixels[0].Item2 - pixelIndex;
            if (diff.x != 0 && diff.y != 0)
            {
                //this is 4), it is the oposite one
            }
            else
            {
                // this is 1) so move in the direction of diff
                returnValue = new Tuple<Vector2, Vector2,Vector2>(cornerPosition + diff, touchingPixels[0].Item2,diff);
            }
            */
            if (touchingPixels.Contains(pixelIndex + direction))
            {
                //we have checked that this is the following pixel in line
                returnValue =
                    new Tuple<Vector2, Vector2, Vector2>(cornerPosition + direction, pixelIndex + direction, direction);
            }
            else
            {
                //it is a corner touch
                //clear the array and add the current pixel, the code bellow for count==1 will trigger
                touchingPixels.Clear();
                touchingPixels.Add(pixelIndex);
            }
        }

        if (touchingPixels.Count == 3)
        {
            /*
            Vector2 notTheOpposite = Vector2.zero;
            Vector2 opposite = Vector2.zero;
            for (int i = 0; i < 2; i++)
            {
                Vector2 diff = touchingPixels[i].Item2 - pixelIndex;
                if (diff.x == 0 || diff.y == 0)
                {
                    //this isnt the oposite one, ignore it
                    notTheOpposite = touchingPixels[i].Item2;
                }
                else
                {
                    //this is the oposite one
                    //we get the direction as the opposite pixel minus the pixel that touches both the pixels
                    opposite = touchingPixels[i].Item2;
                }
            }
            Vector2 dir = opposite - notTheOpposite;
            returnValue = new Tuple<Vector2, Vector2, Vector2>(cornerPosition + dir, notTheOpposite,dir);
            */
            Vector2[] normals = new Vector2[2];
           
            //Vector2 normal = cornerOffset - direction;
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
        Vector2 coordinates = Vector2.positiveInfinity;
        bool breakOut = false;
        for (int y = 0; y < array.GetLength(1); y++)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                int possiblePixel =array[x, y];
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
        SpriteRenderer spriteRenderer = DefaultGameObject.GetComponent<SpriteRenderer>();
        int index = 0;
        Vector2 startCorner = Vector2.zero;
        bool foundStartCorner = false;
        List<Vector2> collPoints = new List<Vector2>();
       
       //conversion from pixels to units is that we divide it by 100
        float factor = pixWidth * 0.01f;
      
        List<Vector3> spawnppoints = new List<Vector3>();
        while (ArePixelsLeft(array))
        {
            collPoints.AddRange(getColliderPoints(ref array));
            coll.pathCount = index + 1;
            for (int i = 0; i < collPoints.Count; i++)
            {
                //we have to resize the vectors to the real size of the sprite
                collPoints[i] *= factor;
            }

            //start corner is the most bottom left corner of the collider
            if (!foundStartCorner)
            {
                startCorner = collPoints.First();
                foundStartCorner = true;
            }
            coll.SetPath(index, collPoints.ToArray());
            /*
            Tuple<Vector2, Vector2> bounds = getBounds(collPoints.ToArray(), coll);
            spawnppoints.AddRange(getSpawnPoints(bounds.Item1,bounds.Item2));
            */
            index++;
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
        //create the default asset and fetch its size
        Vector3 createdSize = resizeThis.GetComponent<SpriteRenderer>().sprite.bounds.size;
        //get x and y scale
        Vector2 scale = new Vector2(defaultSize.x / createdSize.x, defaultSize.y / createdSize.y);

        //get which scale is bigger
        if (scale.x < scale.y)
        {
            //use the smaller scale
            //we want to multiply both sides with one scale to have the same aspect ratio
            resizeThis.transform.localScale = resizeThis.transform.localScale * scale.x;
        }
        else
        {
            //y scale is smaller
            resizeThis.transform.localScale = resizeThis.transform.localScale * scale.y;
        }
        print("final size: " + resizeThis.name);
        
    }
    [PunRPC] public void namePlayer(int gameobjectID, string nameID)
    {
        print("rename player");
        GameObject player = PhotonView.Find(gameobjectID).gameObject;
        player.name = nameID;
        
       
    }

    [PunRPC] public void changePlayerTexture(int gameobjectID, int[] colors1D, int width)
    {
        GameObject player = PhotonView.Find(gameobjectID).gameObject;
        int length = colors1D.Length;
        int height =  length/ width;
        int[,] colorsRPC = new int[width, height];
        for (int i = 0; i < length; i++)
        {
            int x = i % width;
            int y = i / width;
            colorsRPC[x, y] = colors1D[i];
        }
        CombineSpriteArray(player, colorsRPC);
       
        print("changed texture for player!!!!!");
    }
    private Tuple<Vector2, Vector2> getBounds(Vector2[] path,PolygonCollider2D coll)
    {
        Vector2 minX = new Vector2(1000,-1000);
        Vector2 maxX = new Vector2(-1000,-1000);
        Vector2 offset = coll.offset;
        foreach (var pointLocal in path)
        {
            
            //we have to transform from local space to world space
            Vector2 point = pointLocal - offset;//coll.transform.TransformPoint(pointLocal - offset);
            print(point.x);
            float xValue = point.x;
            if (xValue < minX.x)
            {
                minX.x = xValue;
            }

            if (xValue > maxX.x)
            {
                maxX.x = xValue;
            }
            //we have to retrieve the roof of the platform
            if (minX.y < point.y)
            {
                minX.y = point.y;
                maxX.y = point.y;
            }
        }

        return new Tuple<Vector2, Vector2>(minX, maxX);
    }

    private Vector3[] getSpawnPoints(Vector2 min, Vector2 max)
    {
        List<Vector3> spawnList = new List<Vector3>();
        
            int minX = (int)min.x;
            int maxX = (int)max.x;
            int maxY = (int)max.y;
                
            int spawnY = (int)maxY + 1;
            for (int x = (int)minX + 1; x < (int)maxX - 1; x++)
            {
                //now we have increments of 1 on the length of the platform
                Vector3 position = new Vector3(x, spawnY, 0);
                //check if we are safe to spawn there by casting a raycast to the spawnPosition and checking if it collides with something
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, position, out hit, Mathf.Infinity))
                {
                    // Debug.DrawRay(Camera.main.transform.position, position * hit.distance, Color.yellow);
                    if (!hit.collider.gameObject.CompareTag("ground"))
                    {
                        //we did not hit ground 
                        spawnList.Add(position);
                    }
                    //we did hit something, dont spawn here
                }
                else
                {
                    //we did not hit anything we are safe to spawn here
                    spawnList.Add(position);
                }

            }

            return spawnList.ToArray();
    }
}
