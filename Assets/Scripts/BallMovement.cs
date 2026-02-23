using System;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public Transform[] playerTransforms;
    private Rigidbody2D rigidBody;
    public PlayerController[] playerControllerScripts;

    private float dribbleSpeed = 5f;

    private bool isPossessed;
    private bool isRight;
    private float dribbleDistanceFromBody = 1f;
    private float inAirPosition = 1f;
    private Vector3 dribbleStartPos = new Vector3(1f, -0.5f, 0);
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerControllerScripts[0] = GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerController>();
        playerControllerScripts[1] = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (!isPossessed) return;
        if (isRight)
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
        else
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
        if (col.gameObject.CompareTag("Player1") && !isPossessed)
        {
            isRight = true;
            AttachBallToPlayer(playerTransforms[0]);
        }
        else if (col.gameObject.CompareTag("Player2") && !isPossessed)
        {
            isRight = false;
            AttachBallToPlayer(playerTransforms[1]);
        }
    }

    private void AttachBallToPlayer(Transform player)
    {
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.bodyType = RigidbodyType2D.Kinematic; //Stop ball from shoving away

        GetComponent<Collider2D>().enabled = false;
        
        transform.SetParent(player);

        float ballRelativeToPlayerX = isRight ? dribbleDistanceFromBody : -dribbleDistanceFromBody;
        transform.localPosition = new Vector3(ballRelativeToPlayerX, 0, 0);
        isPossessed = true;
    }
    
    //TODO Make event listener method for shooting the ball
}
