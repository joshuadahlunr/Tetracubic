using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// Spawns blocks which are synchronized over the network
public class BlockPlacer : MonoBehaviour {
	// The PhotonView used for network synchronization
	public PhotonView PV;
	// The block placer
	public PlaceAndRotateInteractable placeAndRotateInteractable;

	// List of paths to block prefabs
	public string[] prefabPaths;

	public bool isPlacing = false;
	// The ammount of time (in seconds) it takes for the placer to go from the top to the bottom of the region
	public float timeToDrop = 20;

	void Awake() {
		// Make sure that all of the prefabs have their names properly stripped of path components that will cause them to incorrectly load
		Utilities.PreparePrefabPaths(ref prefabPaths);
	}

	// Ensure that we are listening to room join events
	void OnEnable(){
		// NetworkManager.OnJoinedRoomEvent += OnRoomJoin;
		NetworkManager.OnTurnStartEvent += StartPlacing;
		NetworkManager.OnTurnEndEvent += OnTurnEnd;
	}
	void OnDisable(){
		// NetworkManager.OnJoinedRoomEvent -= OnRoomJoin;
		NetworkManager.OnTurnStartEvent -= StartPlacing;
		NetworkManager.OnTurnEndEvent -= OnTurnEnd;
	}

	void OnTurnEnd(){
		isPlacing = false;
	}

	public void StartPlacing(){ PV.RPC("RPC_StartPlacing", RpcTarget.AllBuffered); }
	[PunRPC] void RPC_StartPlacing(){
		isPlacing = true; // This needs to happen for everyone in case host migration occures
		transform.position = new Vector3(0, 2, 0); // This needs to happen for everyone so that the player whoes turn it is has their placer at the top of the world

		// If it is our turn... spawn a block
		if(NetworkManager.currentPlayerID == PhotonNetwork.LocalPlayer.ActorNumber)
			SpawnBlock(Random.Range(0, prefabPaths.Length));
	}

	void Update(){
		// Don't bother if placement hasn't started yet
		if(!isPlacing) return;

		Vector3 pos = transform.position;
		pos.y -= 2f/timeToDrop /* distance/time */ * Time.deltaTime;
		transform.position = pos;

		// Don't bother if we aren't the host
		if(!PhotonNetwork.IsMasterClient) return;

	}

	// Function which spawns a randomly-colored random block and synchronizes it over the network
	public void SpawnBlock(int blockIndex){ PV.RPC("RPC_SpawnBlock", RpcTarget.AllBuffered, blockIndex, PhotonNetwork.LocalPlayer); }
	[PunRPC] void RPC_SpawnBlock(int blockIndex, Player originatingPlayer){
		if(PhotonNetwork.IsMasterClient){
			placeAndRotateInteractable.managedChild = PhotonNetwork.InstantiateRoomObject(prefabPaths[blockIndex], transform.position, transform.rotation, 0); // RoomObject so its lifetime isn't attached to a particular player
			// Makes sure that the player whoes turn it is has ownership of the new object
			placeAndRotateInteractable.managedChild.GetComponent<PhotonView>().TransferOwnership(originatingPlayer);

			// Setup the poly cube (values should be automatically network synced by PolyCube so ensure only the owner does this)
			PolyCube settings = placeAndRotateInteractable.managedChild.GetComponent<PolyCube>();
			settings.color = Random.ColorHSV(0, 1, .9f, 1, 1, 1, 1, 1);
			settings.isWireframe = true;

			// Make sure it is properly parented
			settings.setChildOfPlacer();
		}
	}
}
