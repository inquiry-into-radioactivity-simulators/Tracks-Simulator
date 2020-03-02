using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 

public class Cells : MonoBehaviour {
	class Cell {
		public Renderer outsideRend;
		public MeshFilter outsideMf;
		public Renderer insideRend;
		public MeshFilter insideMf;
	}
	
	public bool epidermis = false;
	bool currentlyResetting = false;
	bool firstTimeStart = true;
	
	public Mesh cellOutside;
	public Mesh cellNucleus;
	public Material cellMat1;
	public Material cellMat2;
	
	public float nucleusRand = 0.35f;
	public float nucScaleMin = 0.3f;
	public float nucScaleMax = 0.3f;
	public float cellExpand = 1f;
	public Vector3 dimensions;
	public Vector3 tesselationDimensions;
	public float randomFac = 0.35f;

	public Vector3 camPos;
	public Vector3 camPosOffset;
	public static Transform camT;
	public static Cells i;	
	public static Cells iEpi;	
	
	Vector3i position;
	Cell[,,] cells;

	public void Start () {
		if(firstTimeStart) {
			if(!epidermis) i = this;
			else		   iEpi = this;
			
			healthyColor1 = cellMat1.GetColor("_Color");
			healthyColor2 = cellMat1.GetColor("_RimColor");
			healthyColor3 = cellMat1.GetColor("_RimColor1");
			

			camT = ProjectileDriver.trans;
			if(camT) {
				camPos = camT.position+camPosOffset;
				//transform.position = new Vector3(0,0,0);
			}
			
		}
		
		position = GetPosition(camPos);
		
		if(cells != null && cells.Length > 0) {
			for(int x = 0; x < tesselationDimensions.x; x++) {
				for(int z = 0; z < tesselationDimensions.z; z++) {
					for(int y = 0; y < tesselationDimensions.y; y++) {
						SetCell (x,z,y, false);
					}
				}
			}
		} else {
			cells = new Cell[(int)tesselationDimensions.x, (int)tesselationDimensions.z, (int)tesselationDimensions.y];
			for(int x = 0; x < tesselationDimensions.x; x++) {
				for(int z = 0; z < tesselationDimensions.z; z++) {
					for(int y = 0; y < tesselationDimensions.y; y++) {
						Cell c = new Cell();
						GameObject outside = new GameObject("outside");
						outside.transform.parent = transform;
						outside.transform.localScale = Vector3.one;
						outside.layer = 8;
						c.outsideRend = outside.AddComponent<MeshRenderer>();
						c.outsideRend.material = cellMat1;
						c.outsideMf = outside.AddComponent<MeshFilter>();
						c.outsideMf.mesh = cellOutside;
						
						GameObject inside = new GameObject("inside");
						inside.transform.parent = outside.transform;
						inside.layer = 8;
						inside.transform.localScale = Vector3.one*Random.Range(nucScaleMin,nucScaleMax);
						c.insideRend = inside.AddComponent<MeshRenderer>();
						c.insideRend.material = cellMat2;
						c.insideMf = inside.AddComponent<MeshFilter>();
						c.insideMf.mesh = cellNucleus;
						
						cells[x,z,y] = c;
						SetMesh(x,z,y);
					}
				}
			}
		}
		firstTimeStart = false;
	}
	
	Vector3i GetPosition (Vector3 inPos) {
		return new Vector3i(Vector3.Scale(transform.InverseTransformPoint(inPos), new Vector3(1, 1.22474487f, 1.15470054f)), 1);
	}
	
	bool dirt = false;
	
