using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Photon.Pun;
using UnityEngine;

public class AIBehaviourTree : MonoBehaviour
{
    // Start is called before the first frame update
    private CreatePrefab createScript;
    public PathFinder finder;
    public PickWeapon pickScript;
    private DisplayPlayerStats displayStats;
    private bool hasWeapon = false;
    private GameObject currentWeapon;
    private Coroutine oldPlayer;
    public string name = "bot";
    void Start()
    {
        if (!gameObject.GetPhotonView().IsMine)
        {
            this.enabled = false;
        }
        //remove the number of max players when spawning a bot
        PhotonNetwork.CurrentRoom.MaxPlayers--;
        //assign variables
        createScript = GameObject.Find("createPrefabs").GetComponent<CreatePrefab>();
        displayStats = GameObject.Find("displayPlayerStats").GetComponent<DisplayPlayerStats>();
        //create texture for dashing and damage
        CreateTrail trail = gameObject.GetComponent<CreateTrail>();
        trail.CreateTexture();
        trail.CreateDashTexture();
        //set up the pathfinding script
        int space = 3;
        AdjustOffsets(space);
        finder.AddBarier(space,createScript.arenaInt);
        //add the icon
        StartCoroutine(WaitForGameStart());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUpEnemy()
    {
        //prepare the searching process
        StartCoroutine(EnemyAI());
    }

    IEnumerator WaitForGameStart()
    {
        yield return new WaitUntil((() => gameObject.name != "bot"));
        //we have changed the name
        name = gameObject.name;
        displayStats.AddPlayer(name);
        RoomController controller = GameObject.Find("RoomController").GetComponent<RoomController>();
        yield return new WaitUntil((() => controller.gameIsActive));
        SetUpEnemy();
    }
    IEnumerator EnemyAI()
    {
        while (true)
        {
            if (!hasWeapon)
            {
                StartCoroutine(EnemyPickWeapon());
            }
            yield return new WaitUntil(() => hasWeapon);
            //now attack the player
            StartCoroutine(AttackPlayer());
            //wait until we lose weapon
            yield return new WaitUntil(() => !hasWeapon);
        }
    }

    IEnumerator AttackPlayer()
    {
        //keep tracking the closest player and attack him when he is in range
        StartCoroutine(CheckForEnemyInRange());
        int counter = 0;
        while (hasWeapon)
        {
            counter ++;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject playerToAttack = null;
            float lowestDistance = 100000;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == gameObject)
                {
                    continue;
                }
                float currDistance = (players[i].transform.position - gameObject.transform.position).magnitude;
                if (currDistance < lowestDistance)
                {
                    lowestDistance = currDistance;
                    playerToAttack = players[i];
                }
            }
            Vector2 offset = Vector2.zero;
            Vector2 toPlayer = (Vector2)playerToAttack.transform.position - (Vector2)gameObject.transform.position;
            //we want to go a liitle bit back from the player
            if (counter > 1)
            {
                offset = new Vector2(Random.Range(0, 4) * -toPlayer.normalized.x, Random.Range(-2, 2));
                counter = 0;
            }
            else
            {
                offset = new Vector2(Random.Range(0, 2) * -toPlayer.normalized.x, Random.Range(-1, 1));
            }
            oldPlayer = finder.StartFollow(playerToAttack,offset);
            yield return new WaitForSeconds(2);
            finder.StopThisCoroutine(oldPlayer);
        }
    }

    IEnumerator CheckForEnemyInRange()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        while (hasWeapon)
        {
            if (currentWeapon == null)
            {
                LoseWeapon();
                yield break;
            }
            
            //attack player based on the radius of weapon 
            float proximity = currentWeapon.name.Contains("Gun") ? 6 : 2.2f;
            proximity = currentWeapon.name.Contains("Bomb") ? 4 : proximity;
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil((() => CheckProximity(proximity, players)));
            //we are in a proximity of a player
            if (currentWeapon != null)
            {
                  currentWeapon.GetComponent<Weapon>().Use();
                  finder.StopThisCoroutine(oldPlayer);
            }
            else
            {
                LoseWeapon();
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    bool CheckProximity(float distance, GameObject[] players)
    {
        //check if any player is in the range for an attack
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == gameObject)
            {
                continue;
            }
            if (((Vector2)(players[i].transform.position - gameObject.transform.position)).magnitude < distance)
            {
                return true;
            }
        }
        return false;
    }
    IEnumerator EnemyPickWeapon()
    {
        //find weapon to pick
        GameObject weapon = null;
        while (weapon == null)
        {
            GameObject[] weapons = GameObject.FindGameObjectsWithTag("weapon");
            while (weapons.Length == 0)
            {
                Coroutine evadeCoroutine = finder.StartFollow(gameObject,new Vector2(Random.Range(-4, 4), Random.Range(-4, 4)));
                weapons = GameObject.FindGameObjectsWithTag("weapon");
                yield return new WaitForSeconds(1);
                finder.StopThisCoroutine(evadeCoroutine);
            }
            //GameObject weapon = null;
            float lowestDistance = 100000;
            for (int i = 0; i < weapons.Length; i++)
            {
                //find the closest weapon that isnt picked
                if (weapons[i].transform.parent != null)
                {
                    continue;
                }
                float currDistance = (weapons[i].transform.position - gameObject.transform.position).magnitude;
                //check if the collider for the weapon exists, if not it is a thrown bomb
                if (currDistance < lowestDistance && weapons[i].TryGetComponent<PolygonCollider2D>(out var weaponCollider))
                {
                    //we found a closer weapon
                    if (weaponCollider.isTrigger)
                    {
                        //if it is not trigger then it is a thrown bomb
                         lowestDistance = currDistance;
                         weapon = weapons[i];
                    }
                }
            } 
            if (weapon == null)
            {
                //we have not found a weapon, wait a second before trying again
                yield return new WaitForSeconds(1);
            }
        }
        PolygonCollider2D myColl = gameObject.GetComponent<PolygonCollider2D>();
        PolygonCollider2D weaponColl = weapon.GetComponent<PolygonCollider2D>();
        Coroutine followCoroutine = finder.StartFollow(weapon,Vector2.zero);
        yield return new WaitUntil(() => weapon == null  ||
            myColl.IsTouching(weaponColl) ||
            weapon.transform.parent != null);
        //check if the weapon wasnt destroyed
        if (weapon == null)
        {
            finder.StopThisCoroutine(followCoroutine);
            StartCoroutine(EnemyPickWeapon());
            yield break;
        }
        //one of two things happened, either we are close to the weapon to pick it up, or someone else picked it 
        //check if the weapon is able to be picked, if th collider is not trigger it means that it is a bomb that was thrown
        if (weapon.transform.parent == null && weaponColl !=  null && weaponColl.isTrigger)
        {
            //we are ok to pick it, stop the movement
            finder.StopThisCoroutine(followCoroutine);
            //now pick the weapon
            pickScript.Pick(weapon);
            currentWeapon = weapon;
            hasWeapon = true;
        }
        else
        {
            //someone else picked it, we want to start again and find new weapon
            finder.StopThisCoroutine(followCoroutine);
            StartCoroutine(EnemyPickWeapon());
            yield break;
        }
    }
    void AdjustOffsets(int spaceBetween)
    {
        int[,] colors = createScript.arenaInt;
        int length = colors.GetLength(0);
        int height = colors.GetLength(1);
        int firstX = length;
        int lastX = 0;
        int firstY = height;
        int lastY = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < length; x++)
            {
                if (colors[x,y] == 1)
                {
                    if (y < firstY)
                    {
                        firstY = y;
                    }
                    if (y > lastY)
                    {
                        lastY = y;
                    }
                    if (x < firstX)
                    {
                        firstX = x;
                    }
                    if (x > lastX)
                    {
                        lastX = x;
                    }
                }
            }
        }

        //lastX += 3;
        //the size of one pixel is the same as the scale of the object, bcs the original scale was 1 and the original size 1
        float onePixelOffset = GameObject.FindWithTag("ground").transform.localScale.x;
        //firstX is the distance from 0 to the start of the texture, length - lastx is the distance from the end to the end of the texture
        //by subtracting them we get the middle of the original drawing space
        float offsetFromMiddleX = ((lastX - firstX)/2f + firstX) * onePixelOffset;
        float offsetFromMiddleY = ((lastY - firstY)/2f + firstY) * onePixelOffset;
        GameObject arena = GameObject.FindWithTag("ground");
        //find the middle and from there offset it to the bottom left corner
        Vector2 firstPixel = (Vector2)arena.transform.position - new Vector2(offsetFromMiddleX, offsetFromMiddleY);
        //offset it by the spocebetween + 1 bcs the array gets expanded by this number on both sides
        firstPixel -= (new Vector2(spaceBetween + 1, spaceBetween + 1) * onePixelOffset);
        finder.stepX = onePixelOffset;
        finder.stepY = onePixelOffset;
        finder.startOffsetX = firstPixel.x;
        finder.startOffsetY = firstPixel.y;
    }

    public void LoseWeapon()
    {
        hasWeapon = false;
        currentWeapon = null;
    }
}
