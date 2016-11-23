using UnityEngine;
using System.Collections;

public class testElevation : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
        Renderer rend = gameObject.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Custom/Elevation"));
        rend.material = mat;

	}
	


	// Update is called once per frame
	void Update () {
	
	}
    
}