	void Update () {
		if(SelectZoomLevelGUI.selectedButton != 1) {
			if(dirt) {
				camPos = Vector3.zero;
				position = GetPosition(camPos);
				for(int x = 0; x < tesselationDimensions.x; x++) {
					for(int z = 0; z < tesselationDimensions.z; z++) {
						for(int y = 0; y < tesselationDimensions.y; y++) {
							SetCell (x,z,y, true);
						}
					}
				}
				dirt = false;
			}
			return;
		}
		dirt = true;
		
		camT = ProjectileDriver.trans;
		if(camT) {
			camPos = camT.position+camPosOffset;
			//transform.position = new Vector3(0,0,0);
		} else {
			//transform.position = new Vector3(-1000,-1000,-1000);
		}
		Vector3i newPosition = GetPosition(camPos);
		if(new Vector3(newPosition.x - position.x, newPosition.y - position.y, newPosition.z - position.z).sqrMagnitude > 4) {
			Start();
		}
		if(newPosition.x-1 > position.x) newPosition.x = position.x+1;
		if(newPosition.x+1 < position.x) newPosition.x = position.x-1;
		if(newPosition.z-1 > position.z) newPosition.z = position.z+1;
		if(newPosition.z+1 < position.z) newPosition.z = position.z-1;
		if(newPosition != position) {
			int dx = newPosition.x-position.x;
			int dz = newPosition.z-position.z;
			position = newPosition;
			/*
			Cell[,,] newCells = new Cell[(int)dimensions.x, (int)dimensions.z, (int)dimensions.y];
			for(int x = 0; x < dimensions.x; x++) {
				for(int z = 0; z < dimensions.z; z++) {
					int nx = x+dx; if(nx >= dimensions.x) nx = 0; if(nx < 0) nx = (int)dimensions.x-1;
					int nz = z+dz; if(nz >= dimensions.z) nz = 0; if(nz < 0) nz = (int)dimensions.z-1;
					for(int y = 0; y < dimensions.y; y++) {
						newCells[x,z,y] = cells[nx,nz,y];
					}
				}
			}
			cells = newCells;
			*/
			int redoCol = dx == 0 ? -1 : (dx > 0 ? (int)dimensions.x-1 : 0);
			int redoRow = dz == 0 ? -1 : (dz > 0 ? (int)dimensions.z-1 : 0);
			if(redoCol != -1) {
				for(int z = 0; z < dimensions.z; z++) {
					for(int y = 0; y < dimensions.y; y++) {
						SetCell(redoCol, z, y, false);
					}
				}
			}
			if(redoRow != -1) {
				for(int x = 0; x < dimensions.x; x++) {
					for(int y = 0; y < dimensions.y; y++) {
						SetCell(x, redoRow, y, false);
					}
				}
			}
		}
	}
	
	public void SetCell (int x, int z, int y, bool always) {
		Vector3 p = SuddenlyTetrahedrons(x+position.x, y+position.y, z+position.z, false, false);
		MaterialZones.SolidMaterial check = MaterialZones.Check(transform.TransformPoint(p));
		if(((check != MaterialZones.SolidMaterial.Flesh && !epidermis) || (check != MaterialZones.SolidMaterial.Epidermis && epidermis)) && !always) return;
		
		int xRoll = (100000+x+position.x)%((int)tesselationDimensions.x-1);
		int zRoll = (100000+z+position.z)%((int)tesselationDimensions.z-1);
		
		//Debug.Log("cells["+cells.Length+"]"+ " ("+(x+position.x)+", "+(z+position.z)+")  ("+xRoll+", "+zRoll+", "+y+")");
		/*
		if(xRoll < 0 || xRoll >= tesselationDimensions.x) {
			Debug.Log(xRoll);
		}
		if(zRoll < 0 || zRoll >= tesselationDimensions.z) {
			Debug.Log(zRoll);
		}
		if(y < 0 || y >= tesselationDimensions.y) {
			Debug.Log(y);
		}
		*/
		
		
		Cell c = cells[xRoll, zRoll, y];
		c.outsideRend.transform.localPosition = p;
		c.insideRend.transform.localPosition = Random.insideUnitSphere * nucleusRand;
		if(epidermis) {
			c.outsideRend.transform.rotation = MaterialZones.skinRotation;
			c.outsideRend.material.SetColor("_Color", Color.Lerp(healthyColor1, deadColor1, MaterialZones.skinPosition));
			c.outsideRend.material.SetColor("_RimColor", Color.Lerp(healthyColor2, deadColor2, MaterialZones.skinPosition));
			c.outsideRend.material.SetColor("_RimColor1", Color.Lerp(healthyColor3, deadColor3, MaterialZones.skinPosition));
		}
	}
	
	Color healthyColor1;
	Color healthyColor2;
	Color healthyColor3;
	
