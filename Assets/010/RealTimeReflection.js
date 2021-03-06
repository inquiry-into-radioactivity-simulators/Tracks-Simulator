// Attach this script to an object that uses a Reflective shader.
// Realtime reflective cubemaps!

@script ExecuteInEditMode

var cubemapSize = 128;
var oneFacePerFrame = false;
private var cam : Camera;
private var rtex : RenderTexture;

function Start () {
// render all six faces at startup
UpdateCubemap( 63 );
}

function LateUpdate () {
if (oneFacePerFrame) {
var faceToRender = Time.frameCount % 6;
var faceMask = 1 << faceToRender;
UpdateCubemap (faceMask);
} else {
UpdateCubemap (63); // all six faces
}
}

function UpdateCubemap (faceMask : int) {
if (!cam) {
var go = new GameObject ("CubemapCamera", Camera);
go.hideFlags = HideFlags.HideAndDontSave;
go.transform.position = transform.position;
go.transform.rotation = Quaternion.identity;
cam = go.GetComponent.<Camera>();
cam.cullingMask = ~(1 << 9);
cam.nearClipPlane = 0.01f;
cam.farClipPlane = 50; // don't render very far into cubemap
cam.enabled = false;
}

if (!rtex) {
rtex = new RenderTexture (cubemapSize, cubemapSize, 16);
rtex.isPowerOfTwo = true;
rtex.isCubemap = true;
rtex.hideFlags = HideFlags.HideAndDontSave;
GetComponent.<Renderer>().sharedMaterial.SetTexture ("_Cube", rtex);
}

cam.transform.position = transform.position;
cam.RenderToCubemap (rtex, faceMask);
}

function OnDisable () {
DestroyImmediate (cam);
DestroyImmediate (rtex);
}