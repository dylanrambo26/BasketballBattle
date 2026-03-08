using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network_Multiplayer
{
    public class StartMenuUi : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject startMenuButtonParent;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject scoreboardUIParent;
        
        //Script references
        private GameControllerNetwork gameControllerScript;
        private UiController uiControllerScript;

        private void Start()
        {
            //Assign script references
            gameControllerScript =
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerNetwork>();
            uiControllerScript = GameObject.FindGameObjectWithTag("UIController").GetComponent<UiController>();
            
            //Try to add event listeners for client disconnects and server disconnections
            TryConnectNetworkEvents();
        }
        
        //Add listeners for OnClientDisconnectCallback and OnServerStopped for when the players quit at the end of game
        private void TryConnectNetworkEvents()
        {
            //Continue trying to connect listeners until the NetworkManager has completely started
            if (NetworkManager.Singleton == null)
            {
                Invoke(nameof(TryConnectNetworkEvents), 0.1f);
                return;
            }

            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        }

        //Add listeners for start menu buttons
        private void Awake()
        {
            hostButton.onClick.AddListener(StartHost);
            clientButton.onClick.AddListener(StartClient);
            serverButton.onClick.AddListener(StartServer);
        }

        //Disconnect listeners
        private void OnDisable()
        {
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }

        //When the client disconnects reset back to start menu and disable scoreboard
        private void OnClientDisconnected(ulong clientId)
        {
            //Only execute the network manager is up and running
            if (NetworkManager.Singleton == null) return;
            
            //Only execute if the clientId matches the localClientId
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            
            //Reset ui to start menu
            uiControllerScript.hostEndGameMenu.SetActive(false);
            uiControllerScript.clientEndGameMenu.SetActive(false);
            uiControllerScript.gameOverText.gameObject.SetActive(false);
            
            EnableStartMenuParent(true);
            scoreboardUIParent.SetActive(false);
        }

        //Execute when the server is disconnected
        private void OnServerStopped(bool _)
        {
            EnableStartMenuParent(true);
            scoreboardUIParent.SetActive(false);
        }

        //Helper for startmenu enabling
        public void EnableStartMenuParent(bool enable)
        {
            startMenuButtonParent.gameObject.SetActive(enable);
        }

        //Helper for scoreboard enabling
        private void EnableScoreboardParent(bool enable)
        {
            scoreboardUIParent.gameObject.SetActive(enable);
        }

        //Start the host on the network manager, activate/deactivate proper ui elements, reset the game variables
        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            EnableStartMenuParent(false);
            EnableScoreboardParent(true);
            
            gameControllerScript.ResetGameVariables(); //Doesn't need IsServer because host is server and host
        }
        
        //Start the client on the network manager
        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            EnableStartMenuParent(false);
            EnableScoreboardParent(true);
            
        }
        
        //Start the server on the netork manager
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
            
            //Enable/Disable proper ui and update the connection text in the top left of screen
            EnableStartMenuParent(false);
            scoreboardUIParent.SetActive(true);
            UpdateStatusText();
        }

        //Used to update the connection text
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
