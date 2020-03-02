Shader "Particles/Pattern Alpha Blended+4" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_Pattern ("Pattern", 2D) = "white" {TexGen ObjectLinear}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	Tags { "Queue"="Transparent+4" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		//Bind "TexCoord", texcoord
	}

	
	// ---- Dual texture cards
	SubShader {
		Pass {
			SetTexture [_Pattern] {
				constantColor[_TintColor]
				combine texture * constant
			}
			SetTexture [_MainTex] {
				combine previous*texture, texture alpha
			}
		}
	}
	
	// ---- Single texture cards (does not do color tint)
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				combine texture * primary
			}
		}
	}
}
}
