using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

	public Vector3 velocity;
	
	public Vector3 refPos;
	
	public Vector3 delta = Vector3.zero;
	
	void Start() {
		refPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		delta += velocity * 1000 * Time.deltaTime;
		transform.position = refPos + delta*0.001f;
	}
}
