using UnityEngine;
using System; 
using System.Collections.Generic;

public struct UVertex {
	public Vector3 pos;
	public Vector3 normal;
	public HSBColor color;
	public int[] attached;
	public int[] nieghbors; 
	public int[] tris;
	public int state;
}

public struct Line : IEquatable<Line> {
	public int i;
	public int a;
	public int b;
	public List<int> triangles;

	public Line (int sa, int sb) {
		i = 0;
		a = sa; b = sb;
		triangles = new List<int>(2);
	}

	public override bool Equals(object obj) {
		return Equals((Line)obj);
	}
	public bool Equals(Line other) {
		if((other.a == a && other.b == b) || (other.b == a && other.a == b)) return true;
		return false;
	}

	public override int GetHashCode() {
		long tmp = (a^b);
		long msw = ((tmp >> 32) & 0xFFFFFFFF);
		long lsw = tmp & 0xFFFFFFFF;
		return (int)(msw ^ lsw);
	}
}

public struct MyTri {
	public int i0;
	public int i1;
	public int i2;
	public Vector3 normal;	
}

public class Vector3i : IEquatable<Vector3i> {
	public int x,y,z;
	public Vector3i (Vector3 src, int mult) {
		x = (int)(src.x*mult);
		y = (int)(src.y*mult);
		z = (int)(src.z*mult);
	}
	public Vector3i (Vector3 src, float multf) {
		x = (int)(src.x*multf);
		y = (int)(src.y*multf);
		z = (int)(src.z*multf);
	}
	public override bool Equals(object obj) {
		return Equals(obj as Vector3i);
	}
	public bool Equals(Vector3i other) {
		if(other == null) return false;
		if(x != other.x || y != other.y || z != other.z) return false;
		return true;
	}
	public override int GetHashCode() {
		long tmp = (x ^ y ^ z);
		long msw = ((tmp >> 32) & 0xFFFFFFFF);
		long lsw = tmp & 0xFFFFFFFF;
		return (int)(msw ^ lsw);
	}
}

public class MeshEditor {
	
	public float minVertexDist = 0.21f;
	public Mesh loadedFrom;
	public bool closedMesh;
	
	public UVertex[] uverts;
	public int[] vertToUVert;
	public Line[] lines;
	public int[] triangles;
	public MyTri[] tris;
	public Vector3[] vertices;
	public Vector2[] uv;
	
	
	
