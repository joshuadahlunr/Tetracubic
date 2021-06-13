using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// This component will disable other components that are only supposed to be enabled locally
[DisallowMultipleComponent]
public class RemoteComponentDisabler : MonoBehaviour {
	[Tooltip("The PhotonView of the parent used for interfacing with the network.")]
	public PhotonView PV; // PhotonView of the object

	[Tooltip("The list of components that should be disabled.")]
	public MonoBehaviour[] componentsToDisable; // The list of components that should be disabled.

	void Awake(){
		// If the photon view isn't locally owned disable all of the components in question
		if(!PV.IsMine)
			foreach(MonoBehaviour component in componentsToDisable)
				component.enabled = false;
	}
}
