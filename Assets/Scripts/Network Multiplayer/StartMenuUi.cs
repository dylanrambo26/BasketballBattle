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
        
        private void Awake()
        {
            hostButton.onClick.AddListener(StartHost);
            clientButton.onClick.AddListener(StartClient);
            serverButton.onClick.AddListener(StartServer);
        }

        private void DisableStartMenuParent()
        {
            startMenuButtonParent.gameObject.SetActive(false);
        }

        private void EnableScoreboardParent()
        {
            scoreboardUIParent.gameObject.SetActive(true);
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            DisableStartMenuParent();
            EnableScoreboardParent();
        }
        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            DisableStartMenuParent();
            EnableScoreboardParent();
            
        }
        private void StartServer()
        {
            NetworkManager.Singleton.StartServer();
            DisableStartMenuParent();
            EnableScoreboardParent();
        }

        private void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                DisableStartMenuParent();
                statusText.text = "NetworkManager not found";
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                statusText.text = "Not Connected";
            }
            else
            {
                DisableStartMenuParent();
                UpdateStatusText();
            }
        
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
