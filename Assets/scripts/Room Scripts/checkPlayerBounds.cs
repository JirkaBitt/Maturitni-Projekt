using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class checkPlayerBounds : MonoBehaviourPunCallbacks
{
    private PUN2_RoomController controller;
    void Start()
    {
        controller = GameObject.Find("_RoomController").GetComponent<PUN2_RoomController>();
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        //player left the game space
        GameObject gObject = other.gameObject;
        //we can only delete the player from isMine
        if (gObject.CompareTag("Player"))
        {
            PhotonView playerView = gObject.GetPhotonView();
            if (!playerView.IsMine)
            {
                return;
            }
            playerStats stats = gObject.GetComponent<playerStats>();
            if (stats.lastAttacker != null)
            {
                //we have to credit the last guy that launched us
                photonView.RPC("addScore",RpcTarget.All,stats.lastAttacker.GetPhotonView().ViewID,playerView.ViewID,true);
            }
            else
            {
                photonView.RPC("addScore",RpcTarget.All,0,playerView.ViewID,false);
            }
            GameObject weapon = stats.currentWeapon;
            //delete weapon if player is holding it
            if (weapon != null)
            {
                //pickWeapon pickScript = other.GetComponent<pickWeapon>();
                int photonID = weapon.GetPhotonView().ViewID;
                //drop the weapon and then destroy it
                photonView.RPC("deleteWeapon",RpcTarget.All,photonID,gObject.GetPhotonView().ViewID);
            }
            //respawn the player
            int randomIndex = Random.Range(0, controller.spawnPoint.Length);
            Vector3 selectedPoint = controller.spawnPoint[randomIndex];
            gObject.transform.position = selectedPoint;
            //reset the velocity
            gObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //move camera with the player
            Camera.main.transform.position =
                new Vector3(selectedPoint.x, selectedPoint.y, Camera.main.transform.position.z);
        }
        else
        {
            print("Destroy" + gObject.name);
            Destroy(gObject);
        }
    }

    [PunRPC]
    public void deleteWeapon(int weaponID, int playerID)
    {
        GameObject player = PhotonView.Find(playerID).gameObject;
        GameObject weapon = PhotonView.Find(weaponID).gameObject;
        pickWeapon pickScript = player.GetComponent<pickWeapon>();
        //drop the weapon on all instances
        pickScript.drop(true,weapon);
    }

    [PunRPC]
    public void addScore(int addObj, int removeObj, bool hasAttacker)
    {
        GameObject defeated = PhotonView.Find(removeObj).gameObject;
        playerStats defeatedStats = defeated.GetComponent<playerStats>();
        defeatedStats.score -= 1;
        defeatedStats.percentage = 0;
        if (hasAttacker)
        {
             GameObject attacker = PhotonView.Find(addObj).gameObject;
             attacker.GetComponent<playerStats>().score += 1;
             defeatedStats.lastAttacker = null;
        }
    }
}
