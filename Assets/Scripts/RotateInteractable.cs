using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RotateInteractable : DualControllerInteractable {
	public LineRenderer raycastLine, controllerLine;

	protected override void UpdateAfterRaycastSuccess() {
		// Draw a line between where the interactors hit this object
		raycastLine.SetPosition(0, raycastHits[0].point);
		raycastLine.SetPosition(1, raycastHits[1].point);

		// Draw a line between the player's controllers
		controllerLine.SetPosition(0, interactors[0].transform.position);
		controllerLine.SetPosition(1, interactors[1].transform.position);
	}

	protected override void UpdateAfterNoInteraction() {
		// Just hide the line away
		raycastLine.SetPosition(0, Vector3.zero);
		raycastLine.SetPosition(1, Vector3.zero);

		controllerLine.SetPosition(0, Vector3.zero);
		controllerLine.SetPosition(1, Vector3.zero);
	}
}
