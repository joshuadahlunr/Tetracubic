using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RotateInteractable : DualControllerInteractable {
	public LineRenderer raycastLine, controllerLine;

	Plane? initialPlane = null;
	Vector3 rotationAxis;
	Quaternion initialQuat;

	public GameObject[] planes;

	void Start(){
		initialQuat = transform.rotation;
	}

	protected override void UpdateAfterRaycastSuccess() {
		// Draw a line between where the interactors hit this object
		raycastLine.SetPosition(0, raycastHits[0].point);
		raycastLine.SetPosition(1, raycastHits[1].point);

		// Draw a line between the player's controllers
		controllerLine.SetPosition(0, interactors[0].transform.position);
		controllerLine.SetPosition(1, interactors[1].transform.position);

		calculateRotation();

		if(controllers[0].activateAction.action.phase == InputActionPhase.Started || controllers[1].activateAction.action.phase == InputActionPhase.Started){
			removeInteractor(0);
			removeInteractor(1);

			transform.rotation = initialQuat;
		}
	}

	protected override void UpdateAfterNoInteraction() {
		// Just hide the line away
		raycastLine.SetPosition(0, Vector3.zero);
		raycastLine.SetPosition(1, Vector3.zero);

		controllerLine.SetPosition(0, Vector3.zero);
		controllerLine.SetPosition(1, Vector3.zero);

		// Set the initial plane to null;
		initialPlane = null;

		// Hide all of the axis alignment planes
		foreach(var plane in planes)
			plane.SetActive(false);
	}

	Plane calculateCurrentPlane(){
		Vector3 initialA = interactors[0].transform.position;
		Vector3 initialB = interactors[1].transform.position;

		// The last point of the plane is defined in terms of the camera's forward vector
		//Vector3 initialC = initialA + Camera.main.transform.forward;
		Vector3 initialC = initialA + rotationAxis;


		return new Plane(initialA, initialB, initialC);
	}

	void calculateRotation(){
		if(initialPlane == null){
			int closestAxisPlane = findClosestAxisPlane();
			Debug.Log("Closest Plane: " + closestAxisPlane);
			foreach(var plane in planes)
				plane.SetActive(false);
			planes[closestAxisPlane].SetActive(true);

			switch(closestAxisPlane){
				case 0: rotationAxis = transform.up; break;
				case 1: rotationAxis = transform.forward; break;
				case 2: rotationAxis = transform.right; break;
			}

			initialPlane = calculateCurrentPlane();
			initialQuat = transform.rotation;
		}

		Plane currentPlane = calculateCurrentPlane();

		transform.rotation = initialQuat * Quaternion.FromToRotation(initialPlane.Value.normal, currentPlane.normal);
	}


	int findClosestAxisPlane(){
		Plane[] planes = new Plane[] {
			new Plane(transform.position, transform.position + transform.forward, transform.position + transform.right), // up Plane
			new Plane(transform.position, transform.position + transform.right, transform.position + transform.up), // forward Plane
			new Plane(transform.position, transform.position + transform.up, transform.position + transform.forward), // right Plane
			// planeFromMeshFilter(this.planes[0].GetComponent<MeshFilter>()),
			// planeFromMeshFilter(this.planes[1].GetComponent<MeshFilter>()),
			// planeFromMeshFilter(this.planes[2].GetComponent<MeshFilter>()),
		};

		float[] distances = new float[]{
			planes[0].GetDistanceToPoint(raycastHits[0].point),
			planes[1].GetDistanceToPoint(raycastHits[0].point),
			planes[2].GetDistanceToPoint(raycastHits[0].point),
		};

		float min = Mathf.Infinity;
		int index = 0;
		for(int i = 0; i < distances.Length; i++)
			if(distances[i] < min){
				min = distances[i];
				index = i;
			}

		return index;
	}

	Plane planeFromMeshFilter(MeshFilter filter){
		Vector3 normal = Vector3.zero;

		if(filter && filter.mesh.normals.Length > 0)
		    normal = filter.transform.TransformDirection(filter.mesh.normals[0]);

		return new Plane(normal, transform.position);
	}
}
