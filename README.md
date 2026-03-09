# BasketballBattle

BasketballBattle is a 2D multiplayer basketball game built in Unity (6000.3.9f1) using Netcode for GameObjects.
Two players (one as host, one as client) play a 1 v 1 basketball game with 2 one-minute halves.

## File Structure (Important)
The multiplayer files are in the Network Multiplayer folder in this repository. Local Multiplayer consists of scripts for testing gameplayer before 
implementing network logic. The Local Multiplayer files are disabled in the game scene.

## Features

- 1v1 online multiplayer (LAN)
- Network-synced scoring and timer
- Server-authoritative gameplay

## Built With
- Unity 6000.3.9f1
- C#
- Netcode for GameObjects
- Unity Transport (UTP)
- TextMeshPro

## How to Run (Ensure both devices on the same network)
1. Clone the repository.
2. Set the Host's IP in the Unity Transport component attached to the NetworkManager in the scene.
3. Build the project. (Currently set for Windows)
4. Zip build files and send to another player.
5. The other player downloads and extracts the files.
6. Both players run the Unity executable.
7. The host selects Start Host, the client selects Start Client.
8. Play the game and have fun!

## Future Improvements
- Sound effects
- IP setting in game menu
- Improved art

## Author
dylanrambo26  
Made as a student project for CS 596 (Advanced Game Programming) SDSU