// Upgrade NOTE: replaced '_GUIClipTextureMatrix' with 'unity_GUIClipTextureMatrix'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Hidden/Internal-GUITextureClipText" 
{
	Properties { _MainTex ("Texture", 2D) = "white" {} }

	SubShader {
		Tags { "ForceSupported" = "True" }

		Lighting Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } 
		ZTest Always

		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texgencoord : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _GUIClipTexture;

			uniform float4 _MainTex_ST;
			uniform float4 _Color;
			uniform float4x4 unity_GUIClipTextureMatrix;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 texgen = mul(UNITY_MATRIX_MV, v.vertex);
				o.texgencoord = mul(unity_GUIClipTextureMatrix, texgen);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float4 col = _Color;
				col.a *= tex2D(_MainTex, i.texcoord).a * tex2D(_GUIClipTexture, i.texgencoord).a;
				return col;
			}
			ENDCG 
		}
	} 	
 
	SubShader { 
		Tags { "ForceSupported" = "True" }

		Lighting Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } 
		ZTest Always
		
		Pass { 
			SetTexture [_MainTex] { 
				ConstantColor [_Color] combine constant, constant * texture alpha
			}
			SetTexture [_GUIClipTexture] { // clipping texture - Gets bound to the clipping matrix from code
				combine previous, previous * texture alpha 
			}  
		}
	}
	
	// Really ancient sub shader for single-texture cards
	SubShader { 
		Lighting Off 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } 
		ZTest Always
		Tags { "ForceSupported" = "True" }
		
		Pass { // Get the base alpha in
			ColorMask A 
			SetTexture [_MainTex] { 
				ConstantColor [_Color] combine constant * texture alpha, constant * texture alpha
			} 
		} 
		Pass { // Multiply in the clip alpha
			ColorMask A 
			Blend DstAlpha Zero 
			SetTexture [_GUIClipTexture] { 
				combine previous, previous * texture alpha 
			} 
		}
		Pass { // Get color
			ColorMask RGB 
			Blend DstAlpha OneMinusDstAlpha 
			SetTexture [_MainTex] { 
				ConstantColor [_Color] combine constant * texture alpha} 
		} 
	}
}
