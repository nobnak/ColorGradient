Shader "Hidden/ColorGradient" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)

		_GradientTex ("Gradient", 2D) = "white" {}
		_GradientGain ("Gradient Gain", Range(0, 10)) = 1

		_NoiseTex ("Noise", 2D) = "black" {}
		_NoiseGain ("Noise Gain", Range(0,1)) = 0

		_BlendMode ("Blend Mode", Int) = 0
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma multi_compile ___ NOISE_ENABLED

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
			sampler2D _GradientTex;
			sampler2D _NoiseTex;

			float4 _Color;
			float _GradientGain;
			float _NoiseGain;

			float4x4 _GradientMatrix;

			int _BlendMode;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float4 frag (v2f i) : SV_Target 	{
				float gnoise = 0;
				#ifdef NOISE_ENABLED
					gnoise = _NoiseGain * (2 * tex2D(_NoiseTex, i.uv).x - 1.0);
				#endif

				float4 cmain = tex2D(_MainTex, i.uv);

				float gradU = mul(_GradientMatrix, float4(i.uv, 0, 1)).x + gnoise;
				float4 cgrad = _GradientGain * tex2D(_GradientTex, float2(gradU, 0));
				
				return blend_mode4(cmain, cgrad * _Color, _BlendMode);
			}
			ENDCG
		}
	}
}
