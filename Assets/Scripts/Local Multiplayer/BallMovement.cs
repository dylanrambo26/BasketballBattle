using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class BallMovement : MonoBehaviour
{
    //References
    public Transform[] playerTransforms;
    private Rigidbody2D rigidBody;
    public PlayerControllerLocal[] playerControllerScripts;
    private Collider2D ballCollider;

    private float dribbleSpeed = 5f;

    //Dribble Variables
    public bool isPossessed;
    private float dribbleDistanceFromBody = 1f;
    private float inAirPosition = 1f;
    private Vector3 dribbleStartPos = new Vector3(1f, -0.5f, 0);
    
    //Shot Variables
    private float shotForce = 12f;
    private float minArc = 0.2f;
    private float maxArc = 2f;
    
    //Ball out of bounds locations
    private float player1OutOfBounds = 10f;
    private float player2OutOfBounds = -10f;
    
    //BallPossession states
    public enum BallPossesion { None, Player1, Player2}
    public BallPossesion ballPossesion = BallPossesion.None;

    //Events for scoring
    public UnityEvent player1Score;
    public UnityEvent player2Score;
    private void Start()
    {
        //Assign references
        rigidBody = GetComponent<Rigidbody2D>();
        playerControllerScripts[0] = GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerControllerLocal>();
        playerControllerScripts[1] = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerControllerLocal>();
        ballCollider = GetComponent<Collider2D>();

        //Set ball to random start point
        transform.position = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(0f, 4f), 0f);
    }

    private void FixedUpdate()
    {
        //Give ball to player 1 if player 2 shoots it out of bounds
        if (transform.position.x < player2OutOfBounds)
        {
            ballPossesion = BallPossesion.Player1;
            AttachBallToPlayer(playerTransforms[0]);
        }
        //Give ball to player 2 if player 1 shoots it out of bounds
        else if (transform.position.x > player1OutOfBounds)
        {
            ballPossesion = BallPossesion.Player2;
            AttachBallToPlayer(playerTransforms[1]);
        }
        
        //Don't dribble if ball isn't possessed
        if (ballPossesion == BallPossesion.None) return;
        
        //Dribble motion when ball is possessed by player1
        if (ballPossesion == BallPossesion.Player1)
        {
            if (playerControllerScripts[0].isGrounded)
            {
                dribbleStartPos.x = dribbleDistanceFromBody;
                transform.localPosition = dribbleStartPos + Vector3.up * Mathf.Abs(Mathf.Sin(Time.fixedTime * dribbleSpeed));
            }
            else
            {
                transform.localPosition = new Vector3(dribbleDistanceFromBody, inAirPosition, 0f);
            }
                
        }
        //Dribble motion when ball is possessed by player2, ball is placed on left side of player2
        else if(ballPossesion == BallPossesion.Player2)
        {
            if (playerControllerScripts[1].isGrounded)
            {
                dribbleStartPos.x = -dribbleDistanceFromBody;
                transform.localPosition = dribbleStartPos + Vector3.up * Mathf.Abs(Mathf.Sin(Time.fixedTime * dribbleSpeed));     
            }
            else
            {
                transform.localPosition = new Vector3(-dribbleDistanceFromBody, inAirPosition, 0f);
            }
        }
    }

    //Change possession when the ball collides with a player
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player1") && ballPossesion == BallPossesion.None)
        {
            ballPossesion = BallPossesion.Player1;
            AttachBallToPlayer(playerTransforms[0]);
        }
        else if (col.gameObject.CompareTag("Player2") && ballPossesion == BallPossesion.None)
        {
            ballPossesion = BallPossesion.Player2;
            AttachBallToPlayer(playerTransforms[1]);
        }
    }

    //Attach the ball to the player, disable rigidbody and collider physics when dribbling
    private void AttachBallToPlayer(Transform player)
    {
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.bodyType = RigidbodyType2D.Kinematic; //Stop ball from shoving away

        ballCollider.enabled = false;
        
        transform.SetParent(player);

        float ballRelativeToPlayerX = ballPossesion == BallPossesion.Player1 ? dribbleDistanceFromBody : -dribbleDistanceFromBody;
        transform.localPosition = new Vector3(ballRelativeToPlayerX, 0, 0);
    }

    //Shoot the ball and apply specific arc based on charge amount from player input
    public void ShootBall(bool isPlayer1, float chargeAmount)
    {
        chargeAmount = Mathf.Clamp01(chargeAmount);
        float arcAmount = Mathf.Lerp(minArc, maxArc, chargeAmount);

        float xDirection = isPlayer1 ? 1f : -1f;
        Vector2 direction = new Vector2(xDirection, arcAmount).normalized;
        
        ballPossesion = BallPossesion.None;
        transform.SetParent(null);
        ballCollider.enabled = true;

        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        rigidBody.linearVelocity = Vector2.zero;
        
        rigidBody.AddForce(direction * shotForce, ForceMode2D.Impulse);
    }

    //Check if possessed by player 1 or player 2
    public bool IsPossessedBy(bool isPlayer1)
    {
        return (isPlayer1 && ballPossesion == BallPossesion.Player1) ||
               (!isPlayer1 && ballPossesion == BallPossesion.Player2);
    }

    //Make sure ball drops through net visually when ball is scored
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player1ScoreTrigger"))
        {
            StartCoroutine(BallDropThroughNet(false));
        }
        else if (col.gameObject.CompareTag("Player2ScoreTrigger"))
        {
            StartCoroutine(BallDropThroughNet(true));
        }
    }

    //Increment score for the corresponding player and wait for 1 second for visual drop through
    private IEnumerator BallDropThroughNet(bool isPlayer1Score)
    {
        if (isPlayer1Score)
        {
            player1Score.Invoke();

            yield return new WaitForSeconds(1);
            
            ballPossesion = BallPossesion.Player2;
            AttachBallToPlayer(playerTransforms[1].transform);
        }
        else
        {
            player2Score.Invoke();
            
            yield return new WaitForSeconds(1);
            
            ballPossesion = BallPossesion.Player1;
            AttachBallToPlayer(playerTransforms[0].transform);
        }
    }
}
