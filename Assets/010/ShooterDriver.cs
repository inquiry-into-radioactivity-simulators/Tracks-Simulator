using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShooterDriver : MonoBehaviour {

	public float rangeXMin = -40.0f;
	public float rangeXMax = 40.0f;
	public float rangey = 2.0f;
	public float rotSpeed = 5.0f;
	public float moveSpeed = 1.0f;
	public GameObject rot;
	public GameObject slide;
	public Transform gun;
	public Transform[] camLerpToArray;
	public float waitBeforeFiring = 0.3f;
	public DrunkCameraman cameraScript;

	public GameObject projectilePrefab;
	public Animation buttonAnim;

	public Transform[] colliders;

	public static int firing = -1;
	float origRot = 0.0f;
	float rote = 0.0f;
	float move = 0.0f;
	float lerpTimer = 0.0f;
	float origNearClip = 0f;
	float origFarClip = 0f;
	public float lerpTime = 1.0f;
	public float closeCamNearClip = 0.01f;
	public float closeCamFarClip = 10f;
	float originalFieldOfView;
	public float[] fieldOfView;
	
	public LayerMask normalLayers;
	public LayerMask firingLayers;
	public LayerMask atomLayers;
	
	Transform camOrigTranform;
	Vector3 projectileToCameraDelta;
	Transform camT;
	Camera cam;
	Transform mmCamT;
	Camera mmCam;
	Transform projectileT;
	Transform parkingMarker;
	Transform parkingBeginMarker;
	
	public static ShooterDriver i;
	
	void Start () {
		useParking = false;
		GameObject parkingSign = new GameObject("Parking Sign");
		parkingMarker = parkingSign.transform;
		GameObject parkingBegin = new GameObject("Parking Begin");
		parkingBeginMarker = parkingBegin.transform;
		i = this;
		mmCamT = GameObject.FindWithTag("MiniMapCamera").transform;
		mmCam = mmCamT.GetComponent<Camera>();
		cam = Camera.main;
		camT = cam.transform;
		originalFieldOfView = cam.fieldOfView;
		camOrigTranform = new GameObject().transform;
		camOrigTranform.transform.parent = camT.parent;
		origRot = rot.transform.localEulerAngles.y;
		origNearClip = cam.nearClipPlane;
		origFarClip = cam.farClipPlane;
	}

	void LateUpdate () { 
		Transform camLerpTo = camLerpToArray[SelectZoomLevelGUI.selectedButton];
		if(useParking) {
			parkingMarker.position = new Vector3(parkPos.x, camLerpTo.position.y, parkPos.z);
			camLerpTo = parkingMarker;
		}
		cameraScript.enabled = firing == -1;
		//cam.enabled = firing;
		if(firing == 0) {
			Transform usedOrig = useParking ? parkingBeginMarker : camOrigTranform;
			lerpTimer += Time.deltaTime/lerpTime;
			camT.position = Vector3.Lerp(usedOrig.position, camLerpTo.position, lerpTimer);
			camT.position = new Vector3(camT.position.x, Mathf.Lerp(usedOrig.position.y, camLerpTo.position.y, lerpTimer*lerpTimer*lerpTimer), camT.position.z);
			camT.rotation = Quaternion.Slerp(usedOrig.rotation, Quaternion.LookRotation(-Vector3.up, Vector3.forward), lerpTimer*lerpTimer*lerpTimer); 
			cam.nearClipPlane = Mathf.Lerp(origNearClip, SelectZoomLevelGUI.selectedButton == 2 ? origNearClip : closeCamNearClip, lerpTimer);
			cam.farClipPlane  = Mathf.Lerp( origFarClip,  SelectZoomLevelGUI.selectedButton == 2 ? origFarClip : closeCamFarClip, lerpTimer);
			cam.fieldOfView  = Mathf.Lerp( originalFieldOfView,  fieldOfView[SelectZoomLevelGUI.selectedButton], lerpTimer);
			
			if(lerpTimer >= 1.1f) {
				// advances firing state
				StartCoroutine("FireCoroutine");
				firing = 1;
			}
		} else if(firing == 1) {
			// waiting
		} else if(firing == 2) {
			if(lerpTime > 1.1f) lerpTime -= 0.2f;
			if(!useParking) camT.position = projectileT.position + projectileToCameraDelta;
		} else {
			cam.cullingMask = normalLayers;
			camOrigTranform.localPosition = camT.localPosition;
			camOrigTranform.localRotation = camT.localRotation;
			lerpTimer = 0;
			
			float oldRote = rote;
			float oldMove = move;
			rote = Mathf.Clamp(rote + rotSpeed * Time.deltaTime * Input.GetAxis("Horizontal"), rangeXMin, rangeXMax);
			rot.transform.localEulerAngles = new Vector3(rot.transform.localEulerAngles.x, rote + origRot, rot.transform.localEulerAngles.z);
			
			move = Mathf.Clamp(move + moveSpeed * Time.deltaTime * Input.GetAxis("Vertical"), 0, rangey);
			slide.transform.localPosition = new Vector3(slide.transform.localPosition.x, slide.transform.localPosition.y, move);
			
			bool hit = false;
			foreach(Transform t in colliders) {
				foreach(Transform c in t) {
					if(Physics.Raycast(t.position, c.position-t.position, (c.position-t.position).magnitude)) {
						hit = true;
					}
				}
			}
			if(hit) {
				rote = oldRote;
				move = oldMove;
				slide.transform.localPosition = new Vector3(slide.transform.localPosition.x, slide.transform.localPosition.y, move);
				rot.transform.localEulerAngles = new Vector3(rot.transform.localEulerAngles.x, rote+ origRot, rot.transform.localEulerAngles.z);
			}
		}
		
		Molecules.DoUpdate();
	}
	
	public void CleanUp () {
		if(useParking) {
			useParking = false;
			SelectZoomLevelGUI.selectedButton = oldSelButt;
		}
		firing = -1;
		ProjectileDriver.speedScale = 1;
		TimeMan.timeScale = 1;
		Destroy(projectileT.gameObject);
		camT.position = camOrigTranform.position;
		camT.rotation = camOrigTranform.rotation; 
		cam.nearClipPlane = origNearClip;
		cam.farClipPlane  = origFarClip;
		cam.fieldOfView  = originalFieldOfView;
		cam.cullingMask = normalLayers;
	}
	
	int oldSelButt = 0;
	
	public void ParkingMode(Vector3 pos, MediumInfo m, int mode) {
		if(m && (m.name == "Flesh" || m.name == "Epidermis")) pos += Vector3.Scale(new Vector3(1,0,1), (pos-transform.position)).normalized*1f;
		//Vector3 pos2 = new Vector3(pos.x, rot.transform.position.y, pos.z);
		//rot.transform.LookAt(pos2);
		MacroModeGUI.ClearLines();
		parkMedium = m;
		oldSelButt = SelectZoomLevelGUI.selectedButton;
		SelectZoomLevelGUI.selectedButton = 0;
		lerpTimer = 0;
		parkingBeginMarker.position = camT.position;
		parkingBeginMarker.rotation = camT.rotation;
		ProjectileDriver.speedScale = 1;
		TimeMan.timeScale = 1;
		Destroy(projectileT.gameObject);
		firing = 0;	
		parkPos = pos;
		useParking = true;
		parkingMode = mode;
	}
	
	public static int  parkingMode = 0;
	public static bool useParking = false;
	Vector3 parkPos;
	MediumInfo parkMedium;
	
	/*
	IEnumerator ParkingModeHelper (Vector3 pos) {
	
	}
	*/
	public void OnMouseDown () {	
		if(firing == -1){
			firing = 0;	
		}	
	}	
	
	IEnumerator FireCoroutine () {
		ProjectileDriver.updateEnergy = true;
		yield return new WaitForSeconds(waitBeforeFiring);
		cam.cullingMask = SelectZoomLevelGUI.selectedButton == 2 ? normalLayers : firingLayers;
		if(SelectZoomLevelGUI.selectedButton == 0) {
			cam.cullingMask = atomLayers;
			Cells.i.Start();
			Cells.iEpi.Start();
		}
		if(SelectZoomLevelGUI.selectedButton == 1) {
			Molecules.CleanUpAll();
		}

		GameObject i = Instantiate(projectilePrefab, gun.position, gun.rotation) as GameObject;
		projectileT = i.transform;
		if(useParking) {
			Vector3 forward = parkingMode < 2 ? gun.forward : MacroModeGUI.bounceDir;
			projectileT.position = parkPos - forward * 0.008f;
			Vector3 pos2 = new Vector3(parkPos.x, projectileT.position.y, parkPos.z);
			projectileT.LookAt(pos2);
			Vector3 pos = (parkingMode == 1 ? camT.position + transform.forward * 0.003f : camT.position);
			Molecules.Center(pos);
		}
		ProjectileDriver script = i.GetComponent<ProjectileDriver>();
		script.parent = this;
		script.ptype = (ProjectileDriver.ProjectileType)SelectZoomLevelGUI.selectedParticle;
		script.CalculateDelta(cam.transform.position);
		
		
		if(useParking) {
			script.InitMedium(parkMedium, parkingMode);
		}
		projectileToCameraDelta = camT.position - gun.position;
		firing = 2;
	}

}