using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// This component will delete an object if it should only exist locally
[DisallowMultipleComponent]
public class RemoteObjectDeleter : MonoBehaviour {
	[Tooltip("The PhotonView of the parent used for interfacing with the network.")]
	public PhotonView PV; // PhotonView of the object

	[Tooltip("Weather the object should be destroyed or disabled.")]
	public bool shouldDestroy = true; // Weather the object should be destroyed or disabled

	void Awake(){
		// If the photon view isn't locally owned destroy the attached object
		if(!PV.IsMine){
			if(shouldDestroy) Destroy(gameObject);
			else gameObject.SetActive(false);
		}
	}
}
