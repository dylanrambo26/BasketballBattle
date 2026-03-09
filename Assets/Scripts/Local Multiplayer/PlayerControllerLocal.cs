using System;
using UnityEngine;

public class PlayerControllerLocal : MonoBehaviour
{
    //Horizontal movement variables
    private float horizontalInput;
    private float moveSpeed = 6f;
    
    [Header("Jumping Variables")]
    [SerializeField] float jumpShotForce = 9f;
    [SerializeField] float blockJumpForce = 12f;
    private float jumpForce;
    
    public bool isPlayer1 = false;
    public bool isGrounded = true;

    //Shot charge variables
    private bool isChargingShot;
    private float chargeStartTime;
    public float maxChargeTime = 1.2f;
    
    //References
    private Rigidbody2D rigidBody;
    public BallMovement ballMovementScript;
    private void Start()
    {
        //Assign References
        isPlayer1 = gameObject == GameObject.FindGameObjectWithTag("Player1");
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //Only check for input when game is not paused
        if (GameController.isGamePaused) return;
        
        //Player 1 has wasd controls
        if (isPlayer1)
        {
            //Initiate jump
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                jumpForce = ballMovementScript.IsPossessedBy(true) ? jumpShotForce : blockJumpForce;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
            //Initiate shot charge
            if (Input.GetKeyDown(KeyCode.S) && ballMovementScript.IsPossessedBy(true))
            {
                isChargingShot = true;
                chargeStartTime = Time.time;
            }

            //Release shot
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
            //Player 2 has arrow key controls
            //Initiate jump
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            {
                jumpForce = ballMovementScript.IsPossessedBy(false) ? jumpShotForce : blockJumpForce;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
            //Initiate shot charge
            if (Input.GetKeyDown(KeyCode.DownArrow) && ballMovementScript.IsPossessedBy(false))
            {
                isChargingShot = true;
                chargeStartTime = Time.time;
            }
            //Release shot
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
        //Only allow horizontal movement when game is not paused
        if (GameController.isGamePaused) return;
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

    //On ground
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    //In air
    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
