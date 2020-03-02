using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MacroModeGUI : GUIStuff {
	
	public float lineWidth = 1.2f;
	public float bracketSize = 5f;
	public Vector2[] readouts;
	public Vector3[] midpoints;
	public MediumInfo[] mediums;
	public int[] modes;
	public Material lineMaterial;
	public Vector2 barSize;
	List<List<Vector3>> lines;
	List<LineRenderer> lineRenderers;
	List<LineRenderer> lineRenderers2;
	List<float> lineLength;

	public static MacroModeGUI instance;
	public static float worldUnitsToCM = 0.227848101f;
	public static Vector3 bounceDir;
	
	public static int LineCount () {
		return instance ? instance.lines.Count : 0;
	}
	
	void Awake () {
		instance = this;
		lineLength = new List<float>();
		lineRenderers = new List<LineRenderer>();
		lineRenderers2 = new List<LineRenderer>();
		lines = new List<List<Vector3>>();
	}
	
	void OnGUI () {
		if(ShooterDriver.firing != 2 || ShooterDriver.useParking) return;
		base.OnGUI();
		base.Scale(0.25f);
		Rect r0 = new Rect(screen.x*0.5f-barSize.x*0.5f, screen.y*0.420f-barSize.y*0.5f, barSize.x, barSize.y);
		float margin1 = r0.height * 0.2f;
		Rect r1 = new Rect(r0.x+margin1, r0.y+margin1, (r0.width-margin1*2)*ProjectileDriver.lifeBar, r0.height-margin1*2);
		GUI.Box(r0 , "", block);
		Color c = GUI.color; GUI.color = Color.green;
		GUI.Box(r1, "", selection1);
		GUI.color = c;
		GUI.Box(r0 , "", hole);
		base.Scale(4);
		if(SelectZoomLevelGUI.selectedButton != 2) return;
		float margin = 10;
		float x = 180;
		float y = 80;
		
		for(int i = 0; i < lineLength.Count; i++) {
			Rect r = new Rect(screen.x*readouts[i].x-x*0.5f, screen.y*(1f-readouts[i].y)-y*0.5f, x, y);
			GUI.BeginGroup(r, block);
				GUI.Box(new Rect(margin, margin, x-margin*2, y-margin*2), "",  hole);
				float dist = lineLength[i]*worldUnitsToCM;
				//if(ProjectileDriver.isNewBeta && SelectZoomLevelGUI.selectedButton==2) dist *= ProjectileDriver.macroGammaBetaFudge;
				string units = "           cm";
				if(dist < 0.01f) {
					dist *= 10;
					units = "           mm";
				}
				//if(!ProjectileDriver.isNewBeta) {
					if(dist < 0.002f) {
						GUI.Label(new Rect(margin*2.5f, margin*2.5f, x, y), "interaction", font);
					} else {
						GUI.Label(new Rect(margin*2.5f, margin*2.5f, x, y), dist.ToString("00.00"), font); // "" + modes[i] + "." + 
						GUI.Label(new Rect(margin*2.5f, margin*2.5f, x, y), units, font);
					}
				//} else {
				//	GUI.Label(new Rect(margin*2.5f, margin*2.5f, x, y), "pugdog", font);
				//}
				
				if(GUI.Button(new Rect(margin, margin, x-margin*2, y-margin*2), "", clear)) {
					ShooterDriver.i.ParkingMode(midpoints[i], mediums[i], modes[i]);
				}
			GUI.EndGroup();
		}
		
	}
	
	void Update () {
		for(int i = 0; i < readouts.Length; i++) {
			if(i < lineRenderers2.Count) {
				// this code is copy/pasted elsewhere
				Ray ray1 = Camera.main.ViewportPointToRay(readouts[i]);
				float hitDist = 0;
				Vector3 result = ShooterDriver.i.gun.position;
				Plane plane1 = new Plane(result, result+ Vector3.forward, result+Vector3.right);
				plane1.Raycast(ray1, out hitDist);
				Vector3 result2 = ray1.origin + ray1.direction*hitDist;
				lineRenderers2[i].SetPosition(1, result2);
			}
		}
	}
	
	public static void NewLine (Vector3 point, MediumInfo m, int mode) {
		if(m) instance.mediums[instance.lines.Count] = m;
		instance.modes[instance.lines.Count] = mode;
		if(SelectZoomLevelGUI.selectedButton != 2) return;
		List<Vector3> line = new List<Vector3>();
		line.Add(point); line.Add(point + Vector3.up*0.01f);
		instance.lineLength.Add(0);
		instance.lines.Add(line);
		GameObject o = new GameObject("asd");
		LineRenderer lr = o.AddComponent<LineRenderer>();
		lr.material = instance.lineMaterial;
		lr.SetWidth(instance.lineWidth, instance.lineWidth);
		instance.lineRenderers.Add(lr);
		GameObject o2 = new GameObject("asd2");
		LineRenderer lr2 = o2.AddComponent<LineRenderer>();
		lr2.material = instance.lineMaterial;
		lr2.SetWidth(instance.lineWidth, instance.lineWidth);
		lr2.SetPosition(0, Vector3.zero);
		lr2.SetPosition(1, Vector3.zero);
		instance.lineRenderers2.Add(lr2);
	}
	
	public static void UpdateLine (Vector3 point, bool newPoint) {
		if(SelectZoomLevelGUI.selectedButton != 2) return;

		Debug.Log(instance);

		Debug.Log(instance.lines);

		List<Vector3> line = instance.lines[instance.lines.Count-1];
		line[line.Count-1]= point;
		if(newPoint) line.Add(point + (point-line[line.Count-2]).normalized*0.001f);
		
		LineRenderer lr = instance.lineRenderers[instance.lineRenderers.Count-1];
		lr.SetVertexCount((line.Count+2)*2 - 2);
		int ii = 0;
		float dist = 0f;
		for(int i = 0; i < line.Count-1; i++) {
			Vector3 i0 = line[i  ];
			Vector3 i1 = line[i+1];
			Vector3 delta = i1-i0;
			float d = delta.magnitude;
			float dr = 1f/d;
			Vector3 n0 = Vector3.Cross(Vector3.up, delta*dr);
			Vector3 n1 = n0;
			if(i > 0) {
				n0 = (n0 + Vector3.Cross(Vector3.up, (i0-line[i-1]).normalized))*0.5f;
			}
			if(i < line.Count-2) {
				n1 = (n1 + Vector3.Cross(Vector3.up, (i1-line[i+2]).normalized))*0.5f;
			}
			n0*= (instance.lines.Count&1) == 0 ? 1 : -1;
			n1*= (instance.lines.Count&1) == 0 ? 1 : -1;
			i0 += n0 * instance.bracketSize * 1.2f;
			i1 += n1 * instance.bracketSize * 1.2f;
			
			dist += d;
			
			if(i == 0) {
				Vector3 bracket = -n0;
				lr.SetPosition(ii, i0 + bracket * instance.bracketSize);
				ii++;
				lr.SetPosition(ii, i0 + bracket * 0.001f);
				ii++;
			}
			
			lr.SetPosition(ii, i0);
			ii++;
			lr.SetPosition(ii, i1 - delta*dr*0.001f);
			ii++;
			
			if(i == line.Count-2) {
				Vector3 bracket = -n1;
				lr.SetPosition(ii, i1 + bracket * 0.001f);
				ii++;
				lr.SetPosition(ii, i1 + bracket * instance.bracketSize);
				ii++;
			}
		}
		
		instance.lineLength[instance.lineLength.Count-1] = dist;
		dist *= 0.5f;
		Vector3 result = line[0];
		Vector3 midpoint = line[0];
		
		for(int i = 0; i < line.Count-1; i++) {
			Vector3 i0 = line[i  ];
			Vector3 i1 = line[i+1];
			midpoint = (i0 + i1)*0.5f;
			Vector3 delta = i1-i0;
			float d = delta.magnitude;
			float dr = 1f/d;
			Vector3 n0 = Vector3.Cross(Vector3.up, delta*dr);
			Vector3 n1 = n0;
			if(i > 0) {
				n0 = (n0 + Vector3.Cross(Vector3.up, (i0-line[i-1]).normalized))*0.5f;
			}
			if(i < line.Count-2) {
				n1 = (n1 + Vector3.Cross(Vector3.up, (i1-line[i+2]).normalized))*0.5f;
			}
			n0*= (instance.lines.Count&1) == 0 ? 1 : -1;
			n1*= (instance.lines.Count&1) == 0 ? 1 : -1;
			i0 += n0 * instance.bracketSize * 1.2f;
			i1 += n1 * instance.bracketSize * 1.2f;
			if(dist < d) {
				result = Vector3.Lerp(i0, i1, dist/d);
			}
			dist -= d;
		}
		LineRenderer lr2 = instance.lineRenderers2[instance.lineRenderers2.Count-1];
		instance.midpoints[instance.lineRenderers2.Count-1] = midpoint;
		lr2.SetPosition(0, result);
		// this code is copy/pasted elsewhere
		Ray ray1 = Camera.main.ViewportPointToRay(instance.readouts[instance.lines.Count-1]);
		float hitDist = 0;
		Plane plane1 = new Plane(result, result+ Vector3.forward, result+Vector3.right);
		plane1.Raycast(ray1, out hitDist);
		Vector3 result2 = ray1.origin + ray1.direction*hitDist;
		lr2.SetPosition(1, result2);
		
	}
	
	public static void ClearLines () {
		if(SelectZoomLevelGUI.selectedButton != 2) return;
		for(int i = 0; i < instance.lineRenderers.Count;  i++) {
			if(instance.lineRenderers[i])
				Destroy( instance.lineRenderers[i].gameObject);
		}
		for(int i = 0; i < instance.lineRenderers2.Count; i++) {
			if(instance.lineRenderers2[i]) 
				Destroy(instance.lineRenderers2[i].gameObject);
		}
		instance.lineRenderers = new List<LineRenderer>();
		instance.lineRenderers2 = new List<LineRenderer>();
		instance.lines = new List<List<Vector3>>();
		instance.lineLength = new List<float>();
	}
}
