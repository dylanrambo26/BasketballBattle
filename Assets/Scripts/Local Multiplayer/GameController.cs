using System;
using Network_Multiplayer;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI halfText;
   
    //Player score variables
    public int player1Score { get; private set; }
    public int player2Score { get; private set; }
   
    //Game state variables
    private int currentHalf = 1;
    public float currentTime = 120;
    public static bool isGamePaused = true;

    [Header("Script References")]
    public UiController uiControllerScript;
    
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
        //Only update time and half if game is not paused
        if (isGamePaused) return;
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            timerText.text = currentTime < 60 ? time.ToString(@"m\:ss\.f") : time.ToString(@"m\:ss");
        }
        else if (currentTime < 0 && currentHalf < 2)
        {
            uiControllerScript.StartCoroutine(uiControllerScript.Countdown());
            halfText.text = "Halftime";
            currentTime = 120;
            halfText.text = "2nd Half";
            currentHalf++;
        }
    }
}
