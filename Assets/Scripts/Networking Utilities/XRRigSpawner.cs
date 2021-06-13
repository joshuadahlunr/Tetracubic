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

	void OnJoinRoom(){
		// Disable the rig used in the menus
		GameObject menuRig = GameObject.Find("XR Rig");
		if(menuRig) menuRig.SetActive(false);

		// Spawn the local rig connected to the network
		PhotonNetwork.Instantiate(Path.Combine("Prefabs", "XR Rig"), menuRig.transform.position, menuRig.transform.rotation, 0);
	}
}
