using UnityEngine;
using System.Collections;

public class GUIStuff : MonoBehaviour {
	public float guiScale = 0.5f;

	public Font theFont;
	public Texture2D clearImg;
	public Texture2D blockImg;
	public Texture2D holeImg;
	public Texture2D selectionImg;
	public Texture2D selection1Img;
	public Texture2D[] zoomLevelImages;
	public Texture2D[] particleImages;

	public int border = 52;
	public GUIStyle font;
	public GUIStyle clear;
	public GUIStyle[] zoomLevels;
	public GUIStyle[] particles;
	public GUIStyle hole;
	public GUIStyle block;
	public GUIStyle selection;
	public GUIStyle selection1;
	public Vector2 screen;
	public virtual void Start () {
		if(zoomLevelImages == null || zoomLevelImages.Length < 3) zoomLevelImages = new Texture2D[3];
		if(particleImages == null  || particleImages.Length  < 3) particleImages = new Texture2D[3];
		zoomLevels = new GUIStyle[3];
		for(int i = 0; i < 3; i++) {
			zoomLevels[i] = new GUIStyle();
			zoomLevels[i].normal.background = zoomLevelImages[i];
		}
		particles = new GUIStyle[3];
		for(int i = 0; i < 3; i++) {
			particles[i] = new GUIStyle();
			particles[i].normal.background = particleImages[i];
		}
		clear = new GUIStyle();
		clear.normal.background = clearImg;
		font = new GUIStyle();
		font.font = theFont;
		font.normal.textColor = Color.white;
		hole = new GUIStyle();
		block = new GUIStyle();
		selection = new GUIStyle();
		selection1 = new GUIStyle();
		hole.normal.background = holeImg;
		block.normal.background = blockImg;
		selection.normal.background = selectionImg;
		selection1.normal.background = selection1Img;
		hole.border.left = hole.border.right = hole.border.top = hole.border.bottom = border;
		block.border.left = block.border.right = block.border.top = block.border.bottom = border;
		selection.border.left = selection.border.right = selection.border.top = selection.border.bottom = border;
	}
	
	public virtual void OnGUI () {
		float newGUIScale = ((Screen.width / 800f)+(Screen.height / 600f))*0.5f  * guiScale;
		screen = new Vector2(((float)Screen.width)/newGUIScale, ((float)Screen.height)/newGUIScale);
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * newGUIScale);
	}
	
	public void Scale (float s) {
		guiScale *= s;
		float newGUIScale = ((Screen.width / 800f)+(Screen.height / 600f))*0.5f  * guiScale;
		screen = new Vector2(((float)Screen.width)/newGUIScale, ((float)Screen.height)/newGUIScale);
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * newGUIScale);
	}
}
