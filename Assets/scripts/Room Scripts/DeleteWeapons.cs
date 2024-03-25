using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DeleteWeapons : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject weapon = other.gameObject;
        if (weapon.CompareTag("Player"))
        {
            print("!!!!!hit player");
            return;
        }
        if (weapon.GetPhotonView().IsMine || weapon.transform.parent == null)
        {
            print("!!!!§!hit weapon");
            PhotonNetwork.Destroy(weapon);
        }
    }
}
