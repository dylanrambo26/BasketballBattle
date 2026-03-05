using Unity.Netcode;
using UnityEngine;

namespace Network_Multiplayer
{
    public class PlayerControllerNetwork : NetworkBehaviour
    {
        private float moveSpeed = 6f;
        [SerializeField] float jumpShotForce = 9f;
        [SerializeField] float blockJumpForce = 12f;
        private float jumpForce;
        //public bool isPlayer1 = false;

        private bool isChargingShot;
        private float chargeStartTime;
        public float maxChargeTime = 1.2f;
    
        private Rigidbody2D rigidBody;

        //public bool isGrounded = true;

        public NetworkVariable<bool> isGrounded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        [Header("Player 2 Starting Values")]
        [SerializeField] private Material player2Material;
        [SerializeField] private Vector3 player2StartPos;
        private BallMovementNetwork ballMovementScript;
        public override void OnNetworkSpawn()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            ballMovementScript = GameObject.FindGameObjectWithTag("Ball").GetComponent<BallMovementNetwork>();
            if (OwnerClientId != 0)
            {
                gameObject.GetComponent<SpriteRenderer>().material = player2Material;
                gameObject.transform.position = player2StartPos;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded.Value)
            {
                /*jumpForce = ballMovementScript.IsPossessedBy(true) ? jumpShotForce : blockJumpForce;
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);*/

                jumpForce = jumpShotForce; //TODO change later
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
            }
            if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) /*&& ballMovementScript.IsPossessedByLocalPlayer()*/)
            {
                isChargingShot = true;
                chargeStartTime = Time.time;
            }

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
            if (/*GameController.isGamePaused || */!IsOwner) return; //Only move client player when game is not paused
            float horizontalInput = Input.GetAxis("Horizontal");
            rigidBody.linearVelocity = new Vector2(horizontalInput * moveSpeed, rigidBody.linearVelocity.y);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!IsOwner) return;
            if (col.gameObject.CompareTag("Ground"))
            {
                isGrounded.Value = true;
            }
            else if (col.gameObject.CompareTag("Ball"))
            {
                //RequestPossession(OwnerClientId);
            }
        }
    
        private void OnCollisionExit2D(Collision2D col)
        {
            if (!IsOwner) return;
            if (col.gameObject.CompareTag("Ground"))
            {
                isGrounded.Value = false;
            }
        }
    }
}
