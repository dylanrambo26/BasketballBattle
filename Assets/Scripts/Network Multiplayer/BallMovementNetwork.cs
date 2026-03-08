using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network_Multiplayer
{
    public class BallMovementNetwork : NetworkBehaviour
    {
        [Header("References")]
        private Rigidbody2D rigidBody;
        private Collider2D ballCollider;
        [SerializeField] private GameControllerNetwork gameControllerScript;

        //Dribble Variables
        private Vector3 dribbleStartPos = new Vector3(1f, -0.5f, 0);
        private float dribbleSpeed = 5f;
        private float dribbleDistanceFromBody = 1f;
        
        //Ball in air variables
        private float inAirPosition = 1f;
        private float shotForce = 12f;
        private float minArc = 0.2f;
        private float maxArc = 2f;
        
        //Default value for no client id
        public const ulong NoOwner = ulong.MaxValue; 

        //Network Variables
        
        //PossessorClientId keeps track of the clientId who currently has possession of the ball. Only server can write.
        public NetworkVariable<ulong> PossessorClientId = new NetworkVariable<ulong>(NoOwner, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private void Awake()
        {
            //Assign references
            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

            if (ballCollider == null)
            {
                ballCollider = GetComponent<Collider2D>();
            }

            if (gameControllerScript == null)
            {
                gameControllerScript = GameObject.FindGameObjectWithTag("GameController")
                    .GetComponent<GameControllerNetwork>();
            }
        }

        //Add listener to PossessorClientId and initialize with NoOwner
        public override void OnNetworkSpawn()
        {
            PossessorClientId.OnValueChanged += ApplyPossessionState;
            ApplyPossessionState(NoOwner, PossessorClientId.Value);
        }

        //Cleanly get rid of listener and stop update
        public override void OnNetworkDespawn()
        {
            enabled = false;
            PossessorClientId.OnValueChanged -= ApplyPossessionState;
        }
    
        //Change the possession state based on who has triggered the collision with the ball when it was not possessed
        private void ApplyPossessionState(ulong previous, ulong current)
        {
            bool isPossessed = current != NoOwner;
            if (isPossessed)
            {
                //Disable rigidbody physics while dribbling
                rigidBody.linearVelocity = Vector2.zero;
                rigidBody.angularVelocity = 0f;
                rigidBody.bodyType = RigidbodyType2D.Kinematic; //Stop ball from shoving away
                ballCollider.enabled = false; //disable collider to ensure player doesn't get affected by dribbling
                
                //Only attach the ball if server is running the code
                if (IsServer)
                {
                    AttachBallToPlayer(current);
                }
            
            }
            else
            {
                DetachBall(); //Otherwise, the ball is not possessed and detach
            }
        }
    
        //Attach the ball to the client NetworkObject who has collided with the ball
        private void AttachBallToPlayer(ulong clientId)
        {
            var playerGameObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            transform.SetParent(playerGameObject.transform, worldPositionStays: true); 
        }

        //Due to the difference in local scaling between the player sprite and ball sprite, set the localScale of the ball
        //to one and detach from parent. Enable rigidbody and collider physics.
        private void DetachBall()
        {
            transform.localScale = Vector3.one;
            transform.SetParent(null, worldPositionStays: true);
            ballCollider.enabled = true;
            rigidBody.bodyType = RigidbodyType2D.Dynamic;
        }

        //Shoot the ball in the direction specified by GetXDirectionFromClient
        private void ShootBall(float chargeAmount, ulong shooterClientId)
        {
            float xDirection = GetXDirectionFromClientId(shooterClientId);
        
            //Calculate direction of shot, lower charge amount will have less arc, higher will have a high arc
            chargeAmount = Mathf.Clamp01(chargeAmount);
            float arcAmount = Mathf.Lerp(minArc, maxArc, chargeAmount);
            Vector2 direction = new Vector2(xDirection, arcAmount).normalized;
            rigidBody.linearVelocity = Vector2.zero;
            
            //Shoot the ball
            rigidBody.AddForce(direction * shotForce, ForceMode2D.Impulse);
        }

        //The host will be clientId 0, client will be 1. The host is always on the left side of the screen shooting right,
        //and the client is always on the right side shooting toward the left.
        private float GetXDirectionFromClientId(ulong clientId)
        {
            return (clientId == 0) ? 1.0f : -1.0f;
        }
    
        private void FixedUpdate()
        {
            //Only execute FixedUpdate if the server is running the code
            if (!IsServer) return;
            
            //Don't start dribble movement if no one has the ball
            if (PossessorClientId.Value == NoOwner) return;

            //Get the PlayerControllerNetwork script from the player who has the ball
            NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(PossessorClientId.Value);
            if (playerObject == null) return;
            
            PlayerControllerNetwork playerController = playerObject.GetComponent<PlayerControllerNetwork>();
            if (playerController == null) return;
            
            //Set ball's x position to be offset from the player sprite depending on the direction the player is going
            bool isGrounded = playerController.isGrounded.Value;
            float ballXPos = GetXDirectionFromClientId(PossessorClientId.Value) * dribbleDistanceFromBody;

            //If the player is grounded dribble, otherwise set the ball's position to be above the head as if they were about to shoot.
            if (isGrounded)
            {
                dribbleStartPos.x = ballXPos * dribbleDistanceFromBody;
                transform.localPosition = dribbleStartPos + Vector3.up * Mathf.Abs(Mathf.Sin(Time.fixedTime * dribbleSpeed));
            }
            else
            {
                transform.localPosition = new Vector3(ballXPos * dribbleDistanceFromBody, inAirPosition, 0f);
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            //Only change possession value if server is running the code
            if (!IsServer) return;
            if (PossessorClientId.Value != NoOwner) return;

            NetworkObject playerNetworkObject = col.gameObject.GetComponentInParent<NetworkObject>();
            if (playerNetworkObject != null && playerNetworkObject.IsPlayerObject)
            {
                PossessorClientId.Value = playerNetworkObject.OwnerClientId;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            //Only check out of bounds and add scoring if server is running the code
            if (!IsServer) return;
            if (col.gameObject.CompareTag("HostPlayerOutOfBounds"))
            {
                PossessorClientId.Value = 1; //Give ball to client
            }
            else if (col.gameObject.CompareTag("ClientPlayerOutOfBounds"))
            {
                PossessorClientId.Value = 0; //Give ball to host
            }
            //Only score if the ball hits the left player score trigger and the game is not over
            else if (col.gameObject.CompareTag("leftPlayerScoreTrigger") && !gameControllerScript.endOfGame.Value)
            {
                gameControllerScript.AddScore(0); //Left player scores
                StartCoroutine(BallDropThroughNet(0)); //Let ball pass through net visually
            }
            //Only score if the ball hits the rightplayer score trigger and the game is not over
            else if (col.gameObject.CompareTag("rightPlayerScoreTrigger") && !gameControllerScript.endOfGame.Value)
            {
                gameControllerScript.AddScore(1); //right player score
                StartCoroutine(BallDropThroughNet(1)); //Let ball pass through net visually
            }
        }

        //Send an rpc to the server to validate that the person holding the ball is shooting. Detach and shoot the ball.
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestShootServerRpc(float chargeAmount, RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            if (!IsPossessedByClient(senderId)) return;
            PossessorClientId.Value = NoOwner;
            DetachBall();
            ShootBall(chargeAmount, senderId);
        }

        //Ensure ball is possessed by the specified client
        private bool IsPossessedByClient(ulong clientId)
        {
            return PossessorClientId.Value == clientId;
        }

        //Let ball drop for 1 second and switch possession when there is a score
        private IEnumerator BallDropThroughNet(int scoringSide)
        {
            switch (scoringSide)
            {
                case 0:
                    yield return new WaitForSeconds(1f);
                    PossessorClientId.Value = 1;
                    break;
                case 1:
                    yield return new WaitForSeconds(1f);
                    PossessorClientId.Value = 0;
                    break;
            }
        }
        
        //Set a random start position at the start of the game for variability
        public void SetRandomStartPos()
        {
            if (!IsServer) return;
            transform.position = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(0f, 4f), 0f);
            transform.localScale = Vector3.one;
        }
    }
}
