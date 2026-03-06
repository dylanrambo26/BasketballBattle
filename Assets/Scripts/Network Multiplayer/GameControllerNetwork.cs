using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

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

        public NetworkVariable<float> currentTime = new NetworkVariable<float>(10f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> currentHalf = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> endOfHalf = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        private UiController uiControllerScript;
        private bool startGameCountdownRunning = false;
        
        public override void OnNetworkSpawn()
        {
            if (uiControllerScript == null)
            {
                uiControllerScript = GameObject.FindGameObjectWithTag("UIController").GetComponent<UiController>();
            }

            currentTime.OnValueChanged += OnTimeChanged;
            endOfHalf.OnValueChanged += OnEndOfHalfChanged;
            
            if (uiControllerScript != null)
            {
                OnTimeChanged(0, currentTime.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            currentTime.OnValueChanged -= OnTimeChanged;
            endOfHalf.OnValueChanged -= OnEndOfHalfChanged;
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
            if (uiControllerScript == null)
            {
                print("null");
            }
            uiControllerScript.timerText.text = newVal < 60 ? time.ToString(@"m\:ss\.f") : time.ToString(@"m\:ss");
        }

        private void OnEndOfHalfChanged(bool oldVal, bool newVal)
        {
            if (newVal)
            {
                StartCountdownRpc(endOfHalf.Value);
            }
        }

        private void EndOfHalf()
        {
            if (!IsServer) return;
            
            currentTime.Value = 120;
            currentHalf.Value++;
            isGamePaused.Value = true;
            endOfHalf.Value = true;
            StartCoroutine(UnpauseAfterCountdown());
        }

        private void StartGameCountdown()
        {
            if (!IsServer) return;

            startGameCountdownRunning = true;
            StartCountdownRpc(false); 
            StartCoroutine(UnpauseAfterCountdown());
        }

        private IEnumerator UnpauseAfterCountdown()
        {
            float waitTime = UiController.countdownStart + 0.5f;
            yield return new WaitForSeconds(waitTime);

            gameStarted.Value = true;
            isGamePaused.Value = false;
            endOfHalf.Value = false;
            startGameCountdownRunning = false;
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void StartCountdownRpc(bool isSecondHalf)
        {
            if (isSecondHalf)
            {
                uiControllerScript.halfText.text = "2nd Half";
            }
            uiControllerScript.StartCoroutine(uiControllerScript.Countdown());
        }

        private void Update()
        {
            if (!IsServer) return;

            bool waitingForPlayers = NetworkManager.Singleton.ConnectedClients.Count < 2;

            if (startGameCountdownRunning)
            {
                isGamePaused.Value = true;
            }
            else
            {
                isGamePaused.Value = waitingForPlayers || endOfHalf.Value || !gameStarted.Value;
            }
            uiControllerScript.EnableWaitingForPlayersText(waitingForPlayers);

            if (!waitingForPlayers && !gameStarted.Value && !endOfHalf.Value && !startGameCountdownRunning)
            {
                StartGameCountdown();
            }
            
            if (!isGamePaused.Value)
            {
                currentTime.Value = Mathf.Max(0f, currentTime.Value - Time.deltaTime);
            }

            if (currentTime.Value <= 0f && !endOfHalf.Value)
            {
                EndOfHalf();
            }
        }
    }
}
