using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class displayPlayerStats : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public Dictionary<string, Texture2D> characterTextures = new Dictionary<string, Texture2D>();
    public Dictionary<string, playerStats> characterInfos = new Dictionary<string, playerStats>();
    private Dictionary<string, string> playerNicknames = new Dictionary<string, string>();
    public List<string> playerIDs = new List<string>();
    public Texture2D frameTexture;
    public GUIStyle percentageStyle;
    public GUIStyle playerStyle;
    public GUIStyle scoreStyle = new GUIStyle();
    
    void Start()
    {
        percentageStyle.fontSize = 90;
        percentageStyle.normal.textColor = Color.red;
        playerStyle.fontSize = 30;
        scoreStyle.fontSize =  30;
        scoreStyle.normal.textColor = Color.red;
        //playerIDs.Add(PhotonNetwork.LocalPlayer.UserId);
        //percentageStyle.normal.textColor = Color.white;
        //percentageStyle.normal.background = textBackground;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        float screenHeight = Screen.height;
        int playerCount = playerIDs.Count;
      
        for (int i = 0; i < playerCount; i++)
        {
            //find the stats with userId
            string playerID = playerIDs[i];
            try
            {
                float kMult = 220 / 345f;
                float distanceFromX = (60 + i * 670 * kMult + i * 100);
                GUI.Label(new Rect(distanceFromX, (screenHeight - 280), 670 * kMult , 345 * kMult), frameTexture);
                GUI.Label(new Rect(distanceFromX + 80, (screenHeight - 220) , 160 *kMult, 160 *kMult), characterTextures[playerID]);
                
                playerStats stats = characterInfos[playerID];
                //GUI.Label(new Rect(200 + i * 300, Screen.height - 100, 260, 60), textBackground);
                GUI.Label(new Rect(distanceFromX + 230, (screenHeight - 240), 260, 165*kMult),stats.percentage+" %" , percentageStyle);
                GUI.Label(new Rect(distanceFromX + 235, (screenHeight - 155), 280, 90*kMult),"Score: " + stats.score, scoreStyle);
                GUI.Label(new Rect(distanceFromX + 325*kMult , (screenHeight - 110), 341 * kMult, 75*kMult), playerNicknames[playerID], playerStyle);
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerIDs.Remove(otherPlayer.UserId);
        playerNicknames.Remove(otherPlayer.UserId);
        characterTextures.Remove(otherPlayer.UserId);
        characterInfos.Remove(otherPlayer.UserId);
        
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void clearValues()
    {
        playerIDs.Clear();
        playerNicknames.Clear();
        characterTextures.Clear();
        characterInfos.Clear();
    }
    public void addPlayerTexture(string id)
    {
        photonView.RPC("addPlayer",RpcTarget.AllBuffered,id);
    }
    [PunRPC]public void addPlayer(string id)
    {
        GameObject player = GameObject.Find(id);
        SpriteRenderer rend = player.GetComponent<SpriteRenderer>();
        playerStats info = player.GetComponent<playerStats>();
        characterTextures.Add(id,rend.sprite.texture);
        characterInfos.Add(id,info);
        playerIDs.Add(id);
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].UserId == id)
            {
                playerNicknames.Add(id,PhotonNetwork.PlayerList[i].NickName);
            }
        }
    }
}
