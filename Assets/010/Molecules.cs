using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Molecules : MonoBehaviour {

	public bool centerable = false;
	public Vector3 dimensions;
	public float randomFac = 0.35f;

	public float sizeScale = 1;
	public float atomSizeFudge = 1f;
	public float atomSpacingFudge = 1f;
	public int forceMoleculeAtomCount = 0;
	
	public Vector3 camPos;
	public Vector3 camPosOffset;
	public static Transform camT;
	public static Transform camTY;

	Vector3i position;
	Molecule[,,] molecules;
	
	public Molecule prefab;
	public Mesh[] types;
	public float[] interactionses;
	public Molecule[] typeInstances;
	public static Element currentDiatomic;
	
	static List<Molecules> instances = new List<Molecules>();
	
	public virtual void OnEnable() {
		instances.Add(this);
	}
	
	public virtual void OnDisable() {
		instances.Remove(this);
	}

	public void Start () {
		
		//sizeScale = transform.lossyScale.x;
		//Debug.Log(gameObject.name + sizeScale);
		
		typeInstances = new Molecule[types.Length];
		
		for(int i = 0; i < types.Length; i++) {
			if(types[i]) {
				Molecule mo = Instantiate(prefab) as Molecule;
				mo.transform.parent = transform;
				mo.gameObject.name = types[i].name + (Element)(i+1);
				currentDiatomic = (Element)(i+1);
				mo.FirstTime(types[i], i);
				FirstTimeFilter(mo);
				mo.interactions = interactionses[i];
				typeInstances[i] = mo;
			}
		}
		
		camT = Camera.main.transform;
		camTY = Camera.main.transform;
		if(camT && camTY) {
			camPos = new Vector3(camT.position.x, camTY.position.y, camT.position.z)+camPosOffset;
			//transform.position = new Vector3(0,0,0);
		}
		position = GetPosition(camPos);
		if(molecules != null && molecules.Length > 0) {
			for(int x = 0; x < dimensions.x; x++) {
				for(int z = 0; z < dimensions.z; z++) {
					for(int y = 0; y < dimensions.y; y++) {
						Destroy(molecules[x,z,y].gameObject);
					}
				}
			}
		}
		molecules = new Molecule[(int)dimensions.x, (int)dimensions.z, (int)dimensions.y];
		for(int x = 0; x < dimensions.x; x++) {
			for(int z = 0; z < dimensions.z; z++) {
				for(int y = 0; y < dimensions.y; y++) {
					Molecule m = Instantiate(prefab) as Molecule;
					GameObject go = m.gameObject;
					go.transform.parent = transform;
					go.layer = 12;
					SetMolecule(m, SuddenlyTetrahedrons(x+position.x, y+position.y, z+position.z));
					molecules[x,z,y] = m;
				}
			}
		}
	}
	
	public virtual void FirstTimeFilter(Molecule m) {
		
	}
	
	public virtual void SetMolecule(Molecule m, Vector3 pos) {
		m.Reset(pos, Quaternion.Euler(Random.insideUnitSphere*360), this, 1);
	}
	
	Vector3i GetPosition (Vector3 inPos) {
		return new Vector3i(Vector3.Scale(transform.InverseTransformPoint(inPos), new Vector3(1, 1.22474487f, 1.15470054f)), 1);
	}
	
	public static void CleanUpAll () {
		for(int i =0; i < instances.Count; i++) {
			instances[i].CleanUp();
		}
	}
	
	public void CleanUp () {
		position = new Vector3i(Vector3.zero, 1);
		for(int x = 0; x < dimensions.x; x++) {
			for(int z = 0; z < dimensions.z; z++) {
				for(int y = 0; y < dimensions.y; y++) {
					molecules[x, z, y].trans.position = molecules[x,z,y].refPos = molecules[x,z,y].worldPosition = Vector3.zero;
				}
			}
		}
	}
	
	public static void DoUpdate () {
		for(int i = 0; i < instances.Count; i++) {
			instances[i].InstanceDoUpdate();
		}
	}
	public void InstanceDoUpdate () {
		if(SelectZoomLevelGUI.selectedButton != 0) return;
		camT = Camera.main.transform;
		camTY = ProjectileDriver.trans;
		if(camT && camTY) {
			camPos = new Vector3(camT.position.x, camTY.position.y, camT.position.z)+camPosOffset;
			//transform.position = new Vector3(0,0,0);
		} else {
			//transform.position = new Vector3(-1000,-1000,-1000);
		}
		Vector3i newPosition = GetPosition(camPos);
		
		if(Mathf.Abs(newPosition.x-position.x)+Mathf.Abs(newPosition.z-position.z) > 4) {
			position = newPosition;
			for(int x = 0; x < dimensions.x; x++) {
				for(int z = 0; z < dimensions.z; z++) {
					for(int y = 0; y < dimensions.y; y++) {
						Vector3 pos = SuddenlyTetrahedrons(x+position.x, y+position.y, z+position.z);
						SetMolecule(molecules[x, z, y], pos);
					}
				}
			}
			return;
		}
		
		if(position.x == 0 && position.y == 0 && position.z == 0) position = newPosition;
		if(newPosition.x-1 > position.x) newPosition.x = position.x+1;
		if(newPosition.x+1 < position.x) newPosition.x = position.x-1;
		if(newPosition.z-1 > position.z) newPosition.z = position.z+1;
		if(newPosition.z+1 < position.z) newPosition.z = position.z-1;
		if(newPosition != position) {
			int dx = newPosition.x-position.x;
			int dz = newPosition.z-position.z;
			
			position = newPosition;
			Molecule[,,] newMolly = new Molecule[(int)dimensions.x, (int)dimensions.z, (int)dimensions.y];
			for(int x = 0; x < dimensions.x; x++) {
				for(int z = 0; z < dimensions.z; z++) {
					int nx = x+dx; if(nx >= dimensions.x) nx = 0; if(nx < 0) nx = (int)dimensions.x-1;
					int nz = z+dz; if(nz >= dimensions.z) nz = 0; if(nz < 0) nz = (int)dimensions.z-1;
					for(int y = 0; y < dimensions.y; y++) {
						newMolly[x,z,y] = molecules[nx,nz,y];
					}
				}
			}
			molecules = newMolly;
			int redoCol = dx == 0 ? -1 : (dx > 0 ? (int)dimensions.x-1 : 0);
			int redoRow = dz == 0 ? -1 : (dz > 0 ? (int)dimensions.z-1 : 0);
			if(redoCol != -1) {
				for(int z = 0; z < dimensions.z; z++) {
					for(int y = 0; y < dimensions.y; y++) {
						Vector3 pos = SuddenlyTetrahedrons(redoCol+position.x, y+position.y, z+position.z);
						//Debug.DrawLine(pos, pos + Vector3.up);
						SetMolecule(molecules[redoCol, z, y], pos);
					}
				}
			}
			if(redoRow != -1) {
				for(int x = 0; x < dimensions.x; x++) {
					for(int y = 0; y < dimensions.y; y++) {
						Vector3 pos = SuddenlyTetrahedrons(x+position.x, y+position.y, redoRow+position.z);
						//Debug.DrawLine(pos, pos + Vector3.up);
						SetMolecule(molecules[x, redoRow, y], pos);
					}
				}
			}
		}
	}
	
	public static void Center (Vector3 pos) {
		instances[0].StartCoroutine(instances[0].CenterHelper(pos));
	}
	
	public IEnumerator CenterHelper (Vector3 pos) {
		yield return null;
		yield return null;
		//Debug.Log("00");
		bool done = false;
		for(int i = 0; i < instances.Count && !done; i++) {
			if(instances[i].closeCount > 0 && instances[i].centerable) {
				//Debug.Log("000");
				done = true;
				instances[i].InstanceCenter(pos);
			}
		}
	}
	
	public void InstanceCenter (Vector3 pos) {
		float dist = 100000000000;
		int idx = -1;
		int idy = -1;
		int idz = -1;
	
		Transform tt = Camera.main.transform;
	
		for(int x = 0; x < dimensions.x; x++) {
			for(int z = 0; z < dimensions.z; z++) {
				for(int y = 0; y < dimensions.y; y++) {
					Molecule m = molecules[x,z,y];
					if(m.trans) {
						float di = (new Vector3(m.trans.position.x - pos.x, 0, m.trans.position.z - pos.z)).sqrMagnitude;
						if(di < dist) {
							dist = di;
							idx = x;
							idy = y;
							idz = z;
						}
					}
				}
			}
		}
		//Debug.Log("was");
		if(idx != -1) {
			molecules[idx,idz,idy].trans.position = molecules[idx,idz,idy].refPos = molecules[idx,idz,idy].worldPosition = new Vector3(pos.x, molecules[idx,idz,idy].trans.position.y, pos.z);
			molecules[idx,idz,idy].alwaysHit = true;
			//molecules[idx,idz,idy].gameObject.name = "__LULZ";
			//Debug.Log("__LULZ");
		}
	}
	
	public static void Collide(Vector3 start, Vector3 end) {
		for(int i = 0; i < instances.Count; i++) {
			instances[i].InstanceCollide(start, end);
		}
	}
	
	float lstSpeedScale = 1;
	
	public int closeCount = 0;
	
	public void InstanceCollide (Vector3 start, Vector3 end) {
		
		if(lstSpeedScale > 2) {
			lstSpeedScale = ProjectileDriver.speedScale;
			return;
		} else {
			lstSpeedScale = ProjectileDriver.speedScale;
		}
		closeCount = 0;
		for(int x = 0; x < dimensions.x; x++) {
			for(int z = 0; z < dimensions.z; z++) {
				for(int y = 0; y < dimensions.y; y++) {
					Molecule m = molecules[x,z,y];
					if(m.trans) {
						//Debug.DrawLine(m.trans.TransformPoint(localStart), m.trans.TransformPoint(localStart + (localEnd-localStart)*5), Color.white);
						//Debug.DrawLine(m.trans.TransformPoint(m.boundingBox.min), m.trans.TransformPoint(m.boundingBox.max), m.collided ? Color.green : Color.yellow);
						if(m.collided < 0 && (m.worldPosition-end).sqrMagnitude < 0.0009f) { // 0.03 * 0.03
							closeCount ++;
							Vector3 localStart = m.trans.InverseTransformPoint(new Vector3(start.x, m.trans.position.y, start.z));
							Vector3 localEnd   = m.trans.InverseTransformPoint(new Vector3(end.x,   m.trans.position.y, end.z  ));

							float dist = 1000;
							if(m.boundingBox.IntersectRay(new Ray(localStart, localEnd-localStart), out dist) && dist < (localEnd-localStart).magnitude * 5) {
								
								float ct = m.interactions*ProjectileDriver.chanceToInteract;
								m.collided = Random.Range(0.4f, 0.5f)/Mathf.Max(1,(ct*0.03f));
								bool gammaHit = (ProjectileDriver.instance.playbackMode == 1);
								if((gammaHit || ProjectileDriver.instance.ptype != ProjectileDriver.ProjectileType.G) && (ct > Random.value*100 || m.alwaysHit)) {
									m.gammaHit = gammaHit;
									m.StartCoroutine(m.Fragment(localStart, m.trans.InverseTransformDirection(ProjectileDriver.trans.forward), ProjectileDriver.speedometer));
									m.alwaysHit = false;
									//Debug.Break ();
									//Debug.Log("HIT!" + (int)Time.time);
									ProjectileDriver.updateEnergy = true;
								}
							}
						}
					}
				}
			}
		}
	}
	
	Vector3 SuddenlyTetrahedrons (int x, int y, int z) {
		float xf = x; float yf = y; float zf = z;
		int seed = UnityEngine.Random.seed;
		UnityEngine.Random.seed = x*100+y*10+z +(x*y*z%93);
		Vector3 res = new Vector3(xf+(((z+y)&1)==1 ? 0.5f : 0), yf*0.816496581f, zf*0.866025404f + yf*0.288675135f) + UnityEngine.Random.insideUnitSphere*randomFac;
		UnityEngine.Random.seed = seed;
		return res;
	}
}
