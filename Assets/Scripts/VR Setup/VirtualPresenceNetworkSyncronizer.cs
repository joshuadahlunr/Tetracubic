using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

// This component ensures that virtual presence spawners are added to each hand (and network synced)
public class VirtualPresenceNetworkSyncronizer : MonoBehaviour {
	public PhotonView PV;
	public string prefabPath;
	public string parentName;

	// When the object starts make sure the controlers are syncronized over the network
	void Start(){
		if(PhotonNetwork.IsConnected) {
			VirtualPresenceSpawner presence = PhotonNetwork.Instantiate(prefabPath, Vector3.zero, Quaternion.identity).GetComponent<VirtualPresenceSpawner>();
			presence.setParent(parentName);
		} else if(PV.IsMine)
			Instantiate(Resources.Load(prefabPath) as GameObject, Vector3.zero, Quaternion.identity, GameObject.Find(parentName).transform);
	}
}
