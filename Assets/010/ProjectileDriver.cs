using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileDriver : MonoBehaviour {

	public enum ProjectileType {P, E, G};
	public static bool isNewBeta = false;
	public static float macroGammaBetaFudge = 0.8f;
	public ProjectileType ptype = ProjectileType.P;
	public Transform graphic2;
	public ParticleSystem trail0;
	public ParticleSystem trail1;
	public float emissionPerEnergyLost = 1000f;
	public float electronSpew = 0.01f;
	
	public GameObject[] particlePrefabs = new GameObject[3];
	public GameObject[] particlePrefabsSmall = new GameObject[3];
	public float [] speeds = new float[3];
	public float [] speedMult = new float[3];
	public float [] energies = new float[3];
	public float [] energyLosses = new float[3];
	public float [] interactionsPerAtom = new float[3];

	
	//public GameObject cameraPrefab1;
	//public GameObject cameraPrefab2;
	
	public ShooterDriver parent;
	
	//GameObject cameraInstance;
	float camRadius = 0;
	float theSpeed = 0;
	float curEnergy = 0;
	float originalEnergy = 0f;
	float energyLossPerDensityPerUnit = 1f;
	List<MediumInfo> currentMedium;
	Transform graphic;
	
	Vector3 theDelta;
	Vector3 lastPos;
	
	public static float lifeBar = 1f;
	public static float speedometer = 1f;
	public static float speedReading = 1f;
	public static float energyReading = 1f;
	public static float	chanceToInteract = 1f;
	public static bool fastForward1 = false;
	public static bool fastForward2 = false;
	public static bool noFastForward = false;
	public static Transform trans;
	public static ProjectileDriver instance;
	public Transform startT;
	int curLineMode = 0;
	public int playbackMode = 0;
	bool hasWinkedOut = false;
	int enteredChamberState = 0;
	Vector3 enteredChamberPos;
	
	void Start () {
		GameObject startTObj = new GameObject();
		startT = startTObj.transform;
		startT.position = transform.position;
		startT.rotation = transform.rotation;
		startT.localScale = Vector3.one*100;
		instance = this;
		trans = transform;
		trans.parent = startT;
		isNewBeta = false;
		if(ShooterDriver.parkingMode == 2 && ShooterDriver.useParking) {
			ptype = ProjectileType.E;
			isNewBeta = true;
		}
		currentMedium = new List<MediumInfo>();
		MacroModeGUI.NewLine(transform.position, null, 0);
		graphic = (Instantiate(((SelectZoomLevelGUI.selectedButton == 0 || ShooterDriver.useParking) ?  particlePrefabsSmall[(int)ptype] : particlePrefabs[(int)ptype]) , transform.position,transform.rotation) as GameObject).transform;
		graphic.parent = transform;
		theSpeed = speeds[(int)ptype];
		curEnergy = energies[(int)ptype];
		if(ptype == ProjectileType.G) curEnergy *= 1f+(Random.value-0.5f)*0.7f;
		energyLossPerDensityPerUnit = energyLosses[(int)ptype];

		originalEnergy = curEnergy;
		//cameraInstance = Instantiate(cameraPrefab1, transform.position, Quaternion.identity) as GameObject;
	}
	
	void OnDisable () {
		MacroModeGUI.ClearLines();
	}
	
	public static bool updateEnergy = false;
	
	public static float speedScale = 1;
	
	int lastCM1 = 0;
	float cm1 = 0;
	
	void Update () {
	
		if(TimeMan.timeScale != 1 || updateEnergy || SelectZoomLevelGUI.selectedButton != 0) {
			lifeBar = (ptype == ProjectileType.G ? 1 : curEnergy/originalEnergy);
			updateEnergy = false;
		}
		speedReading = theSpeed * (1f+lifeBar)*0.5f;
		chanceToInteract = interactionsPerAtom[(int)ptype];
		energyReading = (ptype == ProjectileType.G ? originalEnergy : curEnergy);

		// TODO replace this with particleSystem shuriken stuff ? VelocityOverLifetime?		
		// if(SelectZoomLevelGUI.selectedButton == 1) {
		// 	Particle[] ps0 = trail0.particles;
		// 	Particle[] ps1 = trail1.particles;
		// 	//float max = 0;
		// 	for(int i = 0; i < ps1.Length; i++) {
		// 		//Color c1 = ps1[i].color;
		// 		//Vector3 orig = new Vector3((c1.r*1000f)-500f, (c1.g*1000f)-500f, (c1.b*1000f)-500f);
		// 		//if(i == 50 || i == 100 || i == 150 || i == 200 || i == 250 || i == 300 || i == 350) {
		// 			//Debug.Log(c1.r + ", " + c1.g + ", " + c1.b + ", :: " + orig);
		// 			//Debug.DrawLine(ps0[i].position, ps1[i].position);
		// 		//}

		// 		bool late = ps1[i].startEnergy - ps1[i].energy > 5;
		// 		Vector3 delta = ps0[i].position - ps1[i].position;
		// 		float mag = delta.magnitude; 
		// 		if(mag != 0) delta /= mag;
		// 		ps1[i].velocity = Vector3.Lerp(ps1[i].velocity, delta*0.2f, Time.deltaTime * (0.001f + Mathf.Clamp01(1f/(mag*5))*(late ? 0.0008f : 0)));
		// 		ps1[i].velocity = Vector3.Lerp(ps1[i].velocity, Vector3.zero, Time.deltaTime * (late ? ps1[i].velocity.magnitude*0.3f + 0.002f : 0)+0.001f);
		// 		if(mag < 0.001f && late) {
		// 			ps0[i].position = Vector3.zero;
		// 			ps1[i].position = Vector3.zero;
		// 			ps0[i].velocity = Vector3.zero;
		// 			ps1[i].velocity = Vector3.zero;
		// 		}
		// 		//if(mag > max) max = mag;
		// 	}
		// 	//Debug.Log(max);
		// 	trail0.particles = ps0;
		// 	trail1.particles = ps1;
		// }
		
		if(curEnergy <= 0) {
			curEnergy = 0;
			if(!hasWinkedOut) {
				StartCoroutine(WinkOut());
				hasWinkedOut = true;
			}
			//foreach(Transform t in graphic) {
			//	Destroy(t.gameObject);
			//}
			//trail0.emit = false;
			//trail1.emit = false;
			return;
		}
		int cm1i = (int)(cm1*5.5f);
		if(cm1i != lastCM1 && ptype == ProjectileType.G) {
			//Debug.Log("" + cm1i +"-"+ lastCM1);
			int delta = cm1i - lastCM1;
			while(delta > 0) {
				if(Random.value > 0.907f) {
					GammaHit();
					return;
				}
				delta --;
			}
			lastCM1 = cm1i;
		}
		Vector3 dir = transform.forward;
		Vector3 point = transform.position;
		float dist = theSpeed * speedReading * speedMult[SelectZoomLevelGUI.selectedButton] * speedScale;

		float density = (ptype == ProjectileType.E ? 0.01f : 0.001f);
		for(int i =0; i < currentMedium.Count; i++) density = currentMedium[i].density;
		float loss = dist*density*energyLossPerDensityPerUnit;
		MaterialZones.SolidMaterial check = MaterialZones.SolidMaterial.Air;//MaterialZones.Check(transform.position, true);
		noFastForward = isNewBeta || (check != MaterialZones.SolidMaterial.Air) || Physics.Raycast(point,  dir, dist/speedScale, ~(1<<2)) || curEnergy-loss <= 0;
		if(lifeBar < 0.01f) noFastForward = false;
		if(check == MaterialZones.SolidMaterial.Chamber) {
			if(ptype != ProjectileType.G) {
				if(enteredChamberState == 0) {
					enteredChamberState = 1;
					enteredChamberPos = point;
				} else if(enteredChamberState == 1  && (enteredChamberPos - point).sqrMagnitude > (ptype == ProjectileType.E && !isNewBeta ? 37 : 3)){
					enteredChamberState = 2;
					GeigerCounter.Beep();
				}
			} else {
				loss *= 100;
			}
		} else {
			if(enteredChamberState == 1) {
				enteredChamberState = 2;
				GeigerCounter.Beep();
			}
		}
		fastForward2 = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
		TimeMan.timeScale = (fastForward1 || fastForward2 ? 15 : 1);
		speedScale = (SelectZoomLevelGUI.selectedButton == 0 && ((fastForward1 || fastForward2) && !noFastForward ) ? 50 : 1);

		if(ptype == ProjectileType.P && SelectZoomLevelGUI.selectedButton != 2 && density > 0.5f) {
			loss *= 0.02f;
		}
		if(SelectZoomLevelGUI.selectedButton == 1) {
			Vector3 disPos = trail0.transform.position;
			float val = Random.Range(0.2f*loss*emissionPerEnergyLost*(ptype == ProjectileType.G ? 0 : 1), 1.8f*loss*emissionPerEnergyLost*(ptype == ProjectileType.G ? 0 : 1)) * Time.deltaTime;
			var mainModule0 = trail0.main;
			var mainModule1 = trail1.main;
			//var velocityModule0 = trail0.velocityOverLifetime;
			//var velocityModule1 = trail1.velocityOverLifetime;

			while(val > 0) {
				if(val > Random.value) {
					Vector3 pp = Vector3.Lerp(lastPos, disPos, Random.value);
					Vector3 emVel = dir*dist;
					Vector3 spew = emVel*0.17f + Vector3.Scale(new Vector3(1,0,1), Random.insideUnitSphere*dist*electronSpew);
					float energy = Random.Range(mainModule0.startLifetime.constantMin, mainModule0.startLifetime.constantMax);

					var params0 = new ParticleSystem.EmitParams();
					params0.position = pp;
					params0.velocity = Vector3.zero;
					params0.startSize = Random.Range(mainModule0.startSize.constantMin, mainModule0.startSize.constantMax);
					params0.startLifetime = energy;
					params0.startColor = Color.white;

					var params1 = new ParticleSystem.EmitParams();
					params1.position = pp;
					params1.velocity = spew;
					params1.startSize = Random.Range(mainModule1.startSize.constantMin, mainModule1.startSize.constantMax);
					params1.startLifetime = energy;
					params1.startColor = Color.white;

					trail0.Emit (params0, 1);
					trail1.Emit (params1, 1);
				}
				val -= 1;
			}
			lastPos = disPos;
		}
		
		speedometer = dist;
		
		dist *= Time.deltaTime;
		loss *= Time.deltaTime;
		cm1 += loss;
		if(ptype != ProjectileType.G) curEnergy -= loss;
		if(curEnergy < 0) {
			dist += curEnergy/(density*energyLossPerDensityPerUnit);
		}
		
		
		/*
		Vector3 pos = Vector3.zero;
		RaycastHit[] hits1 = Physics.RaycastAll(point         ,  dir, dist, ~(1<<2));
		RaycastHit[] hits2 = Physics.RaycastAll(point+dir*dist, -dir, dist, ~(1<<2));
		for(int i = 0; i < hits1.Length + hits2.Length; i++) {
			RaycastHit hit = i < hits1.Length ? hits1[i] : hits2[i-hits1.Length];
			if((hit.point - pos).sqrMagnitude > 0.00001f) {
				pos = hit.point;
				if(Vector3.Dot(hit.normal,dir) < 0) {
					MediumInfo medium = hit.collider.GetComponent<MediumInfo>();	
				}
				MacroModeGUI.NewLine(hit.point, null, curLineMode);
			}
		}
		*/
		
		//List<MediumInfo> oldList = currentMedium;
		string lastName = "";
		for(int i =0; i < currentMedium.Count; i++) lastName = currentMedium[i].name;
		currentMedium = new List<MediumInfo>();
		RaycastHit[] hits3 = Physics.RaycastAll(point         ,  Vector3.down, 100, (1<<14));
		//RaycastHit[] hits2 = Physics.RaycastAll(point+dir*dist, -dir, dist, (1<<14));

		for(int i = 0; i < hits3.Length; i++) {
			RaycastHit hit = hits3[i];
			MediumInfo medium = hit.collider.GetComponent<MediumInfo>();
			
			//Debug.Log(hit.textureCoord.y);
			if(medium.name == "Flesh" && hit.textureCoord.y < MaterialZones.i.skinWidthFudge && hit.textureCoord.y > 0.05f) {
				MediumInfo nm = null;
				nm = gameObject.GetComponent<MediumInfo>();
				if(!nm) nm = gameObject.AddComponent<MediumInfo>();
				nm.name = "Epidermis";
				nm.density = (SelectZoomLevelGUI.selectedButton == 1 ? 3 : 1.3f);
				currentMedium.Add(nm);	
			} else if(hit.textureCoord.y < 0.05f) {
				//Debug.Log(hit.textureCoord.y);
				currentMedium.Add(medium);
			}
		}
		string name1 = "";
		for(int i =0; i < currentMedium.Count; i++) name1 = currentMedium[i].name;
		if(name1 != lastName && !((name1 == "Epidermis" && lastName == "Flesh") || (lastName == "Epidermis" && name1 == "Flesh"))) {
			//Debug.Log("was in: " + lastName + ", now in: " + name1);
			MacroModeGUI.NewLine(point, (currentMedium.Count > 0 ? currentMedium[currentMedium.Count-1] : null), curLineMode);
		}
		
		Molecules.Collide(transform.position, transform.position+dir*dist);
		
		transform.localPosition += transform.parent.InverseTransformDirection(dir)*dist*0.01f;
	}
	
	public void InitMedium (MediumInfo m, int playback) {
		playbackMode = playback;
		if(m != null && currentMedium != null) //{
			currentMedium.Add(m);
		//} else {
		//	Debug.Log("error");
		//}
	}
	
	public void CalculateDelta (Vector3 pos) {
		theDelta = transform.position - pos;
	}
	
	void LateUpdate () {
		if(curLineMode == 2) {
			MacroModeGUI.bounceDir = transform.forward;
		}
		MacroModeGUI.UpdateLine(transform.position, false);
		if(graphic){
			graphic.position = new Vector3(graphic.position.x, Camera.main.transform.position.y + theDelta.y, graphic.position.z);
			graphic.localPosition = new Vector3(0,  graphic.localPosition.y, 0);
		}
		//graphic2.position = Camera.main.transform.position + theDelta;
		//cameraInstance.transform.position = transform.position;
		//cameraInstance.transform.rotation = Quaternion.LookRotation(-Vector3.up, transform.forward);
	}
	
	public void PlaybackGammaHit () {
		isNewBeta = true;
		Destroy(graphic.gameObject);
		ptype = ProjectileType.E;
		graphic = (Instantiate(particlePrefabsSmall[(int)ptype], transform.position,transform.rotation) as GameObject).transform;
		graphic.parent = transform;
		theSpeed = speeds[(int)ptype]*0.9f;
		curEnergy = energies[(int)ptype];
		energyLossPerDensityPerUnit = energyLosses[(int)ptype]*(SelectZoomLevelGUI.selectedButton==2?macroGammaBetaFudge : 1);
		originalEnergy = curEnergy;
		transform.rotation = Quaternion.LookRotation(MacroModeGUI.bounceDir);
	}
	
	public void GammaHit () {
		isNewBeta = true;
		Destroy(graphic.gameObject);
		ptype = ProjectileType.E;
		MediumInfo src = currentMedium[currentMedium.Count-1];
		//MediumInfo hitPt = gameObject.AddComponent<MediumInfo>();
		MediumInfo betaPath = gameObject.AddComponent<MediumInfo>();
		//hitPt.density = src.density;
		betaPath.density = src.density;
		//hitPt.name = src.name;
		betaPath.name = src.name;
		//MacroModeGUI.NewLine(transform.position, hitPt, curLineMode);
		//MacroModeGUI.UpdateLine(transform.position+transform.forward*0.0001f, false);
		MacroModeGUI.NewLine(transform.position+transform.forward*0.0001f, betaPath, 1);
		MacroModeGUI.UpdateLine(transform.position+transform.forward*0.0002f, false);
		MacroModeGUI.NewLine(transform.position+transform.forward*0.0003f, betaPath, 2);
		curLineMode = 2;
		graphic = (Instantiate(particlePrefabs[(int)ptype], transform.position,transform.rotation) as GameObject).transform;
		graphic.parent = transform;
		theSpeed = speeds[(int)ptype]*0.9f;
		curEnergy = energies[(int)ptype];
		energyLossPerDensityPerUnit = energyLosses[(int)ptype]*(SelectZoomLevelGUI.selectedButton==2?macroGammaBetaFudge : 1);
		originalEnergy = curEnergy;
		int c = MacroModeGUI.LineCount();
		float rotY = Random.Range(30,60) * (c == (int)(Mathf.Round((float)c*0.5f - 0.1f)*2) ? -1 : 1);
		transform.Rotate(0, rotY,0);
		//Debug.Log("ASD: " + );
	}
	
	IEnumerator WinkOut () {
		yield return new WaitForSeconds(4);
		var e = graphic.GetComponent<ParticleSystem>().emission;
		e.enabled = false;
	}
}
