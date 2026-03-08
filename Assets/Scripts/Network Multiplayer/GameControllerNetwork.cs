using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Network_Multiplayer
{
    public class GameControllerNetwork : NetworkBehaviour
    {
        [Header("NetworkVariables")]
        public NetworkVariable<int> leftScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    
        public NetworkVariable<int> rightScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(true,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<float> currentTime = new NetworkVariable<float>(ClockResetTime,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> currentHalf = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> endOfHalf = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> endOfGame = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        //References
        private UiController uiControllerScript;
        private BallMovementNetwork ballMovementScript;
        private StartMenuUi startMenuUiScript;

        
        private const float ClockResetTime = 60f; //Starting time value in seconds
        private bool startGameCountdownRunning = false;
        public override void OnNetworkSpawn()
        {
            //Assign references
            if (uiControllerScript == null)
            {
                uiControllerScript = GameObject.FindGameObjectWithTag("UIController").GetComponent<UiController>();
            }

            if (startMenuUiScript == null)
            {
                startMenuUiScript = GameObject.FindGameObjectWithTag("UIController").GetComponent<StartMenuUi>();
            }

            if (ballMovementScript == null)
            {
                ballMovementScript = GameObject.FindGameObjectWithTag("Ball").GetComponent<BallMovementNetwork>();
            }

            //Event Listeners
            currentTime.OnValueChanged += OnTimeChanged;
            endOfHalf.OnValueChanged += OnEndOfHalfChanged;
            
            //Initialize timer
            if (uiControllerScript != null)
            {
                OnTimeChanged(0, currentTime.Value);
            }
        }

        //Unsubscribe from event listeners
        public override void OnNetworkDespawn()
        {
            currentTime.OnValueChanged -= OnTimeChanged;
            endOfHalf.OnValueChanged -= OnEndOfHalfChanged;
        }

        //Increment the correct score only if the server is running the code, scoring side dictates which score is incremented
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

        //Set and format the timer text on time change
        private void OnTimeChanged(float oldVal, float newVal)
        {
            TimeSpan time = TimeSpan.FromSeconds(newVal);
            
            //If time is under 60 seconds show one place after the decimal, otherwise only show minutes and seconds
            uiControllerScript.timerText.text = newVal < 60 ? time.ToString(@"m\:ss\.f") : time.ToString(@"m\:ss");
        }

        //If the end of the half, start an rpc call to all clients to start the next countdown
        private void OnEndOfHalfChanged(bool oldVal, bool newVal)
        {
            if (newVal)
            {
                StartCountdownRpc(endOfHalf.Value);
            }
        }

        //Runs logic for the end of a half or the end of the game
        private void EndOfHalf()
        {
            //Only run when the server is running the code
            if (!IsServer) return;

            //If the end of the game set bool values and call EndOfGameRpc to show end game menus
            if (currentHalf.Value == 2)
            {
                currentTime.Value = 0;
                endOfGame.Value = true;
                endOfHalf.Value = false;
                
                bool leftWins = leftScore.Value > rightScore.Value;
                EndOfGameRpc(leftWins);
                return;
            }
            
            //Otherwise, reset time, increment half, and set bool values
            currentTime.Value = ClockResetTime;
            currentHalf.Value++;
            
            isGamePaused.Value = true;
            endOfHalf.Value = true;
            
            //After endOfHalf is changed to true, start countdown is called on both clients. Unpause is called to ensure game starts again
            StartCoroutine(UnpauseAfterCountdown()); 
        }
        
        //Start the countdown for the start of the game, server can only run
        private void StartGameCountdown()
        {
            if (!IsServer) return;

            startGameCountdownRunning = true;
            StartCountdownRpc(false); 
            StartCoroutine(UnpauseAfterCountdown());
        }

        //Unpause the game after the countdown has finished
        private IEnumerator UnpauseAfterCountdown()
        {
            float waitTime = UiController.countdownStart + 0.5f;
            yield return new WaitForSeconds(waitTime);

            gameStarted.Value = true;
            isGamePaused.Value = false;
            endOfHalf.Value = false;
            startGameCountdownRunning = false;
        }
        
        //Make an rpc call to clients and host to display the countdown and change half text to 2nd half if it is the second half
        [Rpc(SendTo.ClientsAndHost)]
        private void StartCountdownRpc(bool isSecondHalf)
        {
            if (isSecondHalf)
            {
                uiControllerScript.halfText.text = "2nd Half";
            }
            uiControllerScript.StartCoroutine(uiControllerScript.Countdown());
        }

        //Make an rpc call to clients and host to display end game menus on both host and client
        [Rpc(SendTo.ClientsAndHost)]
        private void EndOfGameRpc(bool leftWins)
        {
            uiControllerScript.halfText.text = "Final";
            uiControllerScript.gameOverText.text =
                leftWins ? "Game Over: Left Player Wins!" : "Game Over: Right Player Wins!";
            uiControllerScript.gameOverText.gameObject.SetActive(true);
            uiControllerScript.hostEndGameMenu.SetActive(IsHost);
            uiControllerScript.clientEndGameMenu.SetActive(!IsHost);
        }

        //Call to restart game after previous game ends. Only server can run
        [Rpc(SendTo.Server)]
        public void RequestRestartRpc()
        {
            if (!IsServer) return;
            ResetGameVariables();
        }
        
        //Call to clients and host to hide the end game menus after starting a new game
        [Rpc(SendTo.ClientsAndHost)]
        private void HideEndGameMenusRpc()
        {
            uiControllerScript.hostEndGameMenu.SetActive(false);
            uiControllerScript.clientEndGameMenu.SetActive(false);
            uiControllerScript.gameOverText.gameObject.SetActive(false);
            uiControllerScript.halfText.text = "1st Half";
        }

        //Reset all of the network variables for a new game only if server is running the code
        public void ResetGameVariables()
        {
            if (!IsServer) return;
            endOfGame.Value = false;
            endOfHalf.Value = false;
            gameStarted.Value = false;
            startGameCountdownRunning = false;

            currentHalf.Value = 1;
            currentTime.Value = ClockResetTime;
            leftScore.Value = 0;
            rightScore.Value = 0;
            
            HideEndGameMenusRpc();
        }
        

        private void Update()
        {
            //Only update if server is running the code
            if (!IsServer) return;

            //Used to prevent null reference exceptions upon game exit
            if (NetworkManager.Singleton == null) return;
            
            //waitingForPlayers indicates if both clients are ready to play
            bool waitingForPlayers = NetworkManager.Singleton.ConnectedClients.Count < 2;
            
            //Pause scoring and movement when the game is over
            if (endOfGame.Value)
            {
                isGamePaused.Value = true;
                return;
            }
            
            //Pause the game while the countdown is running, otherwise pause if still waiting for both players, 
            //its the end of the half, or the game hasn't started
            if (startGameCountdownRunning)
            {
                isGamePaused.Value = true;
            }
            else
            {
                isGamePaused.Value = waitingForPlayers || endOfHalf.Value || !gameStarted.Value;
            }
            uiControllerScript.EnableWaitingForPlayersText(waitingForPlayers); //Show a text on host that one player needs to join

            //Start the game start countdown
            if (!waitingForPlayers && !gameStarted.Value && !endOfHalf.Value && !startGameCountdownRunning)
            {
                StartGameCountdown();
            }
            
            //Decrement timer
            if (!isGamePaused.Value)
            {
                //ensure clock stays at 0 when time runs out
                currentTime.Value = Mathf.Max(0f, currentTime.Value - Time.deltaTime);
            }

            //Trigger end of half logic when end of half
            if (currentTime.Value <= 0f && !endOfHalf.Value && !endOfGame.Value)
            {
                EndOfHalf();
            }
        }
    }
}
