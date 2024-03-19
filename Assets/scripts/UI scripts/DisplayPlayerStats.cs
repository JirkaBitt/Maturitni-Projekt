using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DisplayPlayerStats : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public Dictionary<string, Texture2D> characterTextures = new Dictionary<string, Texture2D>();
    public Dictionary<string, PlayerStats> characterInfos = new Dictionary<string, PlayerStats>();
    private Dictionary<string, string> playerNicknames = new Dictionary<string, string>();
    public List<string> playerIDs = new List<string>();
    public Texture2D frameTexture;
    //UI styles for the labels
    public GUIStyle percentageStyle;
    public GUIStyle playerStyle;
    public GUIStyle scoreStyle = new GUIStyle();
    
    void Start()
    {
        //change the styles
        percentageStyle.fontSize = 90;
        playerStyle.fontSize = 30;
        scoreStyle.fontSize =  30;
        percentageStyle.normal.textColor = Color.red;
        scoreStyle.normal.textColor = Color.red;
    }
    private void OnGUI()
    {
        //we use on gui bcs the values change frequently and if some players leave it will handle it
        float screenHeight = Screen.height;
        int playerCount = playerIDs.Count;
        for (int i = 0; i < playerCount; i++)
        {
            //find the stats with userId
            string playerID = playerIDs[i];
            try
            {
                //k mult is a scale of the texture pixels to the real height
                float kMult = 220 / 345f;
                //this depends on the player, we want space between the players
                float distanceFromX = (60 + i * 670 * kMult + i * 100);
                //show all the information about the player and his icon
                GUI.Label(new Rect(distanceFromX, (screenHeight - 280), 670 * kMult , 345 * kMult), frameTexture);
                GUI.Label(new Rect(distanceFromX + 80, (screenHeight - 220) , 160 *kMult, 160 *kMult), characterTextures[playerID]);
                PlayerStats stats = characterInfos[playerID];
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
        //if player leaves we want to remove him from the lists
        playerIDs.Remove(otherPlayer.UserId);
        playerNicknames.Remove(otherPlayer.UserId);
        characterTextures.Remove(otherPlayer.UserId);
        characterInfos.Remove(otherPlayer.UserId);
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void clearValues()
    {
        //reset all values, this is called when the games end
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
        //add a player to all the lists
        GameObject player = GameObject.Find(id);
        SpriteRenderer rend = player.GetComponent<SpriteRenderer>();
        PlayerStats info = player.GetComponent<PlayerStats>();
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
