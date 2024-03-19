using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ShowAllResults : MonoBehaviour
{
    public GameObject resultContainer;
    public GameObject resultPrefab;
    public GameObject[] podiumWinners = new GameObject[3]; //0 is first, 1 is second, 2 is third
    public void showResults(GameObject[] players, Dictionary<string,int> placements,Dictionary<string,string> nicks)
    {
        foreach (var player in players)
        {
            GameObject result = Instantiate(resultPrefab);
            ShowPlayerResult showScript = result.GetComponent<ShowPlayerResult>();
            string playerId = player.name;
            string nick = nicks[playerId];
            int placement = placements[playerId];
            if (placement < 4)
            {
                //add him to the podium
                addPodium(player,placement);
            }
            showScript.showResult(player, placement,nick,resultContainer);
        }
    }

    public void stay()
    {
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        gameObject.GetPhotonView().RPC("stayRPC",RpcTarget.AllBuffered,playerID+"-result");
    }
    public void leave()
    {
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        gameObject.GetPhotonView().RPC("leaveRPC",RpcTarget.AllBuffered,playerID+"-result");
        PhotonNetwork.LeaveRoom();
    }
    [PunRPC]
    public void stayRPC(string name)
    {
        //make his icon green and start him the game
        GameObject result = GameObject.Find(name);
        result.GetComponent<ShowPlayerResult>().voteYes();
    }
    [PunRPC]
    public void leaveRPC(string name)
    {
        //make the icon red and leave
        GameObject result = GameObject.Find(name);
        result.GetComponent<ShowPlayerResult>().voteNo();
    }
    private void addPodium(GameObject winner, int placement)
    {
        //add player texture to the posium that is next to the results
        GameObject place =  podiumWinners[placement - 1];
        place.SetActive(true);
        Image spriteHolder = place.GetComponent<Image>();
        Sprite winnerSprite = winner.GetComponent<SpriteRenderer>().sprite;
        spriteHolder.sprite = winnerSprite;
        spriteHolder.preserveAspect = true;
    }

    private void OnDisable()
    {
        //we have to reset the podium
        foreach (var place in podiumWinners)
        {
            place.SetActive(false);
        }
    }
}
