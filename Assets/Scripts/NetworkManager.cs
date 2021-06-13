using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script which provides the (currently barebones) networking behavior
public class NetworkManager : MonoBehaviour {
	// Event provider for when the current player joins a room
	public delegate void OnJoinedRoomEventType();
	public static OnJoinedRoomEventType OnJoinedRoomEvent;

}
