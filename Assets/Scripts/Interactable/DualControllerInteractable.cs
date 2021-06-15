using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class DualControllerInteractable : XRBaseInteractable {
	// Constants used to alias the following arrays
	static readonly int A = 0;
	static readonly int B = 1;

	// Interactors currently interacting with this object
	protected XRRayInteractor[] interactors = new XRRayInteractor[2]{ null, null };
	// Controllers associated with the interactors
	protected ActionBasedController[] controllers = new ActionBasedController[2];
	// Raycast information about where the controllers are currently interacting with this object
	protected RaycastHit[] raycastHits = new RaycastHit[2];

	[Tooltip("This property determines if a controller should stop being tracked by this object if its interaction ray stops touching this object.")]
	public bool disableInteractionsOnControllerLeave = true;

	// Called whenever an interactor begins interacting with us
	protected override void OnSelectEntered(SelectEnterEventArgs args) {
		base.OnSelectEntered(args);

		// Ignore any interactors that aren't/inherited from XRRayInteractor
		XRRayInteractor rayInt = args.interactor as XRRayInteractor;
		if(rayInt == null) {
			Debug.Log(args.interactor.name + " is not a ray interactor!");
			return;
		}

		// Assign the interactor as A...
		if(interactors[A] == null){
			interactors[A] = rayInt;
			controllers[A] = rayInt.gameObject.GetComponent<ActionBasedController>();
		// unless A is already assigned, then assign it as B
		} else if(interactors[B] == null) {
			interactors[B] = rayInt;
			controllers[B] = rayInt.gameObject.GetComponent<ActionBasedController>();
		}

		// If more than one interactor tries to interact with us... ignore it
	}

	// NOTE: If a child derives from update they must call base.Update() in order for this behavior to take effect!
	protected virtual void Update(){
		// Don't bother updating if we aren't fully interacting with this object
		if(interactors[A] == null || interactors[B] == null) {
			UpdateAfterNoInteraction();
			return;
		}

		// If the select action is not currently pressed on controller A then remove it
		if(controllers[A] && controllers[A].selectAction.action.phase != InputActionPhase.Started)
			removeInteractor(A);
		// If the select action is not currently pressed on controller B then remove it
		if(controllers[B] && controllers[B].selectAction.action.phase != InputActionPhase.Started)
			removeInteractor(B);

		// Variable tracking if raycast assignment has succeded
		bool succeded = true;
		// Update A's raycast
		if( interactors[A] != null && interactors[A].TryGetCurrent3DRaycastHit(out RaycastHit hitA) ){
			raycastHits[A] = hitA;

			// If A is not interacting with us then remove it (but this behavior only occures if enabled)
			if(!hitA.transform.IsChildOf(transform) && disableInteractionsOnControllerLeave){
				removeInteractor(A);
				succeded = false;
			}
		} else succeded = false;

		// Update B's raycast
		if( interactors[B] != null && interactors[B].TryGetCurrent3DRaycastHit(out RaycastHit hitB) ){
			raycastHits[B] = hitB;

			// If B is not interacting wit us then remove it (but this behavior only occures if enabled)
			if(!hitB.transform.IsChildOf(transform) && disableInteractionsOnControllerLeave){
				removeInteractor(B);
				succeded = false;
		  	}
		} else succeded = false;

		// If the updates didn't succed then no interaction is occuring
		if(!succeded) {
			UpdateAfterNoInteraction();
			return;
		}

		// Tell derived classes that we have succeded our raycasting
		UpdateAfterRaycastSuccess();
	}

	// Removes all of our references to a particular interactor
	protected void removeInteractor(int index){
		interactors[index] = null;
		controllers[index] = null;
	}

	// Called when no interaction is occuring to this object
	protected virtual void UpdateAfterNoInteraction() {}
	// Called when we succesfully update the raycast information
	protected virtual void UpdateAfterRaycastSuccess() {}
}
