using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
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
        enabled = false;
        PossessorClientId.OnValueChanged -= ApplyPossessionState;
    }
    
    private void ApplyPossessionState(ulong previous, ulong current)
    {
        bool isPossessed = current != NoOwner;
        if (isPossessed)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
            rigidBody.bodyType = RigidbodyType2D.Kinematic; //Stop ball from shoving away

            ballCollider.enabled = false;
            if (IsServer)
            {
                AttachBallToPlayer(current);
            }
            
        }
        else
        {
            DetachBall();
        }
    }
    
    private void AttachBallToPlayer(ulong clientId)
    {
        var playerGameObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        transform.SetParent(playerGameObject.transform, worldPositionStays: true);
    }

    private void DetachBall()
    {
        transform.localScale = Vector3.one;
        transform.SetParent(null, worldPositionStays: true);
        ballCollider.enabled = true;
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    private void ShootBall(float chargeAmount, ulong shooterClientId)
    {
        float xDirection = GetXDirectionFromClientId(shooterClientId);
        
        chargeAmount = Mathf.Clamp01(chargeAmount);
        float arcAmount = Mathf.Lerp(minArc, maxArc, chargeAmount);
        
        Vector2 direction = new Vector2(xDirection, arcAmount).normalized; //fix direction later
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.AddForce(direction * shotForce, ForceMode2D.Impulse);
    }

    private float GetXDirectionFromClientId(ulong clientId)
    {
        return (clientId == 0) ? 1.0f : -1.0f;
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

        float ballXPos = GetXDirectionFromClientId(PossessorClientId.Value) * dribbleDistanceFromBody;

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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer) return;
        if (col.gameObject.CompareTag("HostPlayerOutOfBounds"))
        {
            PossessorClientId.Value = 1;
        }
        else if (col.gameObject.CompareTag("ClientPlayerOutOfBounds"))
        {
            PossessorClientId.Value = 0;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestShootServerRpc(float chargeAmount, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (!IsPossessedByClient(senderId)) return;
        PossessorClientId.Value = NoOwner;
        DetachBall();
        ShootBall(chargeAmount, senderId);
    }

    private bool IsPossessedByClient(ulong clientId)
    {
        return PossessorClientId.Value == clientId;
    }
}
