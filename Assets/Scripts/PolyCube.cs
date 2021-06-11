using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyCube : MonoBehaviour {
	[SerializeField]
	private Color _color;
	public Color color {
		get => _color;
		set {
			_color = value;
			updateChildren();
		}
	}

	[SerializeField]
	private bool _isWireframe;
	public bool isWireframe {
		get => _isWireframe;
		set {
			_isWireframe = value;
			updateChildren();
		}
	}

	public Material wireframeMaterial;
	public Material backgroundMaterial;

    // Start is called before the first frame update
    void Start(){
		updateChildren(); // Make sure all of the children have had the new color settings applied
    }

	void updateChildren(){
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
}
