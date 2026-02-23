using System;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public Transform[] playerTransforms;
    private Rigidbody2D rigidBody;

    private float dribbleInterval = 0.45f;
    private float dribbleVelocity = 6f;

    private bool isPossessed;
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player1") && !isPossessed)
        {
            AttachBallToPlayer(playerTransforms[0], true);
        }
        else if (col.gameObject.CompareTag("Player2") && !isPossessed)
        {
            AttachBallToPlayer(playerTransforms[1], false);
        }
    }

    private void AttachBallToPlayer(Transform player, bool isRight)
    {
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.bodyType = RigidbodyType2D.Kinematic; //Stop ball from shoving away

        GetComponent<Collider2D>().enabled = false;
        
        transform.SetParent(player);

        float ballRelativeToPlayerX = isRight ? 1f : -1f;
        transform.localPosition = new Vector3(ballRelativeToPlayerX, 0, 0);
        isPossessed = true;
    }
}
