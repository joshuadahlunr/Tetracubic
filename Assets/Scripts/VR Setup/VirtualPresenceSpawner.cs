using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Realtime;

public class VirtualPresenceSpawner : MonoBehaviour {
	[Header("Settings")]
	[Tooltip("Proton View used to synconize this device's state over the network.")]
	public PhotonView PV;

	[Tooltip("The device characteristics used to identify the device we should manage.")]
	public InputDeviceCharacteristics characteristics;
	[Tooltip("Array of paths to possible controllers to spawn (The name of the prefabs in this list must match the name OpenXR gives us for the device).")]
	public string[] possibleDevicePrefabPaths;
	[Tooltip("The index of the default controller to spawn in the above array (most likely this should be 0).")]
	public int defaultIndex = 0;

	[HideInInspector]
	public GameObject spawnedInstance = null;

	void OnEnable(){
		// Register that we should look for new devices when this object is enabled
		InputDevices.deviceConnected += DeviceConnected;
		// Get a list of the currently connected devices
		List<InputDevice> devices = new List<InputDevice>();
		InputDevices.GetDevices(devices);
		// Spawn the appropriate controllers for the connected devices
        foreach(var device in devices)
            DeviceConnected(device);
    }

    void OnDisable(){
		// Stop looking for new devices as long as this object is disabled
        InputDevices.deviceConnected -= DeviceConnected;

		// TODO: make sure that this destroys objects when a player leaves
		// If a model still exists destroy it
		if(spawnedInstance != null)
			NetworkedDestroyChild();
    }

    public void DeviceConnected(InputDevice device){
		// Don't bother with this function if the object doesn't belong to this player
		if(!PV.IsMine) return;

        // Check if the device matches the requested characteristics
        if ((device.characteristics & characteristics) != 0){
			Debug.Log("Found controller '" + device.name + "' with " + characteristics);

			// Loop through the noted possible models and spawn the one with a matching name
			foreach(var prefabPath in possibleDevicePrefabPaths){
				GameObject prefab = Resources.Load(prefabPath) as GameObject;
				if(prefab.name == device.name){
					// If a model already exists... destroy it!
					if(spawnedInstance != null)
						NetworkedDestroyChild();
					// Then spawn the new model
					NetworkedSpawn(prefabPath);
					break;
				}
			}

			// If we couldn't find a matching device spawn the default model
			if(spawnedInstance == null){
				Debug.Log("Controller model '" + device.name + "' not found... using default");
				NetworkedSpawn(possibleDevicePrefabPaths[defaultIndex]);
			}
        }
    }

	// This function is used by the spawner to parent this object to the correct hand
	public void setParent(string parentName){ PV.RPC("RPC_setParent", RpcTarget.AllBuffered, parentName); }
	[PunRPC] protected void RPC_setParent(string parentName){
		GameObject parent = GameObject.Find(parentName);
		transform.SetParent(parent.transform);
	}

	// This function spawns a virtual presence object over the network
	void NetworkedSpawn(string prefabPath){ PV.RPC("RPC_NetworkedSpawn", RpcTarget.AllBuffered, prefabPath); }
	[PunRPC] protected void RPC_NetworkedSpawn(string prefabPath) {
		spawnedInstance = Instantiate(Resources.Load(prefabPath) as GameObject, transform.position, transform.rotation, transform);
	}

	// This function destroys one of this object's children over the network
	void NetworkedDestroyChild(int childIndex = 0){ PV.RPC("RPC_NetworkedDestroy", RpcTarget.AllBuffered, childIndex); }
	[PunRPC] protected void RPC_NetworkedDestroy(int childIndex) {
		Destroy(transform.GetChild(childIndex).gameObject);
	}
}
