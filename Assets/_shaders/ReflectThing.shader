Shader "ReflectiveThing" {
Properties {
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}
SubShader {
	LOD 200
	Tags { "RenderType"="Opaque" }
	
CGPROGRAM
#pragma surface surf Lambert

samplerCUBE _Cube;

struct Input {
	float3 worldRefl;
};

void surf (Input IN, inout SurfaceOutput o) {
	o.Albedo = 0;
	half4 reflcol = texCUBE (_Cube, IN.worldRefl);
	o.Emission = reflcol.rgb;
}
ENDCG
}
	
FallBack "Reflective/VertexLit"
} 
