using UnityEngine;
using System.Collections;

public class FPS : GUIStuff {

	public int count = 20;
	float min = 0f;
	float max = 0f;
	float counter = 0;
	public float[] fpses;
	public void Start () {
		base.Start();
		fpses = new float [count];
		for(int i = 0; i < fpses.Length; i++) {
			fpses[i]=0;
		}
	}
	
	// Use this for initialization
	void OnGUI () {
		base.OnGUI();
		float margin = 5;
		float x = 200;
		float y = 70;
		GUI.BeginGroup(new Rect(margin,margin,x,y), "", block);
			if(max != 0) {
				GUI.Box(new Rect(margin,margin,x-margin*2,y-margin*2), "", hole);
				GUI.Box(new Rect(margin*4,margin*3.5f,x-margin*2,y-margin*2), "fps: "+ min.ToString("00") + " - " + max.ToString("00"), font);
			}
		GUI.EndGroup();
		
	}
	
	// Update is called once per frame
	void Update () {
		counter += 1;
		if(counter >= count) {
			min = 9000;
			max = 0;
		}
		for(int i = fpses.Length-2; i >= 0 ; i--) {
			fpses[i+1] = fpses[i];
			if(counter >= count) {
				if(fpses[i+1] < min) min = fpses[i+1];
				if(fpses[i+1] > max) max = fpses[i+1];
			}
		}
		if(counter >= count) {
			counter = 0;
		}
		fpses[0]=(1f/Time.deltaTime)*Time.timeScale;
	}
}