	public Color deadColor1;
	public Color deadColor2;
	public Color deadColor3;

	
	public void SetMesh (int x, int z, int y) {
		Cell c = cells[x,z,y];
		Mesh mesh = c.outsideMf.mesh;
		Vector3 pNoRand = SuddenlyTetrahedrons(x, y, z, false, false);
		Vector3 p = SuddenlyTetrahedrons(x, y, z, false, true);
		Vector3[] ns = {
			new Vector3(-1, 1, 0),
			new Vector3( 0, 1, 0),
			new Vector3( 0, 1,-1),
			new Vector3(-1, 0, 1),
			new Vector3(-1, 0, 0),
			new Vector3(-1, 0,-1),
			new Vector3( 0, 0, 1),
			new Vector3( 1, 0, 0),
			new Vector3( 0, 0,-1),
			new Vector3( 0,-1, 0),
			new Vector3( 0,-1, 1),
			new Vector3(-1,-1, 0)
		};
		Plane[] planes = new Plane[12];
		float avgDist = 0f;
		for(int i = 0; i < planes.Length; i++) {
			int ny = (int)(y+ns[i].y);
			int nz = (int)(z+ns[i].z);
			int nx = (int)(x+ns[i].x) + (((ny+nz)&1)==0 && i != 2 && i != 4 && i != 7 && i != 10 ? 1 : 0);
			Vector3 pi = SuddenlyTetrahedrons(nx, ny, nz, false, true) - p;
			
			Vector3 planeCenter = pi*0.5f;
			Vector3 planeNormal = pi;
			
			planes[i] = new Plane(planeCenter, planeCenter+Vector3.Cross(planeNormal,Vector3.up), planeCenter+Vector3.Cross(planeNormal,Vector3.right));
			avgDist += planeNormal.magnitude;
		}
		avgDist /= (12*2);
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		for(int i = 0; i < vertices.Length; i++) {
			float minDist = avgDist * cellExpand;
			for(int ii = 0; ii < planes.Length; ii++) {
				float enter = 0;
				if(planes[ii].Raycast(new Ray(Vector3.zero, normals[i]), out enter) && enter < minDist) {
					minDist = enter;
				}
			}
			//float flatten = 0.3f+(1f-(Vector3.Dot(normals[i], Vector3.forward))) * Vector3.Dot(normals[i], Vector3.right) * 3;
			vertices[i] = (p-pNoRand) + normals[i] * minDist*0.95f;// * (epidermis ? flatten : 1f);
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		if(epidermis) {
			c.outsideMf.transform.localScale = new Vector3(3f,1f,0.5f);
			//c.insideMf.transform.localScale = new Vector3(0.6f,1f,1.5f);
		}
	}
	
	Vector3 SuddenlyTetrahedrons (int x, int y, int z, bool repeat, bool random) {
		if(repeat) {
			x = x%(int)dimensions.x;
			z = z%(int)dimensions.x;
		}
		float xf = x; float yf = y; float zf = z;
		int seed = UnityEngine.Random.seed;
		UnityEngine.Random.seed = x*100+y*10+z +(x*y*z%93);
		Vector3 res = new Vector3(xf+(((z+y)&1)==1 ? 0.5f : 0), yf*0.816496581f, zf*0.866025404f + yf*0.288675135f) + (UnityEngine.Random.insideUnitSphere*randomFac * (random ? 1 : 0));
		UnityEngine.Random.seed = seed;
		return res;
	}
}


/*
x = 1
z = sqrt(3)/2  0.866025404
y = sqrt(6)/3  0.816496581

int x,y,z;
new Vector3(x+((int)(z+y) & 1 ? 0.5f : 0), y*sqrt(6)/3, z*sqrt(3)/2 + y*sqrt(3)/6);
*/

					/*
					if(ii == sel &&x==4&&y==0&&z==2) {
						Debug.DrawRay(p+normals[i]*enter, Vector3.Cross(p+normals[i]*enter, planes[ii].normal).normalized*0.2f,new Color(0,1,1,0.5f));
						Debug.DrawRay(p+normals[i]*enter, Vector3.Cross(Vector3.Cross(p+normals[i]*enter, planes[ii].normal), planes[ii].normal).normalized*0.2f,new Color(0,1,1,0.5f));
						Debug.DrawLine(p, p+normals[i]*enter,new Color(1,0,0,0.5f));
					}
					*/
/*
			/*
			if(i == sel) {
				Debug.DrawLine(planeCenter, planeCenter+Vector3.Cross(planeNormal,Vector3.up), Color.white);
				Debug.DrawLine(planeCenter, planeCenter+Vector3.Cross(planeNormal,Vector3.right), Color.white);
			}
			*/
			
			/*
			if(x==4&&y==1&&z==1) {
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.transform.localScale = new Vector3(1,1,0.001f);
				cube.transform.position = (pi+p)*0.5f;
				cube.transform.LookAt(p);
			}
			*/
		
			/*
			if(i == sel && x==4&&y==0&&z==2) {
				Debug.DrawRay((pi+p)*0.5f, Vector3.Cross(pi-p, Vector3.up).normalized*scale*0.05f, new Color(1,0,1,1));
				Debug.DrawRay((pi+p)*0.5f, Vector3.Cross(pi-p, Vector3.Cross(pi-p, Vector3.up)).normalized*scale*0.05f, new Color(1,0,1,1));
				Debug.DrawLine(p,pi, new Color(0,1,0,0.2f));
			}
			*/

			/*
		class MyPlane {
		public Vector3 planeNormal;
		public Vector3 pointOnPlane;
		public MyPlane (Vector3 n, Vector3 p) {
			planeNormal = n;
			pointOnPlane = p;
		}
		
		public bool Linecast (Vector3 start, Vector3 end, out Vector3 hit) {
			float d = Vector3.Dot(planeNormal, end - start);
			float n = -Vector3.Dot(planeNormal, start - pointOnPlane);

			if (Mathf.Approximately(d,0)) {          // segment is parallel to plane
				hit = Vector3.zero;
				return false;
			}
			// they are not parallel
			// compute intersect param
			float sI = n / d;
			if (sI < 0 || sI > 1) {			// no intersection
				hit = Vector3.zero;
				return false;                       
			}

			hit = start + sI * (end - start);                 // compute segment intersect point
			return true;
		}
	}
			*/