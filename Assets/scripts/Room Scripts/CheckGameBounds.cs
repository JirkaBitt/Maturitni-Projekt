using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class CheckGameBounds : MonoBehaviourPunCallbacks
{
    private RoomController controller;
    void Start()
    {
        controller = GameObject.Find("_RoomController").GetComponent<RoomController>();
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        //player left the game space
        GameObject gObject = other.gameObject;
        //we can only delete the player from isMine
        if (gObject.CompareTag("Player"))
        {
            PhotonView playerView = gObject.GetPhotonView();
            if (!playerView.IsMine || !controller.gameIsActive)
            {
                //if this is not my player return
                //if the game is not active return, when deleting player onCollisionExit is called
                return;
            }
            PlayerStats stats = gObject.GetComponent<PlayerStats>();
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
            //delete Weapon if player is holding it
            if (weapon != null)
            {
                //PickWeapon pickScript = other.GetComponent<PickWeapon>();
                int photonID = weapon.GetPhotonView().ViewID;
                //drop the Weapon and then destroy it
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
            Destroy(gObject);
        }
    }

    [PunRPC]
    public void deleteWeapon(int weaponID, int playerID)
    {
        GameObject player = PhotonView.Find(playerID).gameObject;
        GameObject weapon = PhotonView.Find(weaponID).gameObject;
        PickWeapon pickScript = player.GetComponent<PickWeapon>();
        //drop the Weapon on all instances
        pickScript.drop(true,weapon);
    }

    [PunRPC]
    public void addScore(int addObj, int removeObj, bool hasAttacker)
    {
        //add a point for the attacker and remove one for the defeated
        GameObject defeated = PhotonView.Find(removeObj).gameObject;
        PlayerStats defeatedStats = defeated.GetComponent<PlayerStats>();
        defeatedStats.score -= 1;
        defeatedStats.percentage = 0;
        if (hasAttacker)
        {
             GameObject attacker = PhotonView.Find(addObj).gameObject;
             attacker.GetComponent<PlayerStats>().score += 1;
             defeatedStats.lastAttacker = null;
        }
    }
}
