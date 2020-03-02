using UnityEngine;
using System.Collections;
using System.Collections.Generic;
	public class LipidShell { 
		public LipidShell (int segNum) {
			segments = new int[segNum];
		}
		public int[] segments;
	}
	
public class Lipids : Molecules {
	
	int numberOfSegments = 900;
	float numberOfSegmentsRecip = 0.00111111111f;
	
	public Dictionary<Vector3i, LipidShell> shells = new Dictionary<Vector3i, LipidShell>();
	
	public static Lipids lipidsInstance;
	


	public override void OnEnable() {
		base.OnEnable();
		lipidsInstance = this;
	}
	
	public override void OnDisable() {
		base.OnDisable();
		lipidsInstance = null;
	}
	
	public override void FirstTimeFilter(Molecule m) {
		float min = 100000;
		float max = -100000;

		for(int i = 0; i < m.atoms.Length; i++) {
			if(m.atoms[i].pos.x < min) {
				min = m.atoms[i].pos.x;
			}
			if(m.atoms[i].pos.x > max) {
				max = m.atoms[i].pos.x;
			}
		}
		
		Atom[] newAtoms = new Atom[m.atoms.Length*2];
		for(int i = 0; i < m.atoms.Length; i++) {
			Atom a = new Atom();
			a.pos = m.atoms[i].pos;
			a.bonded = m.atoms[i].bonded;
			a.e = m.atoms[i].e;
			newAtoms[i] = a;
			
			Atom a1 = new Atom();
			a1.pos = new Vector3((max-min)-m.atoms[i].pos.x, m.atoms[i].pos.y, m.atoms[i].pos.z);
			a1.bonded = m.atoms[i].bonded;
			for(int ii = 0; ii < a1.bonded.Length; ii++) {
				a1.bonded[ii] += m.atoms.Length;
			}
			a1.e = m.atoms[i].e;
			newAtoms[m.atoms.Length + i] = a1;
			
		}
		
		m.atoms = newAtoms;
	}
	
	public override void SetMolecule(Molecule m, Vector3 pos) {
		
		Vector3 wPos = transform.TransformPoint(pos);
		MaterialZones.SolidMaterial check = MaterialZones.Check(wPos);
		if(check == MaterialZones.SolidMaterial.Flesh || check == MaterialZones.SolidMaterial.Epidermis) {
			Vector3 cellPos = MaterialZones.i.NearestTet(wPos, check == MaterialZones.SolidMaterial.Epidermis);
			float angle = Mathf.Atan2(wPos.z-cellPos.z, wPos.x-cellPos.x)* Mathf.Rad2Deg;
			if(angle < 0) angle += 360;
			Vector3i cell = new Vector3i(cellPos, MaterialZones.cellScaleRecip);
			LipidShell theCell = null; shells.TryGetValue(cell, out theCell);
			if(theCell == null) {
				//Debug.Log("adding (" + cell.x + ", " + cell.y + ", "+ cell.z +").   - "+ cellPos);
				theCell = new LipidShell(numberOfSegments);
				shells[cell] = theCell;
			}
			float segAngle = angle*0.00277777778f*(float)numberOfSegments;
			int segId = Mathf.FloorToInt(segAngle);
			if(theCell.segments[segId] < 1) {
				theCell.segments[segId] ++;
				float angleInc = ((float)segId)*numberOfSegmentsRecip*360*Mathf.Deg2Rad;
				Vector3 rVec = new Vector3(Mathf.Cos(angleInc), 0, Mathf.Sin(angleInc)); // 
				Vector3 lPos = transform.InverseTransformPoint(cellPos + rVec * (MaterialZones.cellScale*0.5f-MaterialZones.cellMembrane*1.2f));
				m.Reset(lPos, Quaternion.LookRotation(Vector3.Cross(Vector3.up, rVec)), this, 0);
			}
		}
	}
	
	public void SetHelper (Vector3 pos) {
		MaterialZones.SolidMaterial check = MaterialZones.Check(pos);
		Vector3 wPos = transform.TransformPoint(pos);
		Vector3 cellPos = MaterialZones.i.NearestTet(wPos, check == MaterialZones.SolidMaterial.Epidermis);
		float angle = Mathf.Atan2(wPos.z-cellPos.z, wPos.x-cellPos.x)* Mathf.Rad2Deg;
		if(angle < 0) angle += 360;
		Vector3i cell = new Vector3i(cellPos, MaterialZones.cellScaleRecip);
		LipidShell theCell = null; shells.TryGetValue(cell, out theCell);
		if(theCell == null) {
			//Debug.Log("adding (" + cell.x + ", " + cell.y + ", "+ cell.z +").   - "+ cellPos);
			theCell = new LipidShell(numberOfSegments);
			shells[cell] = theCell;
		}
		float segAngle = angle*0.00277777778f*(float)numberOfSegments;
		int segId = Mathf.FloorToInt(segAngle);
		if(theCell.segments[segId] < 1) {
			theCell.segments[segId] ++;
		}
	}
}
