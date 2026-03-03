using Unity.Netcode;
using UnityEngine;

public class BallMovementNetwork : NetworkBehaviour
{
    public Transform[] playerTransforms;
    private Rigidbody2D rigidBody;
    public PlayerControllerLocal[] playerControllerScripts;

    private Vector3 dribbleStartPos = new Vector3(1f, -0.5f, 0);
    private float dribbleSpeed = 5f;
    private float dribbleDistanceFromBody = 1f;
    private float inAirPosition = 1f;
    
    private float shotForce = 12f;
    private float minArc = 0.2f;
    private float maxArc = 2f;
    
    private float player1OutOfBounds = 10f;
    private float player2OutOfBounds = -10f;
    
    public bool isPossessed;
    private Collider2D ballCollider;
    
    
    public enum BallPossesion { None, Player1, Player2}
    public BallPossesion ballPossesion = BallPossesion.None;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestShotServerRpc(float chargeAmount, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        //if (!IsPossessedByClient(senderId)) return;
    }

    /*private bool IsPossessedByClient(ulong clientId)
    {
        
    }*/
}
