using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// Spawns blocks which are syncronized over the network
public class BlockSpawner : MonoBehaviour {
	// The PhotonView used for network synchronization
	public PhotonView PV;
	// The block placer
	public PlaceAndRotateInteractable placeAndRotateInteractable;

	// List of paths to block prefabs
	public string[] prefabPaths;

	void Awake() {
		// Make sure that all of the prefabs have their names properly stripped of path components that will cause them to incorrectly load
		Utilities.PreparePrefabPaths(ref prefabPaths);
	}

	// Ensure that we are listening to room join events
	void OnEnable(){ NetworkManager.OnJoinedRoomEvent += OnRoomJoin; }
	void OnDisable(){ NetworkManager.OnJoinedRoomEvent -= OnRoomJoin; }

	void OnRoomJoin(){
		// TODO: this behavior should be removed in favor of some sort of manager which keeps track of who's turn it is
		SpawnBlock(Random.Range(0, prefabPaths.Length)); // When we join the room spawn a block
	}

	// Function which spawns a randomly-colored random block and synchronizes it over the network
	public void SpawnBlock(int blockIndex){ PV.RPC("RPC_SpawnBlock", RpcTarget.AllBuffered, blockIndex); }
	[PunRPC] void RPC_SpawnBlock(int blockIndex){
		if(PhotonNetwork.IsMasterClient){
			placeAndRotateInteractable.managedChild = PhotonNetwork.InstantiateRoomObject(prefabPaths[blockIndex], transform.position, transform.rotation, 0);

			// Setup the poly cube (values should be automatically network synced by PolyCube so ensure only the owner does this)
			PolyCube settings = placeAndRotateInteractable.managedChild.GetComponent<PolyCube>();
			settings.color = Random.ColorHSV(0, 1, .9f, 1, 1, 1, 1, 1);
			settings.isWireframe = true;

			// Make sure it is properly parented
			settings.setChildOfPlacer();
		}
	}
}
