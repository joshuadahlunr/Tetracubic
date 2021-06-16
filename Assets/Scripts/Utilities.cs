using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Some extra utilities that don't really belong anywhere in particular
public class Utilities {

	// Function which removes unnecessary parts from the prefab path that can be copied from unity. If these parts are not removed then the resource loader will fail to find the prefab!
	public static void PreparePrefabPaths(ref string[] paths){
		for(int i = 0; i < paths.Length; i++)
			paths[i] = paths[i].Replace("Assets/Resources/", "").Replace(".prefab", "");
	}

}
