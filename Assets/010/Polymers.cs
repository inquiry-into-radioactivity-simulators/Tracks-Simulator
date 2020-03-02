using UnityEngine;
using System.Collections;

public class Polymers : Molecules {

	public Transform woodTransform;
	public Transform cupTransform;

	public int polymerLength = 10;

	public override void SetMolecule(Molecule m, Vector3 pos) {
		Vector3 p = transform.TransformPoint(pos);
		MaterialZones.SolidMaterial check = MaterialZones.Check(p);
		if(check == MaterialZones.SolidMaterial.Plastic && typeInstances[1]) {
			Vector3 dir = (cupTransform.position - p).normalized+Random.insideUnitSphere*0.25f;
			Vector3 cross = Vector3.Cross(Vector3.up, dir);
			Vector3 lookUp = cross*(Random.value-0.5f) + Vector3.Cross(Vector3.up, cross)*(Random.value-0.5f);
			m.Reset(pos-Vector3.up*0.4f, Quaternion.LookRotation(dir, lookUp)*Quaternion.Euler(Random.value*360,0,0), this, 1);
		} else if(check == MaterialZones.SolidMaterial.Wood && typeInstances[0]) {
			Vector3 dir = woodTransform.forward+Random.insideUnitSphere*0.4f;
			m.Reset(pos, Quaternion.LookRotation(dir)*Quaternion.Euler((Random.value-0.5f)*70-90,90,0), this, 0);
		}
	}
	
	public override void FirstTimeFilter(Molecule m) {
		float min = 100000;
		float max = -100000;
	
		int minIndex = 0;
		int maxIndex = 0;
	
		for(int i = 0; i < m.atoms.Length; i++) {
			if(m.atoms[i].pos.x < min) {
				min = m.atoms[i].pos.x;
				minIndex = i;
			}
			if(m.atoms[i].pos.x > max) {
				max = m.atoms[i].pos.x;
				maxIndex = i;
			}
		}
		
		Atom[] newAtoms = new Atom[m.atoms.Length-1];
		for(int i = 0; i < newAtoms.Length; i++) {
			int index = i >= maxIndex ? i+1 : i;
			Atom a = new Atom();
			a.pos = m.atoms[index].pos;
			a.bonded = m.atoms[index].bonded;
			a.e = m.atoms[index].e;
			newAtoms[i] = a;
			/*
			newAtoms[i] = ;
			int len = newAtoms[i].bonded.Length;
			int removeAt = -1;
			for(int j = 0; j < newAtoms[i].bonded.Length; j++) {
				if(newAtoms[i].bonded[j] == maxIndex) {
					removeAt = j;
					len--;
				}
			}
			int[] newBonded = new int[len];
			for(int j = 0; j < len; j++) {
				newBonded[j] = newAtoms[i].bonded[j >= removeAt ? j+1 : j];
				if(newBonded[j] >= maxIndex) {
					newBonded[j]--;
				}
			}
			
			newAtoms[i].bonded = newBonded;
			*/
		}
		
		float d = max-min;
		m.atoms = new Atom[newAtoms.Length*polymerLength];
		for(int j = 0; j < polymerLength; j++) {
			Vector3 pos = Vector3.right * d * ((float)j - (float)polymerLength*0.5f);
			
			for(int i = 0; i < newAtoms.Length; i++) {
				Atom a = new Atom();
				a.pos = newAtoms[i].pos+pos;
				int[] newNieghbors = new int[newAtoms[i].bonded.Length];
				for(int k = 0; k < newNieghbors.Length; k++) {
					int b = newAtoms[i].bonded[k];
					if(b == maxIndex) b = (j < polymerLength-1 ? newAtoms.Length + minIndex : minIndex );
					if(b > maxIndex) b--;
					newNieghbors[k] = b + j*newAtoms.Length;
				}
				a.bonded = newNieghbors;
				a.e = newAtoms[i].e;
				m.atoms[i + j*newAtoms.Length] = a;
			}
		}
	}
}
