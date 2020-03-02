using UnityEngine;
using System.Collections;

public class Gasses : Molecules {
	public override void SetMolecule(Molecule m, Vector3 pos) {
		MaterialZones.SolidMaterial check = MaterialZones.Check(transform.TransformPoint(pos));
		if(check == MaterialZones.SolidMaterial.Air) {
			float v = Random.value;
			if(v < 0.00039f) {
				m.Reset(pos, Quaternion.LookRotation(Random.onUnitSphere), this, 2);
			} else {
				/*if(v < 0.01039f) {
					Molecules.currentDiatomic = Element.P;
				} else*/ if(v < 0.21f) {
					Molecules.currentDiatomic = Element.O;
				} else {
					Molecules.currentDiatomic = Element.N;
				}
				m.Reset(pos, Quaternion.LookRotation(Random.onUnitSphere), this, 0);
			}
		} else if(check == MaterialZones.SolidMaterial.Soda) {
			float v = Random.value;
			if(v < 0.2f) {
				m.Reset(pos, Quaternion.LookRotation(Random.onUnitSphere), this, 3);
			}
		} else if(check == MaterialZones.SolidMaterial.Flesh || check == MaterialZones.SolidMaterial.Epidermis) {
			float v = Random.value;
			if(v < 0.05f) {
				m.Reset(pos, Quaternion.LookRotation(Random.onUnitSphere), this, 3);
			} else if(v < 0.4f) {
				m.Reset(pos, Quaternion.LookRotation(Random.onUnitSphere), this, 4);
			}
			
		} else if(check == MaterialZones.SolidMaterial.Chamber) {
			Atom[] list = new Atom[1];
			Atom a = new Atom();
			a.pos = Vector3.zero;
			a.bonded = new int[0];
			a.e = (Random.value > 0.5f) ? Element.O : Element.H;
			list[0] = a;
		
			m.averagePos = Vector3.zero;
			m.Reset(pos, Quaternion.LookRotation(Random.onUnitSphere), this, list);
		}
	}
}
