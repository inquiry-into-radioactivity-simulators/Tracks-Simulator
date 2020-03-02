using UnityEngine;
using System.Collections;

public class GeigerCounter : MonoBehaviour {

	public ParticleSystem emitter;
	public AudioSource audio1;
	public static GeigerCounter i;
	
	void Start () {
		i = this;
	}
	
	public static void Beep() {
		i.Beep1();
	}
	
	public void Beep1 () {
		emitter.Emit(1);
		audio1.Play();
	}
}
