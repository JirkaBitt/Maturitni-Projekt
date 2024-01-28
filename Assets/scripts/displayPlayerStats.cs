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
    public Texture2D frameTexture;
    public GUIStyle percentageStyle;
    public GUIStyle playerStyle;
    public Texture2D textBackground;
    public float YoffsetPercentage;
    public float YoffsetPlayer;
    public float XoffsetPercentage;
    public float XoffsetPlayer;
    void Start()
    {
        percentageStyle.fontSize = 90;
        playerStyle.fontSize = 30;
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
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            //find the stats with userId
            string playerID = PhotonNetwork.PlayerList[i].UserId;
            try
            {
                float kMult = 220 / 345f;
                float distanceFromX = 60 + i * 670 * kMult + i * 100;
                GUI.Label(new Rect(distanceFromX, screenHeight - 280, 670 * kMult, 345 * kMult), frameTexture);
                GUI.Label(new Rect(distanceFromX + 80, screenHeight - 220 , 160 *kMult, 160 *kMult), characterTextures[playerID]);
                
                playerStats stats = characterInfos[playerID];
                //GUI.Label(new Rect(200 + i * 300, Screen.height - 100, 260, 60), textBackground);
                GUI.Label(new Rect(distanceFromX + 230, screenHeight - 230, 260, 165*kMult),stats.percentage+" %" , percentageStyle);
                GUI.Label(new Rect(distanceFromX + 325*kMult, screenHeight - 110, 341 * kMult, 75*kMult),PhotonNetwork.PlayerList[i].NickName , playerStyle);
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }
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
    }
}
