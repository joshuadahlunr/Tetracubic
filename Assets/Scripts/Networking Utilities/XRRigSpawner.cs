using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

// This component ensures a smooth transition from the rig used in the menus to the rig used in multiplayer games
public class XRRigSpawner : MonoBehaviour {
	// Register this object as a listener for OnJoinRoom events
	void OnEnable() { NetworkManager.OnJoinedRoomEvent += OnJoinRoom; }
	void OnDisable() { NetworkManager.OnJoinedRoomEvent -= OnJoinRoom; }

	// Reference to the rig that was spawned
	public static GameObject spawnedRig;

	void Start(){
		spawnedRig = null;

		// Pretend we just joined the room if we are connected to the network when we start
		if(PhotonNetwork.IsConnected)
			OnJoinRoom();

	}

	void OnJoinRoom(){
		// Disable the rig used in the menus
		GameObject menuRig = GameObject.Find("XR Rig");
		if(menuRig) menuRig.SetActive(false);

		// Spawn the local rig connected to the network
		spawnedRig = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "XR Rig"), menuRig.transform.position, menuRig.transform.rotation, 0);
	}
}
