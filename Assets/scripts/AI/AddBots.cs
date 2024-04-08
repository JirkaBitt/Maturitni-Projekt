using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AddBots : MonoBehaviour
{
    public CreatePrefab createScript;
    public RoomController controller;
    public DisplayPlayerStats displayStats;
    public CameraMovement camMovement;
    private List<GameObject> spawnedBots = new List<GameObject>();
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
    }
    public void SpawnBot()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length >= 4)
        {
            //max capacity
            return;
        }

        int botNumber = spawnedBots.Count + 1;
        Vector3[] spawnPoint = controller.spawnPoint;
        int randonSpawn = Random.Range(0, spawnPoint.Length);
        GameObject bot = PhotonNetwork.Instantiate("Enemy", spawnPoint[randonSpawn], Quaternion.identity);
        string botName = "bot" + botNumber;
        int viewId = bot.GetPhotonView().ViewID;
        createScript.GetComponent<CreatePrefab>().RenamePlayer(botName, viewId,"bot");
        spawnedBots.Add(bot);
        camMovement.UpdatePlayers();
    }

    public void RemoveBot()
    {
        //remove the last spawned bot
        if (spawnedBots.Count > 0)
        {
            GameObject destroyThis = spawnedBots[^1];
            displayStats.RemovePlayer(destroyThis.name);
            spawnedBots.Remove(destroyThis);
            PhotonNetwork.Destroy(destroyThis);
            camMovement.UpdatePlayers();
        }
    }
}
