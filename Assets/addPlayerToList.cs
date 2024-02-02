using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class addPlayerToList : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] icon;
    public GameObject[] nameHolder;
    public GameObject roomID;

    private bool[] isPlaceFree;
    private string[] playerIDs;
    void Start()
    {
        string roomIdString = PhotonNetwork.CurrentRoom.Name;
        roomID.GetComponent<TextMeshProUGUI>().SetText(roomIdString);
        
        int length = icon.Length;
        isPlaceFree = new bool[length];
        playerIDs = new string[length];
        for (int i = 0; i < length; i++)
        {
            isPlaceFree[i] = true;
            playerIDs[i] = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void addPlayer(GameObject player,string Nickname)
    {
        int numberOfPlaces = isPlaceFree.Length;
        int place = -1;
        for (int i = 0; i < numberOfPlaces; i++)
        {
            if (isPlaceFree[i])
            {
                place = i;
            }
        }

        if (place < 0)
        {
            //we are full
            return;
        }
        //add the character
        Sprite playerSprite = player.GetComponent<SpriteRenderer>().sprite;
        Vector2 spriteSize = playerSprite.bounds.size;
        float spriteMultiplier = 150 / spriteSize.y;
        float width = spriteSize.x * spriteMultiplier;
        icon[place].SetActive(true);
        icon[place].GetComponent<RectTransform>().sizeDelta = new Vector2(width, 150);
        icon[place].GetComponent<Image>().sprite = playerSprite;
        //set the text
        nameHolder[place].SetActive(true);
        nameHolder[place].GetComponent<TextMeshProUGUI>().SetText(Nickname);
        isPlaceFree[place] = false;
        playerIDs[place] = player.name;
    }

    public void removePlayer(string ID)
    {
        for (int place = 0; place < isPlaceFree.Length; place++)
        {
            if (ID == playerIDs[place])
            {
                icon[place].SetActive(false);
                nameHolder[place].SetActive(false);
                isPlaceFree[place] = true;
                playerIDs[place] = "";
            }
        }
    }
}
