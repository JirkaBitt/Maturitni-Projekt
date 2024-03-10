
using UnityEngine;

public class playerStats: MonoBehaviour
{
  public GameObject currentWeapon;
  public int percentage;
  //score is added when we launch someone and is decreased when we fall
  public int score = 0;
  //the reference to the last player that hit us
  public GameObject lastAttacker;
  private void Start()
  {
    percentage = 0;
  }
}
