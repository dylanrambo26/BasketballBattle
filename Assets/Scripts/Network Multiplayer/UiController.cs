using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network_Multiplayer
{
    public class UiController : MonoBehaviour
    {
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
        
        public const int countdownStart = 3;
        [SerializeField] private GameControllerNetwork gameController;
    
        private void Start()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerNetwork>();
            gameController.leftScore.OnValueChanged += OnLeftScoreChanged;
            gameController.rightScore.OnValueChanged += OnRightScoreChanged;

            leftPlayerScoreText.text = gameController.leftScore.Value.ToString();
            rightPlayerScoreText.text = gameController.rightScore.Value.ToString();
            
            hostPlayAgainButton.onClick.AddListener(OnHostPlayAgainClicked);
            hostQuitButton.onClick.AddListener(OnHostQuitClicked);
            clientQuitButton.onClick.AddListener(OnClientQuitClicked);
        }

        private void OnHostPlayAgainClicked()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            gameController.RequestRestartRpc();
        }

        private void OnHostQuitClicked()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            hostEndGameMenu.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            NetworkManager.Singleton.Shutdown();
        }

        private void OnClientQuitClicked()
        {
            clientEndGameMenu.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            NetworkManager.Singleton.Shutdown();
        }

        private void OnLeftScoreChanged(int oldVal, int newVal)
        {
            leftPlayerScoreText.text = newVal.ToString();
        }

        private void OnRightScoreChanged(int oldVal, int newVal)
        {
            rightPlayerScoreText.text = newVal.ToString();
        }
        
        public void EnableWaitingForPlayersText(bool enable)
        {
            waitingForMorePlayersText.gameObject.SetActive(enable);
        }

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
