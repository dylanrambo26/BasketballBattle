using System;
using TMPro;
using UnityEngine;

public class UiController : MonoBehaviour
{
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    private GameController gameController;

    private void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    public void UpdateP1Score()
    {
        player1ScoreText.text = gameController.player1Score.ToString();
    }
    
    public void UpdateP2Score()
    {
        player2ScoreText.text = gameController.player2Score.ToString();
    }
}
