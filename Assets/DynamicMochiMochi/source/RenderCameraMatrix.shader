Shader "omochi/RenderCameraMatrix"
{
	Properties{
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Overlay"
			"DisableBatching" = "True"
			"IgnoreProjector" = "True"
		}
		ZTest Always
		Zwrite Off
		Cull Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _BackgroundTexture;
			sampler2D _MainTex;
			half _TextureOverride;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = float4(v.uv.x * 2.0 - 1.0, _ProjectionParams.x * (v.uv.y * 2.0 - 1.0), 0.1, 1);
				o.uv = ComputeScreenPos(o.vertex);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 screenUV = i.uv.xy / i.uv.w;
#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
				float display_x_px = floor(screenUV.x * 4.0);
				float display_y_px = floor(screenUV.y * 4.0);
				float4 col = float4(0.0, 0.0, 0.0, 0.0);

				if (_ScreenParams.x != 4 && _ScreenParams.y != 4) discard;

				col = (display_x_px == 0 && display_y_px == 0) ? float4(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z, UNITY_MATRIX_P[3][2]) : col;
				col = (display_x_px == 1 && display_y_px == 0) ? float4(UNITY_MATRIX_P[0][0], UNITY_MATRIX_P[1][1], UNITY_MATRIX_P[2][2], UNITY_MATRIX_P[2][3]) : col;
				col = (display_x_px == 2 && display_y_px == 0) ? float4(UNITY_MATRIX_V[0][0], UNITY_MATRIX_V[0][1], UNITY_MATRIX_V[0][2], UNITY_MATRIX_V[0][3]) : col;
				col = (display_x_px == 3 && display_y_px == 0) ? float4(UNITY_MATRIX_V[1][0], UNITY_MATRIX_V[1][1], UNITY_MATRIX_V[1][2], UNITY_MATRIX_V[1][3]) : col;
				col = (display_x_px == 0 && display_y_px == 1) ? float4(UNITY_MATRIX_V[2][0], UNITY_MATRIX_V[2][1], UNITY_MATRIX_V[2][2], UNITY_MATRIX_V[2][3]) : col;
				col = (display_x_px == 1 && display_y_px == 1) ? float4(UNITY_MATRIX_V[3][0], UNITY_MATRIX_V[3][1], UNITY_MATRIX_V[3][2], UNITY_MATRIX_V[3][3]) : col;
				

				return col;
			}
		ENDHLSL
		}
	}
}
