using System;
using Unity.Netcode;
using UnityEngine;

namespace Network_Multiplayer
{
    public class GameControllerNetwork : NetworkBehaviour
    {
        public NetworkVariable<int> leftScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    
        public NetworkVariable<int> rightScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(true,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<float> currentTime = new NetworkVariable<float>(120f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> currentHalf = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private UiController uiControllerScript;

        private void Start()
        {
            currentTime.OnValueChanged += OnTimeChanged;
            OnTimeChanged(0, currentTime.Value);
        }

        public override void OnNetworkSpawn()
        {
            uiControllerScript = GameObject.FindGameObjectWithTag("UIController").GetComponent<UiController>();
        }

        public void AddScore(int scoringSide)
        {
            if (!IsServer) return;
            if (scoringSide == 0)
            {
                leftScore.Value++;
            }
            else
            {
                rightScore.Value++;
            }
        }

        private void OnTimeChanged(float oldVal, float newVal)
        {
            TimeSpan time = TimeSpan.FromSeconds(newVal);
            uiControllerScript.timerText.text = newVal < 60 ? time.ToString(@"m\:ss\.f") : time.ToString(@"m\:ss");
        }

        private void Update()
        {
            if (!IsServer) return;
            isGamePaused.Value = NetworkManager.Singleton.ConnectedClients.Count < 2;
            uiControllerScript.EnableWaitingForPlayersText(isGamePaused.Value);

            if (!isGamePaused.Value)
            {
                currentTime.Value = Mathf.Max(0f, currentTime.Value - Time.deltaTime);
            }
        }
    }
}
