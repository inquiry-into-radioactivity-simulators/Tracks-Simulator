using UnityEngine;
using System.Collections;

public class SelectZoomLevelGUI : GUIStuff {
	public Texture2D fireButtonImg;
	public Texture2D backButtonImg;
	public Texture2D fastForwardButtonImg;
	public Texture2D camBlockImg;
	public Texture2D fastForwardButtonImg2;
	public Texture2D pauseButtonImg;
	public Texture2D pauseButtonImg2;
	GUIStyle fastForwardButton;
	GUIStyle pauseButton;
	GUIStyle pauseButtonPaused;
	GUIStyle fireButton;
	GUIStyle backButton;
	public GUIStyle camBlock;
	
	public ShooterDriver shooterDriver;
	public Vector2 outerMargin;
	public Vector2 iconSize;
	public Vector2 innerMargin;
	
	public static int selectedButton = 2;
	public static int selectedParticle = 1;
	public float selectedLerpSpeed = 5.0f;
	public static bool paused = false;

	Transform mmCamT;
	Camera mmCam;
	
	Rect sr2;
	Rect targetSr2;
	Rect sr3;
	Rect targetSr3;
	
	public override void Start () {
		base.Start();
		
		mmCamT = GameObject.FindWithTag("MiniMapCamera").transform;
		mmCam = mmCamT.GetComponent<Camera>();
		
		fireButton = new GUIStyle();
		fireButton.normal.background = fireButtonImg;
		backButton = new GUIStyle();
		backButton.normal.background = backButtonImg;
		fastForwardButton = new GUIStyle();
		fastForwardButton.normal.background = fastForwardButtonImg;
		fastForwardButton.active.background = fastForwardButtonImg2;
		pauseButton = new GUIStyle();
		pauseButton.normal.background = pauseButtonImg;
		pauseButton.active.background = pauseButtonImg2;
		pauseButtonPaused = new GUIStyle();
		pauseButtonPaused.normal.background = pauseButtonImg2;
		pauseButtonPaused.active.background = pauseButtonImg;
		
		camBlock = new GUIStyle();
		camBlock.normal.background = camBlockImg;
		camBlock.border.left = camBlock.border.right = camBlock.border.top = camBlock.border.bottom = border;
		
		sr2 = new Rect(0,0,0,0);
		targetSr2 = new Rect(0,0,0,0);
	}
	
	void Update () {
		Time.timeScale = paused ? 0.0001f : TimeMan.timeScale;
		sr2 = new Rect(Mathf.Lerp(sr2.x, targetSr2.x, Time.deltaTime * selectedLerpSpeed), targetSr2.y, targetSr2.width, targetSr2.height);
		sr3 = new Rect(Mathf.Lerp(sr3.x, targetSr3.x, Time.deltaTime * selectedLerpSpeed), targetSr3.y, targetSr3.width, targetSr3.height);
	}
	
	public override void OnGUI () {
		base.OnGUI();
		
		bool f = ShooterDriver.firing != -1;
		Vector2 s = new Vector2(iconSize.x*(f ?  2 : 1)+outerMargin.x+innerMargin.x*4, iconSize.y+outerMargin.y+innerMargin.y*4);
		
		Rect outerRect = new Rect(screen.x-(s.x), screen.y-(s.y), s.x-outerMargin.x, s.y-outerMargin.y);
		Rect innerRect = new Rect(innerMargin.x, innerMargin.y, iconSize.x*(f ?  2 : 1)+innerMargin.y*2, iconSize.y+innerMargin.y*2);
		Rect innerInnerRect = new Rect(innerMargin.x, innerMargin.y, iconSize.x, iconSize.y);
		Rect innerInnerRect2 = new Rect(innerMargin.x + iconSize.x, innerMargin.y, iconSize.x, iconSize.y);
		GUI.BeginGroup(outerRect, "", block);
			GUI.BeginGroup(innerRect, "", hole);
				if(ShooterDriver.firing != -1) {
					ProjectileDriver.fastForward1 = false;
					if(!ProjectileDriver.noFastForward || selectedButton == 0) {
						if(ShooterDriver.firing > 0 && GUI.RepeatButton(innerInnerRect2, "", fastForwardButton)) {
							ProjectileDriver.fastForward1 = true;
						}
					} else {
						Color oldColor = GUI.color;
						GUI.color = Color.Lerp(GUI.color, new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0), 0.75f);
						GUI.RepeatButton(innerInnerRect2, "", fastForwardButton);
						GUI.color = oldColor;
					}
					if(GUI.Button(innerInnerRect, "", paused ? pauseButtonPaused : pauseButton)) {
						paused = !paused;
					}
					
				} else {
					if(GUI.Button(innerInnerRect, "", fireButton)) {
						shooterDriver.OnMouseDown();
					}
				}
				
