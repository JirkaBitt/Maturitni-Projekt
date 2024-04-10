using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
   
    public int[,] IntMap;
    private int[,] moveCount;
    private int arrayWidth;
    private int arrayHeight;
    private bool stopSearch = false;
    private int addedSafeArea;
    public float walkSpeed;
    public float dashForce;
    //posiition that is updated in update
    private Vector2 WalkPosition = Vector2.zero;
    private int performedJumps = 0;
    public Rigidbody2D rb;
    private bool dashAvailable = true;
    //information to transform between world coordinates and int array
    public float startOffsetX = -7.2f;
    public float startOffsetY = -4.5f;
    public float stepX = 0.2f;
    public float stepY = 0.2f;

    private Coroutine oldWalk;
   // public GameObject visualPoint;
    private Vector3 previousPos;
    private void Update()
    {
        previousPos = gameObject.transform.position;
        if (WalkPosition != Vector2.zero)
        {
            //jump
            if(WalkPosition.y - gameObject.transform.position.y > 0.6f)
            {
                if (performedJumps == 0)
                {
                    StartCoroutine(Jump());
                    performedJumps = 1;
                }
                //it can happen that we will overshoot, so correct the X coordinate below
                float ToPosX = WalkPosition.x - gameObject.transform.position.x;
                if (Mathf.Abs(ToPosX) > 0.3f)
                {
                    gameObject.transform.position += new Vector3(ToPosX/Mathf.Abs(ToPosX),0,0) * Time.deltaTime * walkSpeed;
                }
            }
            //go sideways
            else
            {
                //vector from enemy to choosed position
                Vector2 ToPos = new Vector2(WalkPosition.x - gameObject.transform.position.x, WalkPosition.y - gameObject.transform.position.y);
                if (Mathf.Abs(ToPos.x) > 2 && dashAvailable)
                {
                     //we can perform a dash
                     StartCoroutine(PerformDash(ToPos));
                }
                //check if we are beeing launched
                float xVelocity = rb.velocity.x;
                if (Mathf.Abs(xVelocity) > 20 && dashAvailable &&
                    ((xVelocity < 0 && ToPos.x > 0) || (xVelocity > 0 && ToPos.x < 0)))
                {
                    //we are being launched, velocity is greater than set amount and is opposite to the direction we want to move
                    StartCoroutine(PerformDash(ToPos));
                }
                if (ToPos.x < 1.5f && Mathf.Abs(xVelocity) > 20)
                {
                    //we are moving too fast, slow down
                    ToPos /= ToPos.magnitude;
                    gameObject.transform.position -= (Vector3)ToPos * Time.deltaTime * walkSpeed;
                }
                else
                {
                    //move to the position
                    ToPos /= ToPos.magnitude;
                    gameObject.transform.position += (Vector3)ToPos * Time.deltaTime * walkSpeed;
                }
            }
        }
        //change the rotation based of previous position
        float xDiff = gameObject.transform.position.x - previousPos.x;
        if (xDiff == 0)
        {
            //we havent moved
            return;
        }
        if (xDiff < 0)
        {
            //we are facing left
            gameObject.transform.rotation = Quaternion.Euler(0,180,0);
        }
        else
        {
            //facing right
            gameObject.transform.rotation = Quaternion.Euler(0,0,0);
        }
    }

    public void StopThisCoroutine(Coroutine stopThis)
    {
        //we have to call stopcoroutine on the same object it was started on
        StopCoroutine(stopThis);
    }
    public Coroutine StartFollow(GameObject target, Vector2 offset)
    {
        return StartCoroutine(Follow(target,offset));
    }
    public void MoveTo(GameObject target, Vector2 offset)
    {
        stopSearch = false;
        //return the path from start to end with points being at the turns
        Vector2 end = WorldToMatrix((Vector2)target.transform.position + offset);
        if (!IsInOriginalBounds(end))
        {
            //check if we are in the bounds of the original not extended area, if not do not follow him, he could be falling down
            return;
        }
        //check if we are not in the ground, if yes start from higher
        while (IntMap[(int)end.x,(int)end.y] == 1)
        {
            if (IsInBounds(end + Vector2.up))
            {
                 end += Vector2.up;
            }
            else
            {
                break;
            }
        }
        Vector2 start = WorldToMatrix(gameObject.transform.position);
        while (IntMap[(int)start.x,(int)start.y] == 1)
        {
            if (IsInBounds(start + Vector2.up))
            {
                start += Vector2.up;
            }
            else
            {
                break;
            }
        }
        Spread(start,end);
        Vector2[] route = FindPath(start, end);
        if (oldWalk != null)
        {
            StopCoroutine(oldWalk);
        }
        oldWalk = StartCoroutine(Walk(route, target, offset != Vector2.zero));
    }
    IEnumerator Jump()
    {
        rb.AddForce(Vector2.up * 350);
        float waitMult = 1 / Mathf.Abs((WalkPosition.y - gameObject.transform.position.y));
        yield return new WaitForSeconds(waitMult);
        if (gameObject.transform.position.y < WalkPosition.y)
        {
            rb.AddForce(Vector2.up * 350);
            performedJumps = 2;
        }
    }
    IEnumerator PerformDash(Vector2 direction)
    {
        dashAvailable = false;
        //create a trail behind the player
        gameObject.GetComponent<CreateTrail>().ShowDash();
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        rb.AddForce(direction.normalized * dashForce);
        //wait for cooldown
        yield return new WaitForSeconds(3);
        dashAvailable = true;
    }

    public void AIRemoveGravity(float totalForce, GameObject enemy)
    {
        StartCoroutine(RemoveGravity(totalForce, enemy));
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

    IEnumerator Follow(GameObject target, Vector2 offset)
    {
        //follow supplied object
        while (true)
        {
            if (target != null)
            {
                 MoveTo(target,offset);
            }
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator Walk(Vector2[] route, GameObject target, bool hasOffset)
    {
        //iterate over the found vectors and set them as the walkPosition that is resolved in update
        float acceptableDistance = 0.4f;
        for (int i = 0; i < route.Length; i++)
        {
            Vector2 position = route[i];
            WalkPosition = MatrixToWorld(position);
            if (target == null)
            {
                //if the weapon was deleted
                yield break;
            }
            if (target.CompareTag("Player") && !hasOffset)
            {
                Vector2 diff = (Vector2)target.transform.position - (Vector2)gameObject.transform.position;
                if (diff.magnitude < 2f)
                {
                    //we are close enough to the player
                    WalkPosition = Vector2.zero;
                    yield break;
                }
            }
            yield return new WaitUntil(() => ((Vector2)gameObject.transform.position - WalkPosition).magnitude < acceptableDistance);
            WalkPosition = Vector2.zero;
        }
    }
    public Vector2 WorldToMatrix(Vector2 world)
    {
        //transform the world coordinates to the array of ints
        int xMatrix = Mathf.RoundToInt((world.x - startOffsetX) / stepX);
        int yMatrix = Mathf.RoundToInt((world.y - startOffsetY) / stepY);
        //clamp the value with the size of the array
        if (xMatrix < 0)
            xMatrix = 0;
        if (xMatrix >= arrayWidth)
            xMatrix = arrayWidth - 1;
        if (yMatrix < 0)
            yMatrix = 0;
        if (yMatrix >= arrayHeight)
            yMatrix = arrayHeight - 1;
        
        return new Vector2(xMatrix, yMatrix);
    }
    Vector2 MatrixToWorld(Vector2 before)
    {
        //transfer from array to world
        return new Vector2(startOffsetX + before.x * stepX, startOffsetY + stepX * before.y);
    }
    bool IsInBounds(Vector2 index)
    {
        //check if we are within the boundries of an array
        return index.x >= 0 && index.x < arrayWidth && index.y >= 0 && index.y < arrayHeight;
    }

    bool IsInOriginalBounds(Vector2 index)
    {
        //check if we are in the bounds of the original not extended area
        //on the y up we can only check if it isnt the very last one, bcs in that way the enemy could be far in the air
        return index.x > addedSafeArea && index.x < arrayWidth - addedSafeArea && index.y > addedSafeArea && index.y < arrayHeight - 1;
    }
    private void refreshMoves()
    {
        //refresh the array 
        for (int y = 0; y < arrayHeight; y++)
        {
            for (int x = 0; x < arrayWidth; x++)
            {
                moveCount[x, y] = 0;
            }
        }
    }
    // assign every position the number of moves neccesary to get there
    void Spread(Vector2 start, Vector2 end)
    {
        //refresh the array
        refreshMoves();
        //use queue bcs we engueue element and dequeue different one
        //this allows us to search in every dimention at the same time
        Queue<Vector2> points = new Queue<Vector2>();
        points.Enqueue(start);
        //prioritize sticking to the ground, so Vector up should be the last
        Vector2[] offsets = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down};
        bool foundEnd = false;
        while (true)
        {
            Vector2 point = points.Dequeue();
            int currentStep = moveCount[(int)point.x, (int)point.y];
            foreach (var offset in offsets)
            {
                int nextStep = currentStep + 1;
                if (point + offset == end || point == end)
                {
                    foundEnd = true;
                    break;
                }
                if (IsInBounds(point + offset))
                {
                    int x = (int)(point.x + offset.x);
                    int y = (int)(point.y + offset.y);
                    if (IntMap[x, y] == 0)
                    {
                        if (moveCount[x, y] == 0 || moveCount[x, y] > nextStep)
                        {
                            //we have not been here or it is a shorter path
                            moveCount[x, y] = nextStep;
                            points.Enqueue(new Vector2(x, y));
                        }
                    }
                }
            }
            if (foundEnd)
            {
                break;
            }
        }
    }
    Vector2[] FindPath(Vector2 start, Vector2 end)
    {
        //we have completed the array and now we have to find the path from the end to the start
        List<Vector2> moves = new List<Vector2>();
        moves.Add(end);
        int finalMove = 10000;
        //find the total number of moves required
        //now prioritize Vector down, bcs we dont want to be going horizontal in the air
        Vector2[] offsets = new[] { Vector2.down, Vector2.right, Vector2.left, Vector2.up};
        for (int i = 0; i < 4; i++)
        {
            Vector2 offset = offsets[i];
            int x = (int)(end.x + offset.x);
            int y = (int)(end.y + offset.y);
            if (IsInBounds(new Vector2(x, y)))
            {
                int move = moveCount[x, y];
                if (move != 0 && move < finalMove)
                {
                    finalMove = move;
                }
            }
        }
        Vector2 currentPos = end;
        int previousDirection = -1;
        for (int i = finalMove; i > 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
               Vector2 offset = offsets[j];
               //find new direction
                int x = (int)(currentPos.x + offset.x);
                int y = (int)(currentPos.y + offset.y);
                if (IsInBounds(new Vector2(x, y)))
                {
                    int move = moveCount[x, y];
                    //check if it the next step
                    if (move == i && IntMap[x,y] != 1)
                    {
                        if (previousDirection != j)
                        {
                            //we only want to add the points at the turns, so the direction would change
                            moves.Add(currentPos);
                        }
                        previousDirection = j;
                        currentPos = new Vector2(x, y);
                        break;
                    }
                }
            }
        }
        
        moves.Reverse();
        return moves.ToArray();
    }
    //add an area around the map so that we dont glitch ito it
    public void AddBarier(int spaceBetween, int[,] map)
    {
        //add a  safe barier around the arena so the AI would not get stuck
        addedSafeArea = spaceBetween + 1;
        int height = map.GetLength(1);
        int width = map.GetLength(0);
        arrayHeight = height + 2*spaceBetween + 2;
        arrayWidth = width + 2*spaceBetween + 2;
        IntMap =  new int[arrayWidth, arrayHeight];
        //make the array bigger, there is more space for pathfinding
        for (int y = 0; y < arrayHeight; y++)
        {
            for (int x = 0; x < arrayWidth; x++)
            {
                if (x > spaceBetween && x < width && y > spaceBetween && y < height)
                {
                    //we are int the original size
                    IntMap[x, y] = map[x - (spaceBetween + 1), y - (spaceBetween + 1)];
                }else
                {
                    IntMap[x, y] = 0;
                }
            }
        }
        //add this so there is at least one free space around the border
        moveCount = new int[arrayWidth, arrayHeight];
        //we have to clone it, bcs it would register the newly added pixels in the for loop and it would be infinite
        int[,] intClone = (int[,])IntMap.Clone();
        for (int y = 0; y < arrayHeight; y++)
        {
            for (int x = 0; x < arrayWidth; x++)
            {
                if (intClone[x,y] == 1)
                {
                    // recursively call this function
                    AddSafeSpace(new Vector2(x,y),spaceBetween,0);
                }
            }
        }
    }
    void AddSafeSpace(Vector2 position, int spaces, int spacesDone)
    {
        if (spacesDone > spaces || !IsInBounds(position))
        {
            return;
        }
        IntMap[(int)position.x, (int)position.y] = 1;
      //  Instantiate(visualPoint, MatrixToWorld(position), quaternion.identity);
        AddSafeSpace(position + Vector2.right, spaces, spacesDone + 1);
        AddSafeSpace(position + Vector2.left, spaces, spacesDone + 1);
        AddSafeSpace(position + Vector2.up, spaces, spacesDone + 1);
        AddSafeSpace(position + Vector2.down, spaces, spacesDone + 1);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        //if we touch ground reset the jumps
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb.velocity.y > -0.1f && rb.velocity.y < 0.1f)
        {
            performedJumps = 0;
        }
    }
}
