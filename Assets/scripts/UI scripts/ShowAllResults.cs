using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ShowAllResults : MonoBehaviour
{
    public GameObject resultContainer;
    public GameObject resultPrefab;
    public GameObject[] podiumWinners = new GameObject[3]; //0 is first, 1 is second, 2 is third
    public void ShowResults(GameObject[] players, Dictionary<string,int> placements,Dictionary<string,string> nicks)
    {
        //first clean up the previous results
        int resultCount = resultContainer.transform.childCount;
        for (int i = 0; i < resultCount; i++)
        {
            Destroy(resultContainer.transform.GetChild(i).gameObject);
        }
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
                AddPodium(player,placement);
            }
            showScript.ShowResult(player, placement,nick,resultContainer);
        }
    }

    public void Stay()
    {
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        gameObject.GetPhotonView().RPC("StayRPC",RpcTarget.AllBuffered,playerID+"-result");
    }
    public void Leave()
    {
        string playerID = PhotonNetwork.LocalPlayer.UserId;
        gameObject.GetPhotonView().RPC("LeaveRPC",RpcTarget.AllBuffered,playerID+"-result");
        PhotonNetwork.LeaveRoom();
    }
    [PunRPC]
    public void StayRPC(string name)
    {
        //make his icon green and start him the game
        GameObject result = GameObject.Find(name);
        if (result != null)
        {
            result.GetComponent<ShowPlayerResult>().VoteYes();
        }
    }
    [PunRPC]
    public void LeaveRPC(string name)
    {
        //make the icon red and leave
        GameObject result = GameObject.Find(name);
        if (result != null)
        {
            result.GetComponent<ShowPlayerResult>().VoteNo();
        }
    }
    private void AddPodium(GameObject winner, int placement)
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
