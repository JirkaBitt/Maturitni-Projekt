using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class playerStats: MonoBehaviour
{
  public int Knockouts = 0;
  public int photonID; 
  public GameObject currentWeapon;
  public int percentage;

  private void Start()
  {
    photonID = gameObject.GetComponent<PhotonView>().ViewID;
    percentage = 0;
  }
  
 
}
