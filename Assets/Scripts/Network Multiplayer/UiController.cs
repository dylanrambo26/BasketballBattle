using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network_Multiplayer
{
    public class UiController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI leftPlayerScoreText;
        [SerializeField] private TextMeshProUGUI rightPlayerScoreText;
        [SerializeField] private TextMeshProUGUI countDownText;
        [SerializeField] private TextMeshProUGUI waitingForMorePlayersText;
        [SerializeField] private Button hostPlayAgainButton;
        [SerializeField] private Button hostQuitButton;
        [SerializeField] private Button clientQuitButton;
        [SerializeField] private TextMeshProUGUI clientWaitingForHostText;
        public TextMeshProUGUI gameOverText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI halfText;
        public GameObject hostEndGameMenu;
        public GameObject clientEndGameMenu;
        
        //Start of Game/Halftime Countdown duration
        public const int countdownStart = 3;
        
        [Header("Script References")]
        [SerializeField] private GameControllerNetwork gameController;
    
        private void Start()
        {
            //Assign reference
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerNetwork>();
            
            //Add event listeners for scoring
            gameController.leftScore.OnValueChanged += OnLeftScoreChanged;
            gameController.rightScore.OnValueChanged += OnRightScoreChanged;

            //Initialize score texts
            leftPlayerScoreText.text = gameController.leftScore.Value.ToString();
            rightPlayerScoreText.text = gameController.rightScore.Value.ToString();
            
            //Add on click listeners for end game menu buttons
            hostPlayAgainButton.onClick.AddListener(OnHostPlayAgainClicked);
            hostQuitButton.onClick.AddListener(OnHostQuitClicked);
            clientQuitButton.onClick.AddListener(OnClientQuitClicked);
        }

        //If the host clicks play again the game is restarted
        private void OnHostPlayAgainClicked()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            gameController.RequestRestartRpc();
        }

        //If host clicks quit, both clients are disconnected
        private void OnHostQuitClicked()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            hostEndGameMenu.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            NetworkManager.Singleton.Shutdown();
        }

        //Locally shutdown network manager connection on client only
        private void OnClientQuitClicked()
        {
            clientEndGameMenu.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            NetworkManager.Singleton.Shutdown();
        }

        //Update left player score UI
        private void OnLeftScoreChanged(int oldVal, int newVal)
        {
            leftPlayerScoreText.text = newVal.ToString();
        }

        //Update right player score UI
        private void OnRightScoreChanged(int oldVal, int newVal)
        {
            rightPlayerScoreText.text = newVal.ToString();
        }
        
        //Show the waiting for player to join text on host
        public void EnableWaitingForPlayersText(bool enable)
        {
            waitingForMorePlayersText.gameObject.SetActive(enable);
        }

        //Starts a three second countdown visible on both clients
        public IEnumerator Countdown()
        {
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
            countDownText.color = Color.white;
        }
    }
}
