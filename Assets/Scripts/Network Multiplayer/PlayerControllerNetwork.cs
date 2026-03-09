using Unity.Netcode;
using UnityEngine;

namespace Network_Multiplayer
{
    public class PlayerControllerNetwork : NetworkBehaviour
    {
        //InternalVariables
        private const float MoveSpeed = 6f;
        private float jumpForce;
        private bool isChargingShot;
        private float chargeStartTime;
        
        [Header("Tunable Variables")]
        public float maxChargeTime = 1.2f;
        [SerializeField] float jumpShotForce = 9f;
        [SerializeField] float blockJumpForce = 12f;
        
        [Header("Player 2 Starting Values")]
        [SerializeField] private Material player2Material;
        [SerializeField] private Vector3 player2StartPos;
        
        //References
        private BallMovementNetwork ballMovementScript;
        private GameControllerNetwork gameControllerScript;
        private Rigidbody2D rigidBody;
        
        //NetworkVariables
        //isGrounded is used to make sure a player doesn't double jump and the network permissions only allow the client that is the owner to write to it
        public NetworkVariable<bool> isGrounded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        public override void OnNetworkSpawn()
        {
            //Assign references
            rigidBody = GetComponent<Rigidbody2D>();
            ballMovementScript = GameObject.FindGameObjectWithTag("Ball").GetComponent<BallMovementNetwork>();
            gameControllerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerNetwork>();
            
            //Set the right player's material and starting position
            if (OwnerClientId != 0)
            {
                gameObject.GetComponent<SpriteRenderer>().material = player2Material;
                gameObject.transform.position = player2StartPos;
            }
        }

        private void Update()
        {
            //Don't allow vertical movement or shooting when the game is paused or if a client doesn't own the script to clients only move their own player
            if (gameControllerScript.isGamePaused.Value ||!IsOwner) return;
            
            //Press W or Up Arrow to initiate a jump if the player is grounded
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded.Value)
            {
                //Apply rigidbody force for jump, if the player has the ball they will have a lower jump height than the one who doesn't
                bool hasPossession = ballMovementScript.PossessorClientId.Value == OwnerClientId;
                jumpForce = hasPossession ? jumpShotForce : blockJumpForce;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
            
            //Press and Hold S or Down Arrow to charge the shot
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                isChargingShot = true;
                chargeStartTime = Time.time;
            }

            //When the player releases the charging button, it will request the server to validate. Server handles the ball movement.
            if ((Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow)) && isChargingShot)
            {
                isChargingShot = false;
                float heldTime = Time.time - chargeStartTime;
                float chargeAmount = Mathf.Clamp01(heldTime / maxChargeTime);

                ballMovementScript.RequestShootServerRpc(chargeAmount);
            }
        }
        
        private void FixedUpdate()
        {
            //Don't move horizontally when game is paused or if a client doesn't own the script
            if (gameControllerScript.isGamePaused.Value || !IsOwner) return;
            float horizontalInput = Input.GetAxis("Horizontal");
            rigidBody.linearVelocity = new Vector2(horizontalInput * MoveSpeed, rigidBody.linearVelocity.y);
        }

        //On Ground
        private void OnCollisionEnter2D(Collision2D col)
        {
            //Only set isGrounded to true if the client owns the script
            if (!IsOwner) return;
            if (col.gameObject.CompareTag("Ground"))
            {
                isGrounded.Value = true;
            }
        }
    
        //In the air, prevent double jumping
        private void OnCollisionExit2D(Collision2D col)
        {
            //Only set isGrounded to false if the client owns the script
            if (!IsOwner) return;
            if (col.gameObject.CompareTag("Ground"))
            {
                isGrounded.Value = false;
            }
        }
    }
}
