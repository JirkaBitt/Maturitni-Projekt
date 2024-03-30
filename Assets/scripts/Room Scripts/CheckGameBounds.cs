using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class CheckGameBounds : MonoBehaviourPunCallbacks
{
    public RoomController controller;
    private void OnTriggerExit2D(Collider2D other)
    {
        //player left the game space
        if (!controller.gameIsActive)
        {
            return;
        }
        GameObject gObject = other.gameObject;
        if (!gObject.CompareTag("Player"))
        {
            //dont delete bombs, bombs change to trigger and that triggers this deletion
            if (gObject.transform.parent == null && gObject.GetPhotonView().IsMine && !gObject.name.Contains("Bomb") && !gObject.name.Contains("Boom"))
            {
                print(gObject.name + "Deleted");
                PhotonNetwork.Destroy(gObject);
            }
            return;
        }
        //we can only delete the player from isMine
        PhotonView playerView = gObject.GetPhotonView();
        if (!playerView.IsMine)
        {
            //if this is not my player return
            //if the game is not active return, when deleting player onCollisionExit is called
            return;
        }
        PlayerStats stats = gObject.GetComponent<PlayerStats>();
        if (stats.lastAttacker != null)
        {
            //we have to credit the last guy that launched us
            photonView.RPC("AddScore",RpcTarget.All,stats.lastAttacker.GetPhotonView().ViewID,playerView.ViewID,true);
        }
        else
        {
            photonView.RPC("AddScore",RpcTarget.All,0,playerView.ViewID,false);
        }
        GameObject weapon = stats.currentWeapon;
        //delete Weapon if player is holding it
        if (weapon != null)
        {
            //PickWeapon pickScript = other.GetComponent<PickWeapon>();
            int photonID = weapon.GetPhotonView().ViewID;
            //Drop the Weapon and then destroy it
            gObject.GetComponent<PickWeapon>().Drop(true,weapon);
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
    [PunRPC]
    public void AddScore(int addObj, int removeObj, bool hasAttacker)
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
