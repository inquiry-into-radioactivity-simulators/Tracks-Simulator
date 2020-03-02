using UnityEngine;
using System.Collections;

public class Cleaner : MonoBehaviour {

	Transform t;
	Transform c;

	void Start () {
		t = transform;
		c = Camera.main.transform;
		StartCoroutine(Clean());
	}
	
	IEnumerator Clean () {
		while(true) {
			if((t.position - c.position).sqrMagnitude > 0.03f*0.03f) {
				Destroy(gameObject);
			}
			yield return new WaitForSeconds(2);
		}
	}
}
