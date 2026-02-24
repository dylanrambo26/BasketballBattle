using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private float horizontalInput;
    private float moveSpeed = 6f;
    private float jumpShotForce = 9f;
    private float blockJumpForce = 12f;
    private float jumpForce;
    public bool isPlayer1 = false;

    private bool isChargingShot;
    private float chargeStartTime;
    public float maxChargeTime = 1.2f;
    
    private Rigidbody2D rigidBody;

    public bool isGrounded = true;
    
    public BallMovement ballMovementScript;
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
                jumpForce = ballMovementScript.IsPossessedBy(true) ? jumpShotForce : blockJumpForce;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
            if (Input.GetKeyDown(KeyCode.S) && ballMovementScript.IsPossessedBy(true))
            {
                isChargingShot = true;
                chargeStartTime = Time.time;
            }

            if (Input.GetKeyUp(KeyCode.S) && isChargingShot)
            {
                isChargingShot = false;
                float heldTime = Time.time - chargeStartTime;
                float chargeAmount = Mathf.Clamp01(heldTime / maxChargeTime);
                
                ballMovementScript.ShootBall(true, chargeAmount);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            {
                jumpForce = ballMovementScript.IsPossessedBy(false) ? jumpShotForce : blockJumpForce;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && ballMovementScript.IsPossessedBy(false))
            {
                isChargingShot = true;
                chargeStartTime = Time.time;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow) && isChargingShot)
            {
                isChargingShot = false;
                float heldTime = Time.time - chargeStartTime;
                float chargeAmount = Mathf.Clamp01(heldTime / maxChargeTime);
                
                ballMovementScript.ShootBall(false, chargeAmount);
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
