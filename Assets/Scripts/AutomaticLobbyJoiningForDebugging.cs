using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// Debugging script which causes a player to automatically join a room when the game starts
public class AutomaticLobbyJoiningForDebugging : MonoBehaviourPunCallbacks {
    // Start is called before the first frame update
    void Start() {
		PhotonNetwork.ConnectUsingSettings(); // Connects to the master photon server.
    }

	public override void OnConnectedToMaster(){
		Debug.Log("Player has connected to the " +  PhotonNetwork.CloudRegion + " master server");
		PhotonNetwork.AutomaticallySyncScene = true; // Makes it so that when the host loads a new level it is synchronized

		// Automatically join a room once we connect to Photon
		JoinRoom();
	}

	public void JoinRoom(){
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnJoinRandomFailed(short returnCode, string message){
		Debug.Log("Tried to join a random game but failed. There must be no open games available");
		CreateRoom();
	}

	void CreateRoom(){
		Debug.Log("Trying to create a new room");
		int randomRoomName = Random.Range(0, 10000);
		RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 3 };
		PhotonNetwork.CreateRoom("Room " + randomRoomName, roomOps);
	}

	public override void OnCreateRoomFailed(short returnCode, string message){
		Debug.Log("Tried to a create a new room but failed, there must already be a room with the same name");
		// If room creation failed... create a new room;
		CreateRoom();
	}

	public override void OnJoinedRoom() {
		base.OnJoinedRoom();
		Debug.Log("We are now in a room!");

		// Make sure all of the listener classes are notified that we just joined a room
		NetworkManager.OnJoinedRoomEvent();
	}
}
