using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //rigidbody of the player
    private Rigidbody2D rb;
    public float playerSpeed = 5.0f;
    public float jumpForce = 200f;
    public int numberOfAllowedJumps = 2;
    private int performedJumps = 0;
    public float dashForce = 300f;
    public int dashCooldown = 3;
    private bool dashAvailable = true;
    public int playerFacing = 0; //0 is right, 1 is left
    //the nameholder that displays the player nickname
    public GameObject nameBar;
    private Animator animator;
    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (gameObject.transform.childCount > 0)
        {
             nameBar = gameObject.transform.GetChild(0).gameObject;
        }
    }

    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime, 0, 0);
        if (move != Vector3.zero)
        {
            //rotate the player if we change direction
            CheckFacingDirection(move);
            //move the player
            gameObject.transform.position += move * playerSpeed;
        }
        //Jump
        if (Input.GetButtonDown("Jump") && performedJumps < numberOfAllowedJumps)
        {
            Jump(jumpForce);
        }
        //Dash
        if((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && dashAvailable)
        {
            StartCoroutine(PerformDash());
        }

    }
    void Jump(float force)
    {
        rb.AddForce(new Vector3(0, force, 0));
        performedJumps++;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //check if we hit the ground
        if (collision.gameObject.CompareTag("ground"))
        {
            //player touches the ground, reset doublejump
            if (Mathf.Round(rb.velocity.y) == 0) 
            {
                //only touches the ground if velocity = 0 else we can be on the side
                performedJumps = 0;
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        //check if we are on the ground and not falling
        if (collision.gameObject.CompareTag("ground"))
        {
            //player touches the ground, reset doublejump
            if (Mathf.Round(rb.velocity.y) == 0) 
            {
                //only touches the ground if velocity = 0 else we can be on the side
                performedJumps = 0;
            }
        }
    }
    void CheckFacingDirection(Vector3 movingVector)
    {
        if (movingVector.x != 0)
        {
            //this should return either 1 or -1, 1->right, -1->left
            float facingInt = movingVector.x / Mathf.Abs(movingVector.x);
            if (facingInt > 0)
            {
                //check if we changed directions
                if (playerFacing == 1)
                {
                    //rotate the player to the right
                    playerFacing = 0;
                    gameObject.transform.RotateAround(gameObject.transform.position, Vector3.up, 180);
                    nameBar.transform.rotation = Quaternion.Euler(0,0,0);
                }
                return;
            }
            if (facingInt < 0)
            {
                if (playerFacing == 0)
                {
                    //rotate the player to the left
                    playerFacing = 1;
                    gameObject.transform.RotateAround(gameObject.transform.position, Vector3.up, 180);
                    nameBar.transform.rotation = Quaternion.Euler(0,0,0);
                }
            }
        }
        
    }
    IEnumerator PerformDash()
    {
        dashAvailable = false;
        //create a trail behind the player
        gameObject.GetComponent<CreateTrail>().ShowDash();
        if (playerFacing == 0)
        {
            //dash right
            rb.AddForce(Vector3.right * dashForce);
        }
        else
        {
            //dash left
            rb.AddForce(Vector3.left * dashForce);
        }
        //wait for cooldown
        yield return new WaitForSeconds(dashCooldown);
        dashAvailable = true;
    }
    IEnumerator RemoveGravity(float totalForce,GameObject enemy)
    {
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        PolygonCollider2D coll = enemy.GetComponent<PolygonCollider2D>();
        rb.gravityScale = 0;
        //only target non trigger objects
        ContactFilter2D filter2D = new ContactFilter2D();
        filter2D.useTriggers = false;
        //wait until we hit an object
        float waitTime = totalForce / 50;
        Stopwatch timer = new Stopwatch();
        timer.Start();
        //remove gravity from the launched enemy for set time or until he hits a solid object
        yield return new WaitUntil(() => coll.IsTouching(filter2D) || timer.Elapsed.Seconds > waitTime);
        timer.Stop();
        rb.gravityScale = 1;
    }

    public void RemoveGrav(float force, GameObject enemy)
    {
        StartCoroutine(RemoveGravity(force, enemy));
    }
    
}
