using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UiControllerOld : MonoBehaviour
{
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI countDownText;
    
    private const int countdownStart = 3;
    private GameController gameController;
    
    private void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        StartCoroutine(Countdown());
    }

    public void UpdateP1Score()
    {
        player1ScoreText.text = gameController.player1Score.ToString();
    }
    
    public void UpdateP2Score()
    {
        player2ScoreText.text = gameController.player2Score.ToString();
    }

    public IEnumerator Countdown()
    {
        GameController.isGamePaused = true;
        countDownText.gameObject.SetActive(true);
        for (int i = countdownStart; i > 0; i--)
        {
            countDownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        countDownText.color = Color.green;
        countDownText.text = "GO!";

        yield return new WaitForSeconds(0.5f);
        countDownText.gameObject.SetActive(false);
        GameController.isGamePaused = false;
    }
}
