using System;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public int player1Score { get; private set; }
    public int player2Score { get; private set; }

    private float currentTime = 120;
    private int currentHalf;

    public void IncrementP1Score()
    {
        player1Score++;
    }
    
    public void IncrementP2Score()
    {
        player2Score++;
    }

    private void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            timerText.text = currentTime < 60 ? time.ToString(@"m\:ss\.ff") : time.ToString(@"m\:ss");
        }
    }
}
