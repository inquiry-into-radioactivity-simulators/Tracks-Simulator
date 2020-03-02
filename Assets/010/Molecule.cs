using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum Element { H, O, N, C, P }

public class Atom {
	public int side = 0;
	public Vector3 pos;
	public Element e = Element.H;
	public int[] bonded;
}

public class Molecule : MonoBehaviour {
	public Atom[] atoms;

	public MoleculeDisplay display;
	
	public static float[] atomSizes = new float[] {0.025f, 0.060f, 0.065f, 0.070f, 0.11f};
	
	public float interactions = 1;
	public float atomCount = 0;
	public Vector3 worldPosition;
	public GameObject ionPrefab;
	public Bounds boundingBox = new Bounds();
	public float collided = 0;
	public Vector3 averagePos;
	bool virgin = true;
	public Transform trans;
	string name = "";
	
	public bool alwaysHit = false;
		public bool gammaHit = false;
	
	Vector3 velocity;
	Vector3 angVelocity;
	
	int myTypeIndex = 0;
	Molecules parentScript;
	
	public void FirstTime (Mesh source, int typeInt) {
		MeshEditor me = new MeshEditor();
		me.LoadFrom(source);
		name = source.name;
		
		float smallestLine = 10000;
		for(int i = 0; i < me.lines.Length; i ++) {
			float line = (me.uverts[me.lines[i].a].pos - me.uverts[me.lines[i].b].pos).magnitude;
			if(line > 0.09f && line < smallestLine) {
				smallestLine = line;
			}
		}
		
		atoms = new Atom[me.uverts.Length];
		
		for(int j = 1; j <= 5; j++) {
			for(int i = 0; i < me.uverts.Length; i ++) {
				if(me.uverts[i].nieghbors.Length == j || (j == 4 && me.uverts[i].nieghbors.Length > 4)) {
					Atom a = new Atom();
					a.bonded = me.uverts[i].nieghbors;
					a.pos = me.uverts[i].pos;
					if(me.uverts.Length == 2) {
						a.e = Molecules.currentDiatomic;
					} else if(source.name == "CO2") {
						if(me.uverts[i].nieghbors.Length == 2) {
							a.e = Element.C;
						} else {
							a.e = Element.O;
						}
					} else if(source.name == "metal") {
						a.e = Element.C;
						int len = me.uverts[i].nieghbors.Length;
						if(((int)((float)len*0.5f))*2 == len) a.e = Element.P;
					} else if(j == 1 && (me.uverts[me.uverts[i].nieghbors[0]].pos - me.uverts[i].pos).magnitude > smallestLine * 2 ){
						a.e = Element.O;
					} else {
						int bond = 0;
						for(int k = 0; k < j; k++) {
							Atom n = atoms[me.uverts[i].nieghbors[k]];
							if(n != null) {
								if(n.e == Element.O && me.uverts[me.uverts[i].nieghbors[k]].nieghbors.Length == 1)
									bond += 2;
								else
									bond ++;
							} else {
								bond ++;
							}
						}
						
						a.e = (Element)Mathf.RoundToInt(Mathf.Min(5,bond)-1);
						if(a.e == Element.P && (me.uverts[me.uverts[i].nieghbors[0]].pos - me.uverts[i].pos).sqrMagnitude < 1.5f) a.e = Element.C;
					}
					
					atoms[i] = a;
				}
			}
		}
		trans = transform;
	}
	
	public GameObject electronPrefab;
	
	Vector3 hitPos;
	Vector3 rightDirection;
	Vector3 avg0;
	int[] propagated;
	Atom[] newAtoms;
		
