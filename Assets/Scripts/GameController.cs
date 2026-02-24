using UnityEngine;

public class GameController : MonoBehaviour
{
    public int player1Score { get; private set; }
    public int player2Score { get; private set; }

    public void IncrementP1Score()
    {
        player1Score++;
    }
    
    public void IncrementP2Score()
    {
        player2Score++;
    }
}
