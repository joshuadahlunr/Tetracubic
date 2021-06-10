using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VirtualPresenceSpawner : MonoBehaviour {
	[Header("Settings")]
	[Tooltip("The device characteristics used to identify the device we should manage.")]
	public InputDeviceCharacteristics characteristics;
	[Tooltip("Array of possible controllers to spawn (The name of the prefabs in this list must match the name OpenXR gives us for the device).")]
	public GameObject[] possibleDevicePrefabs;
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
    }

    void DeviceConnected(InputDevice device){
        // Check if the device matches the requested characteristics
        if ((device.characteristics & characteristics) != 0){
			Debug.Log("Found controller '" + device.name + "' with " + characteristics);

			// Loop through the noted possible models and spawn the one with a matching name
			foreach(var prefab in possibleDevicePrefabs)
				if(prefab.name == device.name){
					// If a model already exists... destroy it!
					if(spawnedInstance != null) Destroy(spawnedInstance);
					spawnedInstance = Instantiate(prefab, transform.position, transform.rotation, transform);
					break;
				}

			// If we couldn't find a matching device spawn the default model
			if(spawnedInstance == null){
				Debug.Log("Controller model '" + device.name + "' not found... using default");
				spawnedInstance = Instantiate(possibleDevicePrefabs[defaultIndex], transform.position, transform.rotation, transform);
			}

        }
    }
}
