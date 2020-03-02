using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoleculeDisplay : MonoBehaviour {

	public class Atom {
		public Vector3 position;
		public float size;
	}

	public Material[] materials;

	private Mesh mesh;
	private Atom[] myAtoms;

	void Start () {
		if(transform.childCount == 0) {
			var o = new GameObject("MoleculeDisplay ");
			var mf = o.AddComponent<MeshFilter>();
			var r = o.AddComponent<MeshRenderer>();
			r.materials = materials;
			mesh = new Mesh();
			mf.mesh = mesh;
			o.transform.parent = transform;
			o.transform.localPosition = Vector3.zero;
			o.transform.localRotation = Quaternion.identity;
		} else {
			mesh = transform.GetChild(0).GetComponent<MeshFilter>().mesh;
		}
		
		myAtoms = new Atom[0];
	}

	private Quaternion lastRotation = Quaternion.identity;
	private Boolean lastRotationIsSet = false;
	void Update() {
		if(lastRotationIsSet && Quaternion.Angle(transform.rotation, lastRotation) > 10) {
			RebuildMesh(myAtoms);
			lastRotation = transform.rotation;
			lastRotationIsSet = true;
		}

		if(!lastRotationIsSet) {
			lastRotation = transform.rotation;
			lastRotationIsSet = true;
		}
		
	}

	public Atom[] GetAtoms() {
		return myAtoms;
	}

	public void SetAtoms(Atom[] atoms) {
		myAtoms = atoms;
		RebuildMesh(myAtoms);
	}

	private void RebuildMesh (Atom[] atoms) {
		if(mesh == null) {
			return;
		}


		var vertexColors = new Color[atoms.Length*4];
		var vertices = new Vector3[atoms.Length*4];
		var uvs = new Vector2[atoms.Length*4];
		var triangles = new int[atoms.Length*6];

		//  0----1
		//  |  / |
		//  | /  |
		//  2----3

		for(var i = 0; i < atoms.Length; i++) {
			var vi = i * 4;
			var ti = i * 6;

			vertices[vi  ] = atoms[i].position + transform.InverseTransformDirection(new Vector3(-1, 0, 1)) * atoms[i].size;
			vertices[vi+1] = atoms[i].position + transform.InverseTransformDirection(new Vector3( 1, 0, 1)) * atoms[i].size;
			vertices[vi+2] = atoms[i].position + transform.InverseTransformDirection(new Vector3(-1, 0,-1)) * atoms[i].size;
			vertices[vi+3] = atoms[i].position + transform.InverseTransformDirection(new Vector3( 1, 0,-1)) * atoms[i].size;

			uvs[vi  ] = new Vector2(0, 1);
			uvs[vi+1] = new Vector2(1, 1);
			uvs[vi+2] = new Vector2(0, 0);
			uvs[vi+3] = new Vector2(1, 0);

			vertexColors[vi  ] = Color.white;
			vertexColors[vi+1] = Color.white;
			vertexColors[vi+2] = Color.white;
			vertexColors[vi+3] = Color.white;

			triangles[ti  ] = vi  ;
			triangles[ti+1] = vi+1;
			triangles[ti+2] = vi+2;

			triangles[ti+3] = vi+1;
			triangles[ti+4] = vi+3;
			triangles[ti+5] = vi+2;
			
		}

		mesh.Clear();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.colors = vertexColors;
		mesh.triangles = triangles;


	}
}
