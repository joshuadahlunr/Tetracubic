using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

// Script which provides the (currently barebones) networking behavior
public class NetworkManager : MonoBehaviourPunCallbacks {
	// Manager setup
	public static NetworkManager inst;
	void Awake() { inst = this; }

	// Event provider for when the current player joins a room
	public delegate void NoArgumentOrReturnEvent();
	public static NoArgumentOrReturnEvent OnJoinedRoomEvent;

	// Event provider for when a new turn starts
	public static NoArgumentOrReturnEvent OnTurnStartEvent;
	public static NoArgumentOrReturnEvent OnTurnEndEvent;

	public PhotonView PV;

	// The index of the current player in PhotonNetwork.PlayerList
	public int currentPlayerIndex = -1;
	public int currentPlayerID;

	// List of the players who have lost the game so far
	public List<int> playersWhoHaveLost = new List<int>();

	void Start(){
		// Pretend we just joined the room if we are connected to the network when we start
		if(PhotonNetwork.IsConnected)
			NextTurn();
	}

	// When the host joins the room for the first time... start the first turn
	public override void OnJoinedRoom(){
		base.OnJoinedRoom();

		if(currentPlayerIndex <= 0 && PhotonNetwork.IsMasterClient)
			NextTurn();
	}

	// Starting the next turn takes two steps
	// 1) The host has to figure out whoes turn it is (this function)
	// 2) Once the host has figured it out that information must be synced with the rest of the players
	public void NextTurn(){ PV.RPC("RPC_NextTurn", RpcTarget.AllBuffered); }
	[PunRPC] void RPC_NextTurn(){
		// End the current turn;
		OnTurnEndEvent();

		// Increment the player index (ensuring it stays within bounds)
		if(PhotonNetwork.IsMasterClient){
			do {
				currentPlayerIndex = (currentPlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
			// This while loop ensures that it will never be the turn of any player who has lost the game
			} while(playersWhoHaveLost.Contains(PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber));
			UpdatePlayerIndex(); // Ensure that everyone is on the same page as the host about who's turn it is
		}
	}

	// Starting the next turn takes two steps
	// 1) The host has to figure out whoes turn it is
	// 2) Once the host has figured it out that information must be synced with the rest of the players (this function)
	void UpdatePlayerIndex(){ PV.RPC("RPC_UpdatePlayerIndex", RpcTarget.AllBuffered, currentPlayerIndex); }
	[PunRPC] void RPC_UpdatePlayerIndex(int _currentPlayerIndex){
		// Syncronize player index
		currentPlayerIndex = _currentPlayerIndex;
		currentPlayerID = PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber;

		// Start the next turn
		Debug.Log("Starting Next Turn");
		OnTurnStartEvent();

		if(isMyTurn())
			StartCoroutine(ShowTurnMessageForSeconds(3));
	}

	public override void OnPlayerLeftRoom(Player otherPlayer){
		// Remove the player who left from the list of players who have lost the game
		playersWhoHaveLost.Remove(otherPlayer.ActorNumber);

		// Don't bother unless we are the host;
		if(!PhotonNetwork.IsMasterClient) return;

		// If the current player is the one who left... start the next turn
		if(otherPlayer.ActorNumber == currentPlayerID){
			currentPlayerIndex--; // We have to decrement the current player index so we don't skip a player's turn
			NextTurn();
		}
	}

	public void CurrentPlayerLost(){
		playersWhoHaveLost.Add(currentPlayerID);
		// Display a message to the player reminding them that they have lost
		ShowLostMessage();

		if(playersWhoHaveLost.Count >= PhotonNetwork.PlayerList.Length){
			// The game is over
			Debug.Log("The game is over!");

			// Load the MenuScene again
			EndGame();
		}
	}

	void EndGame(){ PV.RPC("RPC_EndGame", RpcTarget.AllBuffered); }
	[PunRPC] void RPC_EndGame(){ StartCoroutine(LoadMenuAfterSeconds(5)); }

	public IEnumerator LoadMenuAfterSeconds(int seconds){
		// Wait 5 seconds
		yield return new WaitForSeconds(seconds);

		if(PhotonNetwork.IsConnected){
			Debug.Log("Disconnecting...");

			// Destroy all object and buffered events from this player!
			PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

			// Disconnect from the multiplayer server
			PhotonNetwork.Disconnect();
			// Wait and make sure that we have disconnected
			// while (PhotonNetwork.IsConnected){
			// 	Debug.Log("Still connected!");
	    	// 	yield return null;
			// }

			Debug.Log("Disconnected from Photon!");
		}

		// Load the menu scene
		SceneManager.LoadScene(0);
	}

	IEnumerator ShowTurnMessageForSeconds(int seconds){
		GameObject yourTurn = XRRigSpawner.spawnedRig.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
		yourTurn.SetActive(true);

		yield return new WaitForSeconds(seconds);

		yourTurn.SetActive(false);
	}

	void ShowLostMessage(){
		GameObject youLost = XRRigSpawner.spawnedRig.transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
		youLost.SetActive(true);
	}

	// Function which returns true if it is this player's turn
	public bool isMyTurn(){
		return currentPlayerID == PhotonNetwork.LocalPlayer.ActorNumber;
	}

}
