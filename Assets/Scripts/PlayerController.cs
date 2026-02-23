using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private float horizontalInput;
    private float moveSpeed = 6f;
    private float jumpForce = 9f;
    
    private bool isPlayer1 = false;
    private Rigidbody2D rigidBody;

    public bool isGrounded = true;
    private void Start()
    {
        isPlayer1 = gameObject == GameObject.FindGameObjectWithTag("Player1");
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isPlayer1)
        {
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isPlayer1)
        {
            horizontalInput = Input.GetAxis("P1_Horizontal");
        }
        else
        {
            horizontalInput = Input.GetAxis("P2_Horizontal");
        }

        rigidBody.linearVelocity = new Vector2(horizontalInput * moveSpeed, rigidBody.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
