using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

// Class which manages the positioning and rotation of a block. It uses two controllers, one for positioning and one for orientation.
public class PlaceAndRotateInteractable : XRBaseInteractable {
	[Serializable]
	public struct ControllerInteractorPair{
		public ActionBasedController controller;
		public XRRayInteractor interactor;
	}
	// List of controllers that are currently in the scene
	public static ControllerInteractorPair[] controllersInScene;

	// The plane used to place the block
	public GameObject placementPlane;
	// The block which is manipulated
	[SerializeField]
	private GameObject _managedChild;
	public GameObject managedChild {
		get => _managedChild;
		set {
			_managedChild = value;

			// Unregister this with the interaction manager
			enabled = false;
			colliders.Clear();

			// Make sure all of the child cubes are accepting raycasts and are considered a part of this object's collision (but only if it is my turn)
			if(managedChild != null && NetworkManager.inst.isMyTurn()){
				for(int i = 0; i < managedChild.transform.childCount; i++){
					BoxCollider child = managedChild.transform.GetChild(i).gameObject.GetComponent<BoxCollider>();
					child.gameObject.layer = 0;
					colliders.Add(child);
				}
			// If it isn't my turn... mark the newly spawned object as raycast ignored
			} else if(managedChild != null && !NetworkManager.inst.isMyTurn())
				for(int i = 0; i < managedChild.transform.childCount; i++)
					managedChild.transform.GetChild(i).gameObject.layer = 2;

			// Reregister this with the interaction manager (makes sur)
			enabled = true;
		}
	}

	// Indices of the controllers used for movement and rotation
	public int interactorIndex = -1, rotatorIndex = -1;
	// The initial forward vector of the rotation controller
	Vector3 controllerInitialForward;

	void Start(){
		placementPlane = transform.GetChild(0).gameObject;
		placementPlane.SetActive(false);
	}


	// Called whenever an interactor begins interacting with us
	protected override void OnSelectEntered(SelectEnterEventArgs args) {
		base.OnSelectEntered(args);

		// Don't bother with this function if there is no managed child;
		if(managedChild == null) return;
		// Don't both with this function if we already have an interactor
		if(interactorIndex > 0) return;

		// Update the list of controllers shared between all instances of this class
		UpdateControllerList();

		// Ignore any interactors that aren't/inherited from XRRayInteractor
		XRRayInteractor rayInt = args.interactor as XRRayInteractor;
		if(rayInt == null) {
			Debug.Log(args.interactor.name + " is not a ray interactor!");
			return;
		}

		// Figure out the index of the interactor
		for(int i = 0; i < controllersInScene.Length; i++)
			if(controllersInScene[i].interactor == rayInt){
				SetupInteractor(i);
				break;
			}
	}

	void Update(){
		// Don't bother with this function if there is no managed child;
		if(managedChild == null) return;
		// Don't bother with the update function if we don't have an interactor
		if(interactorIndex < 0) return;

		// If the player is pointing at the placement plane with the interactor controller...
		if(controllersInScene[interactorIndex].interactor.TryGetCurrent3DRaycastHit(out RaycastHit raycastHit) && raycastHit.transform == placementPlane.transform)
			// Move the origin of the stored object to where they are interacting
			managedChild.transform.position = raycastHit.point;

		// Look for another controller with the select button pressed... it will act as our rotator
		if(rotatorIndex < 0)
			// Figure out the index of the rotator (making sure to ignore the interactor)
			for(int i = 0; i < controllersInScene.Length; i++){
				if(i == interactorIndex) continue;

				if(controllersInScene[i].controller.selectAction.action.phase == InputActionPhase.Started){
					SetupRotator(i);
					break;
				}
			}

		// If we didn't find a rotation controller don't bother with the rest of this function
		if(rotatorIndex < 0) return;

		managedChild.transform.rotation = controllersInScene[rotatorIndex].controller.transform.rotation * Quaternion.FromToRotation(controllerInitialForward, Vector3.up);
	}

	// Make sure the list of controllers is up to date
	void UpdateControllerList(){
		// Get the list of controllers in the scene
		ActionBasedController[] controllers = GameObject.FindObjectsOfType<ActionBasedController>();

		// From that list of controllers calculate the list of interactors and create pairs
		controllersInScene = new ControllerInteractorPair[controllers.Length];
		for(int i = 0; i < controllers.Length; i++){
			controllersInScene[i].controller = controllers[i];
			controllersInScene[i].interactor = controllers[i].gameObject.GetComponent<XRRayInteractor>();
		}
	}

	// Code that is called up when an interactor is added
	void SetupInteractor(int index){
		// Set the interactor index
		interactorIndex = index;

		// Add the unsetup as a callback listening to state changes
		controllersInScene[index].controller.selectAction.action.canceled += UnSetupInteractor;
		controllersInScene[index].controller.selectAction.action.performed += UnSetupInteractor;

		placementPlane.SetActive(true);
		for(int i = 0; i < managedChild.transform.childCount; i++)
			managedChild.transform.GetChild(i).gameObject.layer = 2; // Mark all of the children as raycast ignored
	}

	// Code that is called automatically when the button keeping the interactor around is depressed
	void UnSetupInteractor(InputAction.CallbackContext _){
		// Remove this function as a callback
		controllersInScene[interactorIndex].controller.selectAction.action.canceled -= UnSetupInteractor;
		controllersInScene[interactorIndex].controller.selectAction.action.performed -= UnSetupInteractor;

		// Set the interactor index to null
		interactorIndex = -1;
		// If the rotator index is set... nullify it as well
		if(rotatorIndex >= 0) UnSetupRotator(new InputAction.CallbackContext());

		placementPlane.SetActive(false);
		for(int i = 0; i < managedChild.transform.childCount; i++)
			managedChild.transform.GetChild(i).gameObject.layer = 0; // Mark all of the children as raycast accpeted
	}

	// Code that is called when a rotator is added
	void SetupRotator(int index){
		// Set the rotator index
		rotatorIndex = index;

		// Add the unsetup as a callback listening to state changes
		controllersInScene[index].controller.selectAction.action.canceled += UnSetupRotator;
		controllersInScene[index].controller.selectAction.action.performed += UnSetupRotator;

		// TODO: Add a setting so that the player can adjust the angle of the controler in relation to their hand
		// Save the forward vector of the controller
		controllerInitialForward = controllersInScene[index].controller.transform.forward;
	}

	// Code that is called automatically when the button keeping the rotator around is depressed
	void UnSetupRotator(InputAction.CallbackContext _){
		// Remove this function as a callback
		controllersInScene[rotatorIndex].controller.selectAction.action.canceled -= UnSetupRotator;
		controllersInScene[rotatorIndex].controller.selectAction.action.performed -= UnSetupRotator;

		// Set the rotator index to null
		rotatorIndex = -1;
	}
}