	public void LoadFrom (Mesh source) {
		loadedFrom = source;
		vertices = source.vertices;
		uv = source.uv;
		triangles = source.triangles;
		
		// detect vertices that are in the same position but have different uvs, normals or something
		Dictionary<Vector3i,List<int>> vertexHash = new Dictionary<Vector3i,List<int>>(vertices.Length);
		vertToUVert = new int[vertices.Length];
		int vertexHashCount = 0;
		int mult = (int)(1.0f/minVertexDist);
		for(int i = 0; i < vertices.Length; i++) {
			Vector3i vi = new Vector3i(vertices[i], mult);
			if(!vertexHash.ContainsKey(vi)) {
				vertexHash[vi] = new List<int>(3);
				vertexHash[vi].Add(vertexHashCount);
				vertexHashCount++;
			}
			vertexHash[vi].Add(i);
		}
		
		HSBColor white = HSBColor.FromColor(Color.white);
		uverts = new UVertex[vertexHashCount];
		foreach(Vector3i p in vertexHash.Keys) {
			UVertex pv = new UVertex();
			pv.color = white;
			pv.attached = new int[vertexHash[p].Count-1];
			for(int i = 1; i < vertexHash[p].Count; i++) {
				int vi = vertexHash[p][i];
				pv.attached[i-1] = vi;
				vertToUVert[vi] = vertexHash[p][0];
			}
			pv.pos = vertices[pv.attached[0]];
			uverts[vertexHash[p][0]] = pv;
		}
		
		// generate a new triangle list that indexes into the new "positionally unique" vertex list
		for(int i = 0; i < triangles.Length; i++) {
			triangles[i] = vertToUVert[triangles[i]];
		}
		
		tris = new MyTri[(int)(((float)triangles.Length) / 3f)];
		for(int i = 0; i < tris.Length; i++) {
			MyTri myTri = new MyTri();
			myTri.i0 = triangles[i*3  ];
			myTri.i1 = triangles[i*3+1];
			myTri.i2 = triangles[i*3+2];
			myTri.normal = Vector3.Cross(uverts[myTri.i1].pos - uverts[myTri.i0].pos, uverts[myTri.i2].pos - uverts[myTri.i0].pos).normalized;
			tris[i] = myTri;
		}
		
		// and the same for lines
		int lineHashCount = 0;
		Dictionary<Line,Line> lineHash = new Dictionary<Line,Line>((int)(vertices.Length*0.5)); 
		for(int i = 0; i < tris.Length; i++) {
			MyTri myTri = tris[i];
			Line l0 = new Line(myTri.i0, myTri.i1);
			Line l1 = new Line(myTri.i1, myTri.i2);
			Line l2 = new Line(myTri.i2, myTri.i0);
			if(!lineHash.ContainsKey(l0) && myTri.i0 != myTri.i1) {
				l0.i = lineHashCount;
				lineHash[l0] = l0;
				lineHashCount ++;
				lineHash[l0].triangles.Add(i);
			}
			if(!lineHash.ContainsKey(l1) && myTri.i1 != myTri.i2) {
				l1.i = lineHashCount;
				lineHash[l1] = l1;
				lineHashCount ++;
				lineHash[l1].triangles.Add(i);
			}
			if(!lineHash.ContainsKey(l2) && myTri.i2 != myTri.i0) {
				l2.i = lineHashCount;
				lineHash[l2] = l2;
				lineHashCount ++;
				lineHash[l2].triangles.Add(i);
			}
		}
		
		lines = new Line[lineHashCount];
		foreach(Line line in lineHash.Keys) {
			lines[line.i] = line;
			Debug.DrawLine(uverts[line.a].pos, uverts[line.b].pos);
		}
		
		// generate nieghbor references on vertices
		for(int i = 0; i < lines.Length; i++) { 
			uverts[lines[i].a].state++;
			uverts[lines[i].b].state++;
		}
		closedMesh = true;
		for(int i = 0; i < uverts.Length; i++) {
			uverts[i].nieghbors = new int[uverts[i].state];
			if(uverts[i].state < 3) closedMesh = false;
			uverts[i].state = 0;
		}
		for(int i = 0; i < lines.Length; i++) {
			UVertex a = uverts[lines[i].a];
			UVertex b = uverts[lines[i].b];
			a.nieghbors[a.state] = lines[i].b;
			b.nieghbors[b.state] = lines[i].a;
			uverts[lines[i].a].state++;
			uverts[lines[i].b].state++;
		}
		
		// generate references from vertices to triangles
		for(int i = 0; i < uverts.Length; i++) {
			uverts[i].state = 0;
		}
		for(int i = 0; i < tris.Length; i++) {
			uverts[tris[i].i0].state++;
			uverts[tris[i].i1].state++;
			uverts[tris[i].i2].state++;
		}
		for(int i = 0; i < uverts.Length; i++) {
			uverts[i].tris = new int[uverts[i].state];
			uverts[i].state = 0;
		}
		for(int i = 0; i < tris.Length; i++) {
			int i0 = tris[i].i0;
			int i1 = tris[i].i1;
			int i2 = tris[i].i2;
			uverts[i0].tris[uverts[i0].state] = i;
			uverts[i0].state++;
			uverts[i1].tris[uverts[i1].state] = i;
			uverts[i1].state++;
			uverts[i2].tris[uverts[i2].state] = i;
			uverts[i2].state++;
		}

		
		/*
		for(int i = 0; i < triangles.Length; i+=3) {
			Debug.DrawLine(uverts[triangles[i  ]].pos, uverts[triangles[i+1]].pos);
			Debug.DrawLine(uverts[triangles[i+1]].pos, uverts[triangles[i+2]].pos);
			Debug.DrawLine(uverts[triangles[i+2]].pos, uverts[triangles[i  ]].pos);
		}
		
		// nieghbors
		for(int i = 0; i < uverts.Length; i++) {
			foreach(int n in uverts[i].nieghbors) {
				Debug.DrawLine(uverts[i].pos, uverts[n].pos, new Color(uverts[i].pos.x*0.5f+0.5f, uverts[i].pos.y*0.5f+0.5f, uverts[i].pos.z*0.5f+0.5f, 1f));
			}
		}
		
		// vertex to triangle references
		for(int i = 0; i < uverts.Length; i++) {
			for(int vti = 0; vti < uverts[i].tris.Length; vti++) {
				MyTri t = tris[uverts[i].tris[vti]];
				Color color = new Color(uverts[i].pos.x*0.5f+0.5f, uverts[i].pos.y*0.5f+0.5f, uverts[i].pos.z*0.5f+0.5f, 1f); 
				Debug.DrawLine(uverts[t.i0].pos, uverts[t.i1].pos, color);
				Debug.DrawLine(uverts[t.i1].pos, uverts[t.i2].pos, color);
				Debug.DrawLine(uverts[t.i2].pos, uverts[t.i0].pos, color);
			}
		}
		
		
		for(int i = 0; i < lines.Length; i++) {
			Color color1 = new Color(uverts[lines[i].a].pos.x*0.5f+0.5f, uverts[lines[i].a].pos.y*0.5f+0.5f, uverts[lines[i].a].pos.z*0.5f+0.5f, 1f);
			Color color2 = new Color(uverts[lines[i].b].pos.x*0.5f+0.5f, uverts[lines[i].b].pos.y*0.5f+0.5f, uverts[lines[i].b].pos.z*0.5f+0.5f, 1f);
			Debug.DrawLine(uverts[lines[i].a].pos, uverts[lines[i].b].pos, Color.Lerp(color1,color2,0.5f));
		}
		
		for(int i = 0; i < tris.Length; i++) {
			Debug.DrawLine(uverts[tris[i].i0].pos, uverts[tris[i].i1].pos);
			Debug.DrawLine(uverts[tris[i].i1].pos, uverts[tris[i].i2].pos);
			Debug.DrawLine(uverts[tris[i].i2].pos, uverts[tris[i].i0].pos);
		}
		
		for(int i = 0; i < uverts.Length; i++) {
			for(int vti = 0; vti < uverts[i].tris.Length; vti++) {
				MyTri t = tris[uverts[i].tris[vti]];
				Color color = new Color(uverts[i].pos.x*0.5f+0.5f, uverts[i].pos.y*0.5f+0.5f, uverts[i].pos.z*0.5f+0.5f, 1f); 
				Debug.DrawLine(uverts[t.i0].pos, uverts[t.i1].pos, color);
				Debug.DrawLine(uverts[t.i1].pos, uverts[t.i2].pos, color);
				Debug.DrawLine(uverts[t.i2].pos, uverts[t.i0].pos, color);
			}
		}
		*/
	}
	
	public void ClearAllButShape (Mesh target) {
		if(loadedFrom != target) LoadFrom(target);
		Vector3[] newVerts = new Vector3[uverts.Length];
		for(int i = 0; i < newVerts.Length; i++) {
			newVerts[i] = uverts[i].pos;
		}
		target.Clear();
		target.vertices = newVerts;
		target.triangles = triangles;
	}
	
	public Mesh BakeResultNoSeams () {
		Mesh m = new Mesh();
		Vector3[] vs = new Vector3[uverts.Length];
		Color[] cs = new Color[uverts.Length];
		Vector3[] ns = new Vector3[uverts.Length];
		int[] ts = new int[tris.Length*3];
		
		for(int i = 0; i < uverts.Length; i++) {
			vs[i] = uverts[i].pos;
			cs[i] = HSBColor.ToColor(uverts[i].color);
			ns[i] = uverts[i].normal;
		}
		
		for(int i = 0; i < tris.Length; i++) {
			ts[i*3  ] = tris[i].i0;
			ts[i*3+1] = tris[i].i1;
			ts[i*3+2] = tris[i].i2;
		}
		
		m.vertices = vs;
		m.colors = cs;
		m.normals = ns;
		m.triangles = ts;
		return m;
	}
}

