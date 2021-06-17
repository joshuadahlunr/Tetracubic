using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
	public GameObject canvasRotator;
	public Slider slider;

	public void OnJoinPressed(){
		SceneManager.LoadScene(1);
	}

	public void OnQuitPressed(){
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void OnSliderValueChanged(){
		canvasRotator.transform.rotation = Quaternion.Euler(0, slider.value, 0);
	}
}
