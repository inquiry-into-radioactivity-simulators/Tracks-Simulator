  Shader "CellWall" {
    Properties {
	  _Color ("_Color", Color) = (0.26,0.19,0.16,0.0)
      _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
	  _RimColor1 ("Rim Color1", Color) = (0.26,0.19,0.16,0.0)
      _RimPower1 ("Rim Power1", Range(0.5,8.0)) = 3.0
    }
    SubShader {
      Tags { "RenderType" = "Transparent" "Queue"="Transparent+4"}
      Blend SrcAlpha OneMinusSrcAlpha
      Cull back
      Zwrite off
      CGPROGRAM
        #pragma surface surf Lambert alpha
        struct Input {
            float3 viewDir;
        };
        float4 _Color;
        float4 _RimColor;
        float _RimPower;
        float4 _RimColor1;
        float _RimPower1;
        void surf (Input IN, inout SurfaceOutput o) {
        half protoRim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
        half rim = pow (protoRim, _RimPower);
        half rim1 = pow (protoRim, _RimPower1);
            o.Emission = o.Albedo = lerp(_Color.rgb, lerp(_RimColor.rgb, _RimColor1.rgb, rim1), rim+rim1);
            o.Alpha = rim*_RimColor.a + rim1*_RimColor1.a + _Color.a;
        }
      ENDCG
    } 
    Fallback "Diffuse"
  }
