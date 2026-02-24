using System;
using UnityEngine;
using UnityEngine.Events;

public class BallMovement : MonoBehaviour
{
    public Transform[] playerTransforms;
    private Rigidbody2D rigidBody;
    public PlayerController[] playerControllerScripts;

    private float dribbleSpeed = 5f;

    public bool isPossessed;
    private float dribbleDistanceFromBody = 1f;
    private float inAirPosition = 1f;
    private Vector3 dribbleStartPos = new Vector3(1f, -0.5f, 0);
    private float shotForce = 9f;
    private float minArc = 0.2f;
    private float maxArc = 2f;
    //private float arcUp = 2;

    private Collider2D ballCollider;
    private float player1OutOfBounds = 10f;
    private float player2OutOfBounds = -10f;
    
    public enum BallPossesion { None, Player1, Player2}
    public BallPossesion ballPossesion = BallPossesion.None;

    public UnityEvent player1Score;
    public UnityEvent player2Score;
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerControllerScripts[0] = GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerController>();
        playerControllerScripts[1] = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerController>();
        ballCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (transform.position.x < player2OutOfBounds)
        {
            ballPossesion = BallPossesion.Player1;
            AttachBallToPlayer(playerTransforms[0]);
        }
        else if (transform.position.x > player1OutOfBounds)
        {
            ballPossesion = BallPossesion.Player2;
            AttachBallToPlayer(playerTransforms[1]);
        }
        
        if (ballPossesion == BallPossesion.None) return;
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

    public bool IsPossessedBy(bool isPlayer1)
    {
        return (isPlayer1 && ballPossesion == BallPossesion.Player1) ||
               (!isPlayer1 && ballPossesion == BallPossesion.Player2);
    }
    //TODO Fix ball not shooting when dribbling on the ground

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player1ScoreTrigger"))
        {
            player2Score.Invoke();
            ballPossesion = BallPossesion.Player1;
            AttachBallToPlayer(playerTransforms[0].transform);
        }
        else if (col.gameObject.CompareTag("Player2ScoreTrigger"))
        {
            player1Score.Invoke();
            ballPossesion = BallPossesion.Player2;
            AttachBallToPlayer(playerTransforms[1].transform);
        }
    }
}