			GUI.EndGroup();
		GUI.EndGroup();
		
		mmCam.enabled = (SelectZoomLevelGUI.selectedButton != 2 && ShooterDriver.firing > 0);
		if(mmCam.enabled) {
			Vector3 p1 = Camera.main.transform.position;
			mmCamT.position = new Vector3(p1.x, 100, p1.z);
			Vector2 s1 = new Vector2(iconSize.x*2+outerMargin.x+innerMargin.x*4, iconSize.y*2+outerMargin.y+innerMargin.y*4);
			Rect cr = new Rect(screen.x-(s1.x), screen.y-(s1.y+s.y-outerMargin.y), s1.x-outerMargin.x, s1.y-outerMargin.y);
			innerRect = new Rect(cr.x+innerMargin.x, cr.y+innerMargin.y, iconSize.x*2+innerMargin.y*2, iconSize.y*2+innerMargin.y*2);
			GUI.BeginGroup(cr, "", camBlock);
				mmCam.rect = new Rect(innerRect.x/screen.x, (screen.y-innerRect.y-innerRect.height)/screen.y, innerRect.width/screen.x, innerRect.height/screen.y);
			GUI.EndGroup();
		}
		
		if(ShooterDriver.firing != -1) {
			outerRect = new Rect(outerMargin.x, screen.y-(iconSize.x+outerMargin.y+innerMargin.y*4), iconSize.x+innerMargin.x*4, iconSize.y+innerMargin.x*4);
			innerRect = new Rect(innerMargin.x, innerMargin.y, iconSize.x+innerMargin.y*2, iconSize.y+innerMargin.y*2);
			innerInnerRect = new Rect(innerMargin.x, innerMargin.y, iconSize.x, iconSize.y);
			GUI.BeginGroup(outerRect, "", block);
				GUI.BeginGroup(innerRect, "", hole);
					if(ShooterDriver.firing > 0 && GUI.Button(innerInnerRect, "", backButton)) {
						ShooterDriver.i.CleanUp();
						paused = false;
					}
				GUI.EndGroup();
			GUI.EndGroup();
		} else {
			outerRect = new Rect(outerMargin.x, screen.y-(iconSize.y*2+innerMargin.y*5+outerMargin.y), iconSize.x*3+innerMargin.x*5f, iconSize.y*2+innerMargin.y*4.5f);
			innerRect = new Rect(outerRect.x+innerMargin.x, outerRect.y+innerMargin.y, outerRect.width-innerMargin.x*2, outerRect.height-innerMargin.y*2);
			GUI.Box(outerRect, "", block);
			GUI.BeginGroup(innerRect, "", hole);
				Rect[] buttonRects = new Rect[3];
				for(int i = 0; i < 3; i++) {
					buttonRects[i] = new Rect(i != 0 ? buttonRects[i-1].x+iconSize.x+innerMargin.x*0.5f : innerMargin.x,innerMargin.y, iconSize.x, iconSize.y);
				}
				Rect sr = buttonRects[selectedButton];
				targetSr2 = new Rect(sr.x-innerMargin.x*0.5f, sr.y-innerMargin.y*0.5f, sr.width+innerMargin.x, sr.height+innerMargin.y);

				Color c = GUI.color; GUI.color = Color.yellow;
				GUI.Box(sr2, "", selection);
				GUI.color = c;
				
				for(int i = 0; i < 3; i++) {
					if(GUI.Button(buttonRects[i], "", zoomLevels[i])) {
						selectedButton = i;
					}
				}
				buttonRects = new Rect[3];
				for(int i = 0; i < 3; i++) {
					buttonRects[i] = new Rect(i != 0 ? buttonRects[i-1].x+iconSize.x+innerMargin.x*0.5f : innerMargin.x,innerMargin.y*1.5f+iconSize.y, iconSize.x, iconSize.y);
				}
				sr = buttonRects[selectedParticle];
				targetSr3 = new Rect(sr.x-innerMargin.x*0.5f, sr.y-innerMargin.y*0.5f, sr.width+innerMargin.x, sr.height+innerMargin.y);

				Color c1 = GUI.color; GUI.color = Color.yellow;
				GUI.Box(sr3, "", selection);
				GUI.color = c;
				
				for(int i = 0; i < 3; i++) {
					if(GUI.Button(buttonRects[i], "", particles[i])) {
						selectedParticle = i;
					}
				}
			GUI.EndGroup();
		}
		
	}
}
