Shader "Hidden/ColorGradient" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		
		_GradientBlendMode ("Gradient Blend Mode", Int) = 0
		_GradientTex ("Gradient Tex", 2D) = "white" {}
		_GradientOpacity ("Gradient Opacity", Range(0, 1)) = 1
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc" 
			#include "Assets/Packages/Gist/CGIncludes/BlendMode.cginc" 

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			sampler2D _MainTex;
			
			int _GradientBlendMode;
			sampler2D _GradientTex;
			float _GradientOpacity;

			float4x4 _GradientMatrix;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float4 frag (v2f i) : SV_Target {
				float4 cmain = tex2D(_MainTex, i.uv);

				float gradU = mul(_GradientMatrix, float4(i.uv, 0, 1)).x;
				float4 cgrad = _GradientOpacity * tex2D(_GradientTex, float2(gradU, 0));
				
				return blend_mode4(cmain, cgrad, _GradientBlendMode);
			}
			ENDCG
		}
	}
}
