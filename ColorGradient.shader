Shader "Hidden/ColorGradient" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile ___ BLEND_MULT BLEND_SCRN BLEND_OVRY
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _Color;

			float4x4 _Poss;
			float4x4 _Colors;
			float4 _Throttles;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_Target {
				float2 p = i.uv;
				float2x4 diff = float2x4(_Poss[0] - p.xxxx, _Poss[1] - p.yyyy);
				float2x4 sqdiff = diff * diff;
				float4 metaball = _Throttles / (sqdiff[0] + sqdiff[1]);
				float sum = metaball.x + metaball.y + metaball.z + metaball.w;
				float4 t = metaball / sum;

				float4 cmain = tex2D(_MainTex, i.uv);
				float4 cgrad = mul(_Colors, t) * _Color;

				#ifdef UNITY_COLORSPACE_GAMMA
				cmain.rgb = GammaToLinearSpace(cmain.rgb);
				#endif

				float4 c = cgrad;
				#if defined(BLEND_MULT)
				c *= cmain;
				#elif defined(BLEND_SCRN)
				c = 1 - (1 - cmain) * (1 - c);
				#elif defined(BLEND_OVRY)
				c = (c < 0.5 ? (2 * cmain * c) : (1 - 2 * (1 - cmain) * (1 - c)));
				#endif
				c = lerp(cmain, c, cgrad.a);

				#ifdef UNITY_COLORSPACE_GAMMA
				c.rgb = LinearToGammaSpace(c.rgb);
				#endif
				return c;
			}
			ENDCG
		}
	}
}
