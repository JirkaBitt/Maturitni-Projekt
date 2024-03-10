using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
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
    //particles that play when we jump
    private ParticleSystem particles;
    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        nameBar = gameObject.transform.GetChild(0).gameObject;
        particles = gameObject.GetComponent<ParticleSystem>();
        moveParticlesToFeet();
    }

    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime, 0, 0);
        if (move != Vector3.zero)
        {
            //rotate the player if we change direction
            checkFacingDirection(move);
            //move the player
            gameObject.transform.position += move * playerSpeed;
        }
        //jump
        if (Input.GetButtonDown("Jump") && performedJumps < numberOfAllowedJumps)
        {
            jump(jumpForce);
        }
        //Dash
        if((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && dashAvailable)
        {
            StartCoroutine(performDash());
        }

    }
    void jump(float force)
    {
        particles.Play();
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
    void checkFacingDirection(Vector3 movingVector)
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

    void moveParticlesToFeet()
    {
        Collider2D coll = gameObject.GetComponent<Collider2D>();
        float height = coll.bounds.size.y;
        //move the particles below the feet
        particles.transform.position = gameObject.transform.position - new Vector3(0, height / 2 + 0.2f, 0);
    }
    IEnumerator performDash()
    {
        dashAvailable = false;
        //create a trail behind the player
        gameObject.GetComponent<CreateTrail>().createTrail();
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
    
}
