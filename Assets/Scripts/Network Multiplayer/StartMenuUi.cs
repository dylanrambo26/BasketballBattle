using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network_Multiplayer
{
    public class StartMenuUi : MonoBehaviour
    {
        public GameObject startMenuButtonParent;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject scoreboardUIParent;

        private GameControllerNetwork gameControllerScript;
        private UiController uiControllerScript;

        private void Start()
        {
            gameControllerScript =
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerNetwork>();
            uiControllerScript = GameObject.FindGameObjectWithTag("UIController").GetComponent<UiController>();
            TryHookNetworkEvents();
        }
        
        private void TryHookNetworkEvents()
        {
            if (NetworkManager.Singleton == null)
            {
                // try again next frame
                Invoke(nameof(TryHookNetworkEvents), 0.1f);
                return;
            }

            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;

            Debug.Log("StartMenuUi: hooked NetworkManager callbacks");
        }

        
        private void Awake()
        {
            hostButton.onClick.AddListener(StartHost);
            clientButton.onClick.AddListener(StartClient);
            serverButton.onClick.AddListener(StartServer);
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            uiControllerScript.hostEndGameMenu.SetActive(false);
            uiControllerScript.clientEndGameMenu.SetActive(false);
            uiControllerScript.gameOverText.gameObject.SetActive(false);
            
            EnableStartMenuParent(true);
            scoreboardUIParent.SetActive(false);
            //statusText.text = "Not Connected";
        }

        private void OnServerStopped(bool _)
        {
            EnableStartMenuParent(true);
            scoreboardUIParent.SetActive(false);
        }

        public void EnableStartMenuParent(bool enable)
        {
            startMenuButtonParent.gameObject.SetActive(enable);
        }

        private void EnableScoreboardParent(bool enable)
        {
            scoreboardUIParent.gameObject.SetActive(true);
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            EnableStartMenuParent(false);
            EnableScoreboardParent(true);
            
            gameControllerScript.ResetGameVariables();
        }
        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            EnableStartMenuParent(false);
            EnableScoreboardParent(true);
            
        }
        private void StartServer()
        {
            NetworkManager.Singleton.StartServer();
            EnableStartMenuParent(false);
            EnableScoreboardParent(true);
        }

        private void UpdateUI()
        {
            //network manager doesn't exist yet
            if (NetworkManager.Singleton == null)
            {
                EnableStartMenuParent(true);
                scoreboardUIParent.SetActive(true);
                statusText.text = "Not Connected";
                return;
            }

            //network manager exists but isn't connected
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                EnableStartMenuParent(true);
                scoreboardUIParent.SetActive(false);
                statusText.text = "Not Connected";
                return;
            }
            EnableStartMenuParent(false);
            scoreboardUIParent.SetActive(true);
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;
            statusText.text = $"{transport}\n{modeText}";
        }

        private void Update()
        {
            UpdateUI();
        }
    }
}
