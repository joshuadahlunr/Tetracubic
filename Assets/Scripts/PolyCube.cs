using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PolyCube : MonoBehaviour {
	// Proton View for this cube
	public PhotonView PV;
	// Rigid body for this cube
	public Rigidbody rigidbody;

	// The color of this cube
	[SerializeField]
	private Color _color;
	public Color color {
		get => _color;
		set {
			_color = value;
			updateChildren(_color, _isWireframe);
		}
	}

	// Weather or not this cube should be wireframe rendered
	[SerializeField]
	private bool _isWireframe;
	public bool isWireframe {
		get => _isWireframe;
		set {
			_isWireframe = value;
			updateChildren(_color, _isWireframe);
		}
	}

	// Materials used for wireframe and non-wireframe rendering
	public Material wireframeMaterial;
	public Material backgroundMaterial;

    // Start is called before the first frame update
    void Start(){
		updateChildren(_color, _isWireframe); // Make sure all of the children have had the new color settings applied
    }

	// Whenever the object collides with another block, it is detached from the placer and the physics engine takes over
	void OnCollisionEnter(Collision collision){
		// Collisions with the placement plane are to be expected (ignore them)
		if(collision.transform.name == "PlacementPlane") return;

		Debug.Log(transform.name + " Has detached from its parent!");

		detachFromParent();
	}

	// Function which ensures that the color settings for this cube are synced over the network
	void updateChildren(Color color, bool isWireframe) { PV.RPC("RPC_updateChildren", RpcTarget.AllBuffered, color.r, color.g, color.b, color.a, isWireframe); }
	[PunRPC] void RPC_updateChildren(float r, float g, float b, float a, bool isWireframe){
		// Make sure the local variables are synced with what came over the network
		_color = new Color(r, g, b, a);
		_isWireframe = isWireframe;

		// Determine the material to use
		Material mat = isWireframe ? new Material(wireframeMaterial) : new Material(backgroundMaterial);
		// Set the wire color
		mat.SetColor("_WireColor", color);

		// Make the background color a little darker than wire color
		Color.RGBToHSV(color, out float H, out float S, out float V);
		S += .15f; // Color theory says for a darker color to look good you must bump the saturation
		V -= .3f;
		Color darkerColor = Color.HSVToRGB(H, S, V);
		darkerColor.a = color.a; // Make sure alpha is synced
		mat.SetColor("_BaseColor", darkerColor);

		// Apply the new material to all of the children
		var children = GetComponentsInChildren<MeshRenderer>();
		if(children != null)
			foreach(MeshRenderer child in children)
				child.material = mat;
	}

	// Function which ensures that the block is properly childed to the object placer all across the network
	public void setChildOfPlacer(){ PV.RPC("RPC_setChildOfPlacer", RpcTarget.AllBuffered); }
	[PunRPC] void RPC_setChildOfPlacer(){
		PlaceAndRotateInteractable placer = GameObject.Find("CubeInteractor").GetComponent<PlaceAndRotateInteractable>();

		transform.localScale = new Vector3(.1f, .1f, .1f);
		transform.parent = placer.transform;
		placer.managedChild = gameObject; // TODO: this should only happen for the player whose turn it is
	}

	// Function which detaches the block from the object placer all accross the network and then turns over control to the rigidbody.
	public void detachFromParent(){ PV.RPC("RPC_detachFromParent", RpcTarget.AllBuffered); }
	[PunRPC] void RPC_detachFromParent(){
		transform.parent.gameObject.GetComponent<PlaceAndRotateInteractable>().managedChild = null;
		transform.parent = null;

		// Mark all of the children as raycast ignored
		for(int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.layer = 2;

		// On the owning object... make sure control is switched over to the rigidbody
		if(PV.IsMine){
			// Ensure that the rigidbody is enabled once we detach from the parent
			rigidbody.constraints = RigidbodyConstraints.None; // Make sure that all of the movement constraints are removed
			rigidbody.WakeUp();

			// Switch from wireframe to solid rendered
			isWireframe = false;
		}
	}
}
