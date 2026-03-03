using System;
using Unity.Netcode;
using UnityEngine;

public class BallMovementNetwork : NetworkBehaviour
{
    private Rigidbody2D rigidBody;
    private Collider2D ballCollider;

    private Vector3 dribbleStartPos = new Vector3(1f, -0.5f, 0);
    private float dribbleSpeed = 5f;
    private float dribbleDistanceFromBody = 1f;
    private float inAirPosition = 1f;
    
    private float shotForce = 12f;
    private float minArc = 0.2f;
    private float maxArc = 2f;
    
    private float player1OutOfBounds = 10f;
    private float player2OutOfBounds = -10f;
    
    //public bool isPossessed;
    
    
    
    public enum BallPossesion { None, Player1, Player2}
    public BallPossesion ballPossesion = BallPossesion.None;

    public const ulong NoOwner = ulong.MaxValue; //Default value for no client id

    public NetworkVariable<ulong> PossessorClientId = new NetworkVariable<ulong>(NoOwner, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        if (ballCollider == null)
        {
            ballCollider = GetComponent<Collider2D>();
        }
    }

    public override void OnNetworkSpawn()
    {
        PossessorClientId.OnValueChanged += ApplyPossessionState;
        ApplyPossessionState(NoOwner, PossessorClientId.Value);
    }

    public override void OnNetworkDespawn()
    {
        PossessorClientId.OnValueChanged -= ApplyPossessionState;
    }
    
    private void ApplyPossessionState(ulong previous, ulong current)
    {
        bool isPossessed = current != NoOwner;
        if (isPossessed)
        {
            AttachBallToPlayer(current);
        }
        else
        {
            DetachBall();
        }
    }
    
    private void AttachBallToPlayer(ulong clientId)
    {
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.bodyType = RigidbodyType2D.Kinematic; //Stop ball from shoving away

        ballCollider.enabled = false;

        var playerGameObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        transform.SetParent(playerGameObject.transform, worldPositionStays: true);
    }

    private void DetachBall()
    {
        transform.SetParent(null, worldPositionStays: false);
        ballCollider.enabled = true;
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (PossessorClientId.Value == NoOwner) return;

        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(PossessorClientId.Value);
        if (playerObject == null) return;

        PlayerControllerNetwork playerController = playerObject.GetComponent<PlayerControllerNetwork>();

        if (playerController == null) return;
        bool isGrounded = playerController.isGrounded.Value;

        float ballXPos = playerObject.transform.localScale.x >= 0 ? 1f : -1f;

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
        if (!IsServer) return;
        if (PossessorClientId.Value != NoOwner) return;

        NetworkObject playerNetworkObject = col.gameObject.GetComponentInParent<NetworkObject>();
        if (playerNetworkObject != null && playerNetworkObject.IsPlayerObject)
        {
            PossessorClientId.Value = playerNetworkObject.OwnerClientId;
        }
    }
    // [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    /*public void RequestShotServerRpc(float chargeAmount, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        //if (!IsPossessedByClient(senderId)) return;
    }*/

    /*private bool IsPossessedByClient(ulong clientId)
    {

    }*/
}
