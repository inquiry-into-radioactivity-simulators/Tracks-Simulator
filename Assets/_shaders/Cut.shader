Shader "Cut" {
Properties {

}

SubShader {
	Tags { "Queue" = "Geometry+10" }
	LOD 100
	
		cull front
        Lighting Off
        ZTest LEqual
        ZWrite Off
        ColorMask 0
	
Pass {
}


}

Fallback off
}