	public IEnumerator Fragment (Vector3 pos, Vector3 dir, float speed) {
		Molecule m1 = Instantiate(this) as Molecule;
		Molecule m2 = Instantiate(this) as Molecule;
		m1.trans.parent = trans.parent;
		m2.trans.parent = trans.parent;
		m1.trans.localScale = trans.localScale;
		m2.trans.localScale = trans.localScale;
		m1.trans.GetChild(0).GetComponent<Renderer>().enabled = false;
		m2.trans.GetChild(0).GetComponent<Renderer>().enabled = false;
		//Debug.Log((m1.averagePos * 1000) + ", " + (averagePos * 1000));

		float spacingScale = 0.028f;
		float atomScale = 0.5f;
		
		newAtoms = new Atom[atoms.Length];
		avg0 = Vector3.zero;
		int len0 = 0;
		for(int i = 0; i < atoms.Length; i++) {
			if(atoms[i] != null) {
				Atom a = new Atom();
				a.side = 0;
				a.pos = atoms[i].pos;
				a.e = atoms[i].e;
				a.bonded = atoms[i].bonded;
				newAtoms[i] = a;
				//if(parentScript == null) Debug.Log("BUH");
				avg0 += a.pos*spacingScale*parentScript.atomSpacingFudge;
				len0++;
			}
		}
		if(len0 != 0) avg0/=len0;
		
		Vector3 localWorldUp = trans.InverseTransformDirection(Vector3.up);
		rightDirection = Vector3.Cross(localWorldUp, dir);
		
		float dist = 1000000;
		int id = -1;
		//Debug.DrawLine(trans.TransformPoint(pos), trans.TransformPoint(pos+dir*100), Color.green);
		for(int i = 0; i < newAtoms.Length; i++) {
			if(newAtoms[i] != null) {
				Vector3 apos = newAtoms[i].pos*spacingScale*parentScript.atomSpacingFudge-avg0;
				Vector3 proj = Vector3.Project(apos - pos, dir);
				Vector3 diff = Vector3.Project(apos - (pos+proj), rightDirection);
				float mag = diff.magnitude + Vector3.Project(apos-pos, dir).magnitude*0.2f;
				
				//Debug.DrawLine(trans.TransformPoint(pos+proj), trans.TransformPoint(pos+proj+diff), Color.yellow);
				
				if(mag < dist && Vector3.Dot(dir, apos-pos)>0) {
					dist = mag;
					id = i;
				}
			}
		}
		
		if(id != -1) {
			//Debug.DrawLine(trans.TransformPoint(pos), trans.TransformPoint(newAtoms[id].pos*spacingScale*parentScript.atomSpacingFudge-avg0), Color.yellow);

			float time = 0;
			if(speed != 0) {
				Vector3 d = pos - (newAtoms[id].pos*spacingScale*parentScript.atomSpacingFudge-avg0);
				d = Vector3.Project(d,dir);
				time = d.magnitude/speed;
			}
			if(time != 0) {
				collided += time;
				yield return new WaitForSeconds(time);
			}
			
			if(gammaHit) {
				if(ProjectileDriver.instance) ProjectileDriver.instance.PlaybackGammaHit();
			}
			
			//Debug.DrawLine(trans.TransformPoint(pos), trans.TransformPoint(newAtoms[id].pos*spacingScale*parentScript.atomSpacingFudge-avg0), Color.yellow);
			
			rightDirection = MaterialZones.lastCheck == MaterialZones.SolidMaterial.Plastic ? localWorldUp : Vector3.Cross(localWorldUp, dir);
			
			propagated = new int[5];
			hitPos = pos;
			props = 0;
			if(MaterialZones.lastCheck != MaterialZones.SolidMaterial.Steel) {
				yield return StartCoroutine(Propagate(id, 0, false));
			} else {
				propagated[0] = 100;
				propagated[1] = 100;
			}
			
			int index = -1;
			int min = 100000;
		
			//Debug.Log("propagated["+0+"]: " + propagated[0] );
			for(int i = 1; i < propagated.Length; i++) {
				//Debug.Log("propagated["+i+"]: " + propagated[i] );
				if(min > propagated[i] && propagated[i] != 0) {
					min = propagated[i];
					index = i;
				}
			}
			//Debug.Log("bumping those with side " + (index*2-1));
			if(propagated[index] == propagated[0] || propagated[0] == 3) {
				//Debug.Log(" " + propagated[index] + " == " + propagated[0]);
				int len = propagated[0];
				propagated = new int[5];
				bool once = true;
				for(int i = 0; i < newAtoms.Length; i++) {
					Atom cur = newAtoms[i];
					bool go = once && (cur.side & (1<<0)) != 0 && ((len == 3 && cur.e == Element.H) || (i == id && len != 3));
					cur.side = (1<<0) | ((go ? 1 : 0)<<1);
					if(go) once = false;
					propagated[0]++;
					propagated[Mathf.FloorToInt(((float)cur.side)*0.5f)+1] ++;
				}
				index = -1;
				min = 100000;
				for(int i = 1; i < propagated.Length; i++) {
					//Debug.Log("propagated["+i+"]: " + propagated[i] );
					if(min > propagated[i] && propagated[i] != 0) {
						min = propagated[i];
						index = i;
					}
				}
			}
			index = index*2-1;
			//Debug.Log("bumping those with side " + index);
			
			int[] to1 = new int[newAtoms.Length];
			int[] to2 = new int[newAtoms.Length];
			for(int i = 0; i < to1.Length; i++) { to1[i] = -1; }
			for(int i = 0; i < to2.Length; i++) { to2[i] = -1; }
			List<Atom> l1 = new List<Atom>(); int l1c = 0;
			List<Atom> l2 = new List<Atom>(); int l2c = 0;
			
			for(int i = 0; i < newAtoms.Length; i++) {
				if(newAtoms[i] != null) {
					//Debug.Log(" " + newAtoms[i].side + " == " + index);
					if(newAtoms[i].side == index) {
						l1.Add(newAtoms[i]);
						to1[i] = l1c;
						l1c++;
					} else {
						l2.Add(newAtoms[i]);
						to2[i] = l2c;
						l2c++;
					}
				}
			}
			//Vector3 avg1 = Vector3.zero; int len1 = 0;
			Atom[] a1 = new Atom[l1.Count]; l1c = 0;
			Atom[] a2 = new Atom[l2.Count]; l2c = 0;
			for(int i = 0; i < a1.Length; i++) {
				a1[i] = l1[i];
				//avg1+= a1[i].pos*spacingScale*parentScript.atomSpacingFudge-avg0;
				//len1++;
				List<int> newBonded = new List<int>();
				for(int ii = 0; ii < a1[i].bonded.Length; ii++) {
					if(a1[i].bonded[ii] < to1.Length) {
						int to1i = to1[a1[i].bonded[ii]];
						if(to1i != -1) {
							newBonded.Add(to1i);
						}
					}
				}
				int[] newBondedArr = new int[newBonded.Count];
				for(int ii = 0; ii < newBondedArr.Length; ii++) {
					newBondedArr[ii] = newBonded[ii];
				}
				a1[i].bonded = newBondedArr;
			}
			//if(len1 != 0) avg1/=len1;
			for(int i = 0; i < a2.Length; i++) {
				a2[i] = l2[i];
				List<int> newBonded = new List<int>();
				for(int ii = 0; ii < a2[i].bonded.Length; ii++) {
					if(a2[i].bonded[ii] < to2.Length) {
						int to2i = to2[a2[i].bonded[ii]];
						if(to2i != -1) {
							newBonded.Add(to2i);
						}
					}
				}
				int[] newBondedArr = new int[newBonded.Count];
				for(int ii = 0; ii < newBondedArr.Length; ii++) {
					newBondedArr[ii] = newBonded[ii];
				}
				a2[i].bonded = newBondedArr;
			}
			
			m1.Reset(trans.localPosition, trans.rotation, parentScript, a1);
			m2.Reset(trans.localPosition, trans.rotation, parentScript, a2);
			
			//Debug.DrawRay(ProjectileDriver.trans.position, ProjectileDriver.trans.right, Color.red);
			//Debug.DrawRay(m1.trans.position, (ProjectileDriver.trans.position-m1.trans.position), Color.green);
			
			if(ProjectileDriver.trans) {
				//Vector3 direction = ProjectileDriver.trans.right.normalized * (Vector3.Dot(ProjectileDriver.trans.right, m1.trans.position-ProjectileDriver.trans.position) > 0 ? 1 : -1);
				Vector3 direction = ProjectileDriver.trans.right.normalized * (Random.value > 0.5 ? 1 : -1);
				//Debug.DrawRay(ProjectileDriver.trans.position, direction, Color.yellow);
				
				Vector3 randomDir = Vector3.Scale(Random.onUnitSphere, new Vector3(1,0,1)).normalized;
				m1.velocity = (direction * 0.5f + randomDir * 0.2f) * Random.Range(0.0003f, 0.00045f) * (ShooterDriver.useParking ? 0.25f : 1f);
				m1.angVelocity = Random.onUnitSphere * Random.Range(10,30);
				m1.refPos = m1.trans.position;
				m1.delta = Vector3.zero;
			
				Vector3 ionizationPosition = atoms[id].pos * spacingScale*parentScript.atomSpacingFudge-avg0;
			
				GameObject posIon = Instantiate(ionPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				posIon.transform.parent = m1.trans;
				posIon.transform.localPosition = Vector3.zero;
				
				if(!gammaHit) {
					GameObject el = Instantiate(electronPrefab, trans.TransformPoint(ionizationPosition), Quaternion.identity) as GameObject;
					Mover ms = el.AddComponent<Mover>();
					ms.velocity = (-m1.velocity * 50 * (ShooterDriver.useParking ? 4f : 1f) + ProjectileDriver.trans.forward * Random.Range(0.0031f, 0.0043f)) * 0.12f;
				}
				m1.velocity += ProjectileDriver.trans.forward*Random.Range(0.00006f, 0.00009f)*(ShooterDriver.useParking ? 1f : 4f);
				
				if(MaterialZones.lastCheck == MaterialZones.SolidMaterial.Steel) {
					m1.angVelocity = m1.velocity = Vector3.zero;
					posIon.transform.position = trans.TransformPoint(newAtoms[id].pos*spacingScale*parentScript.atomSpacingFudge-avg0);
				}
			}
			if(trans.parent == Lipids.lipidsInstance) Lipids.lipidsInstance.SetHelper(trans.localPosition);
			refPos = trans.position = Vector3.zero;
			m1.gameObject.AddComponent<Cleaner>();
			m2.gameObject.AddComponent<Cleaner>();
			m1.trans.GetChild(0).GetComponent<Renderer>().enabled = true;
			m2.trans.GetChild(0).GetComponent<Renderer>().enabled = true;
		}
	}
	
	public void Reset (Vector3 pos, Quaternion rot, Molecules parent, Atom[] theAtoms) {
		parentScript = parent;
		trans = transform;
		trans.localPosition = pos;
		refPos = trans.position;
		trans.rotation = rot;
		atoms = theAtoms;

		MoleculeDisplay.Atom[] displayAtoms = new MoleculeDisplay.Atom[atoms.Length]; 
		Vector3 avg = averagePos;
		float spacingScale = 0.028f;
		float atomScale = 0.5f;
		Vector3 avg2 = Vector3.zero;
		int len = 0;
		Bounds box = new Bounds();
		for(int i = 0; i < atoms.Length; i++) {
			if(atoms[i] != null) {
				Vector3 p = atoms[i].pos*spacingScale*parent.atomSpacingFudge;
				if(i == 0) {
					box.center = p;
					box.size = Vector3.one*0.5f*spacingScale*parent.atomSpacingFudge;
				} else {
					box.Encapsulate (p);
				}
				avg2 += p;
				len ++;
			}
		}
		
		if(len != 0) avg2/= len;
		box.center -= avg2;
		box.size += Vector3.one*2*spacingScale*parent.atomSpacingFudge;
		for(int i = 0; i < atoms.Length; i++) {
			if(atoms[i] != null) {
				MoleculeDisplay.Atom a = new MoleculeDisplay.Atom();
				a.position = (atoms[i].pos * spacingScale*parent.atomSpacingFudge)-avg2;
				a.size = atomSizes[(int)atoms[i].e] * atomScale*parent.atomSizeFudge;
				displayAtoms[i] = a;
			}
		}
		display.SetAtoms(displayAtoms);
		boundingBox = box;
		atomCount = len;
		collided = 0;
		trans.position = trans.TransformPoint((avg2-avg)*parentScript.sizeScale);
		refPos = worldPosition = trans.position;
	}
	
	int props = 0;
	
	IEnumerator Propagate(int id, int depth, bool init) {
		Atom cur = newAtoms[id];
		float spacingScale = 0.028f;
		float atomScale = 0.5f;
		
		int currentlyRightSide = (Vector3.Dot(rightDirection, (cur.pos*spacingScale*parentScript.atomSpacingFudge-avg0) - (hitPos)) > 0 ? 1 : 0);
		int tracebackToRightSide = ((depth == 0 ? (currentlyRightSide == 1) : init) ? 1 : 0);
		
		//Debug.DrawRay(ProjectileDriver.trans.position, trans.TransformDirection(rightDirection).normalized, Color.red);
		//Debug.DrawLine(trans.TransformPoint(cur.pos*spacingScale*parentScript.atomSpacingFudge-avg0), trans.TransformPoint(hitPos), new Color( currentlyRightSide, tracebackToRightSide, 0));
		
		
		cur.side = (1<<0) | (currentlyRightSide<<1) | (tracebackToRightSide<<2);
		//Debug.Log(cur.side);

		propagated[0]++;
		propagated[Mathf.FloorToInt(((float)cur.side)*0.5f)+1] ++;
		

		
		for(int i = 0; i < cur.bonded.Length; i++) {
			if(cur.bonded[i] < newAtoms.Length) {
				Atom a = newAtoms[cur.bonded[i]];
				if(a != null && a.side == 0) {
					if(props < 8) {
						props ++;
						yield return StartCoroutine(Propagate(cur.bonded[i], depth+1, depth == 0 ? (Vector3.Dot(rightDirection, a.pos*spacingScale*parentScript.atomSpacingFudge - hitPos) > 0) : init));
					} else {
						StartCoroutine(Propagate(cur.bonded[i], depth+1, depth == 0 ? (Vector3.Dot(rightDirection, a.pos*spacingScale*parentScript.atomSpacingFudge - hitPos) > 0) : init));
					}
				}
			}
		}
	}

	public Vector3 refPos;
	
	public Vector3 delta = Vector3.zero;
	
	void Update () {
		collided -= Time.deltaTime;
		if(trans) {
			delta += velocity * 1000 * Time.deltaTime;
			trans.position = refPos + delta*0.001f;
			trans.Rotate(angVelocity * Time.deltaTime);
		}
	}
	
	public void Reset (Vector3 pos, Quaternion rot, Molecules parent, int moleculeType) {
		myTypeIndex = moleculeType;
		trans = transform;
		parentScript = parent;
		trans.localPosition = pos;
		refPos = worldPosition = trans.position;
		trans.rotation = rot;
		//Debug.Log(moleculeType + "        " + parent.typeInstances.Length);
		atoms = parent.typeInstances[moleculeType].atoms;
		
		// THIS MUST MATCH Cut() !!!!!!!!!
		MoleculeDisplay.Atom[] displayAtoms = new MoleculeDisplay.Atom[atoms.Length]; 
		Vector3 avg = Vector3.zero;
		float spacingScale = 0.028f;
		float atomScale = 0.5f;
		int len = 0;
		Bounds box = new Bounds();
		for(int i = 0; i < atoms.Length; i++) {
			if(atoms[i] != null) {
				Vector3 p = atoms[i].pos*spacingScale*parent.atomSpacingFudge;
				if(i == 0) {
					box.center = p;
					box.size = Vector3.one*0.5f*spacingScale*parent.atomSpacingFudge;
				} else {
					box.Encapsulate (p);
				}
				avg += p;
				len ++;
			}
		}
		
		avg /= len;
		box.center -= avg;
		box.size += Vector3.one*2*spacingScale*parent.atomSpacingFudge;
		for(int i = 0; i < atoms.Length; i++) {
			if(atoms[i] != null) {
				MoleculeDisplay.Atom a = new MoleculeDisplay.Atom();
				a.position = (atoms[i].pos * spacingScale*parent.atomSpacingFudge)-avg;
				a.size = atomSizes[(int)atoms[i].e] * atomScale*parent.atomSizeFudge;
				displayAtoms[i] = a;
			}
		}
		averagePos = avg;
		display.SetAtoms(displayAtoms);
		boundingBox = box;
		atomCount = len;
		collided = 0;
	}
}
