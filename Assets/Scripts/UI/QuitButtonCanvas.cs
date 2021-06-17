using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

// Small script which provides support for the quit button
public class QuitButtonCanvas : MonoBehaviour {

	public void OnButtonPressed() {
		Debug.Log("Disconnecting");
		StartCoroutine(NetworkManager.inst.LoadMenuAfterSeconds(1));
	}


}
