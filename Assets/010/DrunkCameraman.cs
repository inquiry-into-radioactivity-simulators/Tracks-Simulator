using UnityEngine;
using System.Collections;

public class DrunkCameraman : MonoBehaviour {
	public float speed = 3.0f;
	public float movement = 1.0f;
	public float rotation = 1.0f;

	void Update () {
		Camera.main.transform.localPosition = SmoothRandom.GetVector3(speed)*movement;
		Camera.main.transform.LookAt(Camera.main.transform.parent.position + Camera.main.transform.parent.forward*100 + SmoothRandom.GetVector3(speed)*rotation);
	}
}