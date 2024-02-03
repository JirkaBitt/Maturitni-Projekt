using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class showAllResults : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject resultContainer;
    public GameObject resultPrefab;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showResults(GameObject[] players, Dictionary<string,int> placements,Dictionary<string,string> nicks)
    {
        foreach (var player in players)
        {
            GameObject result = Instantiate(resultPrefab);
            showPlayerResult showScript = result.GetComponent<showPlayerResult>();
            string playerId = player.name;
            
            string nick = nicks[playerId];
            int placement = placements[playerId];
            
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
        GameObject result = GameObject.Find(name);
        result.GetComponent<showPlayerResult>().voteYes();
    }
    [PunRPC]
    public void leaveRPC(string name)
    {
        GameObject result = GameObject.Find(name);
        result.GetComponent<showPlayerResult>().voteNo();
    }
}
