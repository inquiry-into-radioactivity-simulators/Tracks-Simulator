using UnityEngine;
using System.Collections;

public class Liquids : Molecules {
	
	public float rotate = 10f;
	public bool liquid = true;

	
	public override void SetMolecule(Molecule m, Vector3 pos) {
		Vector3 wPos = transform.TransformPoint(pos);
		MaterialZones.SolidMaterial check = MaterialZones.Check(wPos);
		if((liquid && (check == MaterialZones.SolidMaterial.Soda || ((check == MaterialZones.SolidMaterial.Flesh || check == MaterialZones.SolidMaterial.Epidermis) && !MaterialZones.i.CellwallArea(wPos, check)))) || (!liquid && check == MaterialZones.SolidMaterial.Steel)) {
			
			Vector3 direction = pos*rotate;
			if(!liquid) direction = new Vector3(0,(Mathf.Abs(direction.x)+Mathf.Abs(direction.y)+Mathf.Abs(direction.z))*0.3f,0);
			m.Reset(pos, Quaternion.Euler(direction), this, 0);
		}
	}
}
