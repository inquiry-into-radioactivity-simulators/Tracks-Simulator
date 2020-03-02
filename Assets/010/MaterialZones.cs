using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialZones : MonoBehaviour {
	
	public float skinWidthNorm = 0.8f;
	public float skinWidthFudge = 0.5f;
	
	public enum SolidMaterial { Air, Epidermis, Steel, Wood, Flesh, Plastic, Soda, Chamber }

	public static MaterialZones i;
	public static SolidMaterial lastCheck;
	void Awake () {
		i = this;
	}
	
	public static SolidMaterial Check (Vector3 input) {
		lastCheck = i.Check2(input, false);
		return lastCheck;
	}
	
	public static SolidMaterial Check (Vector3 input, bool skinFudge) {
		lastCheck = i.Check2(input, skinFudge);
		return lastCheck;
	}
	
	public static Quaternion skinRotation;
	public static float skinPosition;
	
	//public static string lastStr = "";
	
	public SolidMaterial Check2 (Vector3 input, bool skinFudge) {
	
		List<MediumInfo> currentMedium = new List<MediumInfo>();
		RaycastHit[] hits3 = Physics.RaycastAll(new Vector3(input.x, 18.25958f, input.z),  Vector3.down, 100, (1<<14));
		//RaycastHit[] hits2 = Physics.RaycastAll(point+dir*dist, -dir, dist, (1<<14));
		//string strr = "hit: ";
		for(int i = 0; i < hits3.Length; i++) {
			RaycastHit hit = hits3[i];
			MediumInfo medium = hit.collider.GetComponent<MediumInfo>();
			//strr += hit.collider.gameObject.name +", ";
			
			float fudge = skinFudge ? skinWidthFudge : skinWidthNorm;
			//Debug.Log(fudge);
			if(skinFudge) Debug.Log(hit.textureCoord.y);
			if(medium.name == "Flesh" && hit.textureCoord.y < fudge && hit.textureCoord.y > 0.05f) {
				if(skinFudge) Debug.Log(hit.textureCoord.y + " < " + fudge);
				MediumInfo nm = null;
				nm = gameObject.GetComponent<MediumInfo>();
				if(!nm) nm = gameObject.AddComponent<MediumInfo>();
				nm.name = "Epidermis";
				nm.density = 1.3f;
				currentMedium.Add(nm);
				
				skinRotation = Quaternion.LookRotation(-Vector3.Scale(hit.normal, new Vector3(1,0,1)));
				skinPosition = hit.textureCoord.y;
				
			} else if(hit.textureCoord.y < 0.05f) {
				if(skinFudge) Debug.Log(hit.textureCoord.y + " < " + 0.05f);
				//Debug.Log(hit.textureCoord.y);
				currentMedium.Add(medium);
			}
		}
		
		//if(lastStr != strr) Debug.Log(strr );
		///lastStr = strr;
		
		string s = "Air";
		SolidMaterial m = SolidMaterial.Air;
		for(int i =0; i < currentMedium.Count; i++) s = currentMedium[i].name;
		if(s == "Soda") m = SolidMaterial.Soda;
		if(s == "Plastic") m = SolidMaterial.Plastic;
		if(s == "Steel") m = SolidMaterial.Steel;
		if(s == "Flesh") m = SolidMaterial.Flesh;
		if(s == "Epidermis") m = SolidMaterial.Epidermis;
		if(s == "Wood") m = SolidMaterial.Wood;
		if(s == "Chamber") m = SolidMaterial.Chamber;
		
		return m;
	}
	
	public static float cellScale = 0.5f;
	public static float cellMembrane = 0.006f;
	public static float cellScaleRecip = 2f;
	
	public Vector3 NearestTet(Vector3 pos, bool epidermis) {
		if(epidermis) {
			Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, skinRotation, Vector3.one);
			Vector3 local = mat.inverse.MultiplyPoint3x4(pos);
			local = new Vector3(Mathf.Round(pos.x*cellScaleRecip*0.2f)*cellScale*5f, pos.y, Mathf.Round(pos.z*cellScaleRecip*5f)*cellScale*0.2f);
			return mat.MultiplyPoint3x4(pos);
		} else {
			return new Vector3(Mathf.Round(pos.x*cellScaleRecip)*cellScale, pos.y, Mathf.Round(pos.z*cellScaleRecip)*cellScale);
		}
	}
	
	/*
	public Vector3 NearestTet(Vector3 pos) {
		Vector3[] ns = {
			new Vector3(-1, 0, 1),
			new Vector3(-1, 0, 0),
			new Vector3(-1, 0,-1),
			new Vector3( 0, 0, 1),
			new Vector3( 1, 0, 0),
			new Vector3( 0, 0,-1),
		};
		
		Vector3i pi = new Vector3i(Mathf.RoundToInt(pos.x*cellScale), 0, Mathf.RoundToInt(pos.z*cellScale));
		Vector3[] closest = new Vector3[ns.Length+1];
		closest[0] = SuddenlyTetrahedrons(pi.x, pi.y, pi.z);
		for(int i = 0; i < ns.Length; i++) {
			closest[i+1] = SuddenlyTetrahedrons(pi.x + (int)ns[i].x, pi.y, pi.z + (int)ns[i].z);
		}
		float d = 100000000;
		int ii = 0;
		for(int i = 0; i < closest.Length; i++) {
			float sm = new Vector3(pos.x-closest[i].x, 0, pos.z-closest[i].z).sqrMagnitude;
			if(sm < d) {
				d = sm;
				ii = i;
			}
		}
		return closest[ii]*cellScaleRecip;
	}
	*/
	
	public bool CellwallArea (Vector3 pos, SolidMaterial check) {
		float r = cellScale * 0.5f - cellMembrane*1.2f;
		float d = 0;
		
		if(check != SolidMaterial.Epidermis) {
			Vector3 nearPos = NearestTet(pos, false);
			d = new Vector3(pos.x-nearPos.x, 0, pos.z-nearPos.z).magnitude;
		} else {
			Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, skinRotation, Vector3.one).inverse;
			pos = mat.MultiplyPoint3x4(pos);
			Vector3 local = mat.MultiplyPoint3x4(pos);
			local = new Vector3(Mathf.Round(pos.x*cellScaleRecip*0.2f)*cellScale*5f, pos.y, Mathf.Round(pos.z*cellScaleRecip*5f)*cellScale*0.2f);
			d = new Vector3(pos.x-local.x, 0, pos.z-local.z).magnitude;
		}

		bool res = (d > r-cellMembrane*1.1f) && (d < r+cellMembrane*1.1f);
		//Debug.DrawRay(pos, new Vector3(pos.x-nearPos.x, 0, pos.z-nearPos.z), Color.green);
		//Vector3 v = Vector3.Cross(new Vector3(pos.x-nearPos.x, 0, pos.z-nearPos.z), Vector3.up).normalized;
		//Debug.DrawRay((nearPos+v*1), (new Vector3(pos.x-nearPos.x, 0, pos.z-nearPos.z).normalized * r), Color.red);
		//Debug.Log("" + (r-cellMembrane) + " < " + d + " < " + (r+cellMembrane) );
		return  res;
	}
	
	//void OnGUI () {
	//	GUI.DrawTexture(new Rect(0,0,texture.width*0.3f,texture.height*0.3f), texture);
	//	GUILayout.Box(a);
	//	GUI.Box(new Rect((asd.x*texture.width*0.3f)-10, ((1.0f-asd.z)*texture.height*0.3f)-10, 20, 20), "+");
	//}
}
