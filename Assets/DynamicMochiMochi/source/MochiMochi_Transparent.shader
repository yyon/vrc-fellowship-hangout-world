Shader "omochi/MochiMochiShaderTransparent" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MochiMask2("Mask for Clipping", 2D) = "white" {}
		_MatCap("MatCap (Add)", 2D) = "black" {}
		_MatCapPower("MatCap Power", Range(0,1)) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Transparency("Transparency", Range(0,1)) = 0.0
		[Space(30)]
		_Tess("Tessellation Detail", Range(0,0.999)) = 0.999
		[Space(30)]
		_MochiMask ("Custom Render Texture", 2D) = "black" {}
		[HideInInspector]_mochi_dir ("Direction", vector) = (0,1,0,0)
	}
		SubShader{
			Tags{ "Queue" = "AlphaTest" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }
			LOD 200

			Cull Front
			Zwrite Off
			ZTest LEqual
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows vertex:vert addshadow tessellate:tessEdge nolightmap keepalpha
			
			#include "Tessellation.cginc"
			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 5.0
			sampler2D _MainTex;
			sampler2D _MochiTex;
			//float4 _MochiTex_ST;
			sampler2D _MochiMask;
			sampler2D _MochiMask2;
			float4 _MochiMask_ST;
			sampler2D _MatCap;
			float _MatCapPower;

			half _Glossiness;
			half _Metallic;
			half _Transparency;
			fixed4 _Color;
			fixed4 _BlendColorMochi;
			fixed3 _mochi_dir;

			float2 temp;
			static float _mochi_wid = -1.0;
			static float _mochi_fade = 0.95;
			static float mochi_hei = 1.0;
			static float mochi_min = 0.0;

			struct Input {
				float2 uv_MainTex;
				float3 worldNormal;
				float3 worldPos;
				//float2 uv_MochiMask;
			};

			float _Tess;

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 tessEdge(appdata v0, appdata v1, appdata v2)
			{
				if (mochi_hei < 0.0002)
				{
					return 1;
				}
				return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, 10 - (_Tess * 10));
			}
			float4 tessFixed()
			{
				return _Tess;
			}

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)


			void vert(inout appdata v) {
				//UNITY_SETUP_INSTANCE_ID(v);

				float3 sa = _mochi_dir;

				float2 uv_pos;
				uv_pos = float2(0, 0);

				v.texcoord.x *= _MochiMask_ST.x;
				v.texcoord.y *= _MochiMask_ST.y;
				if (v.texcoord.x < 0) { v.texcoord.x += 1; }
				if (v.texcoord.y < 0) { v.texcoord.y += 1; }
				float4 c_sm = tex2Dlod(_MochiMask, float4(v.texcoord.xy, 0, 0));
				
				float d = 4.0 / 1024.0;
				float mx1 = tex2Dlod(_MochiMask2, float4(v.texcoord.x + d, v.texcoord.y, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				float mx2 = tex2Dlod(_MochiMask2, float4(v.texcoord.x - d, v.texcoord.y, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				float my1 = tex2Dlod(_MochiMask2, float4(v.texcoord.x, v.texcoord.y - d, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				float my2 = tex2Dlod(_MochiMask2, float4(v.texcoord.x, v.texcoord.y + d, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				d = 1.0 / 1024.0;
				float dx = (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x + d, v.texcoord.y, 0, 0))) * mx1 - (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x - d, v.texcoord.y, 0, 0))) * mx2;
				float dz = (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x, v.texcoord.y - d, 0, 0))) * my1 - (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x, v.texcoord.y + d, 0, 0))) * my2;
				v.normal = normalize(float3(dx * abs(unity_ObjectToWorld[0][0]), d * 4.0 * abs(unity_ObjectToWorld[1][1]), -dz * abs(unity_ObjectToWorld[2][2])));


				float _mochi_hei_conv = mochi_hei;
				fixed4 mask2 = tex2Dlod(_MochiMask2, float4(v.texcoord.xy, 0, 0));
				c_sm.r = mask2.r >= 0.99 ? c_sm.r : 1.0;
				_mochi_hei_conv *= (1 - c_sm.r);
				_mochi_hei_conv = max(_mochi_hei_conv, mochi_hei*mochi_min);
				v.vertex.xyz += sa * _mochi_hei_conv;
				
			}

			float3 getMatcapUV(float3 v_normal, float3 v_view_dir)
			{
				float3 matcap_base = v_view_dir * float3(-1, -1, 1) + float3(0, 0, 1);
				float3 matcap_detail = v_normal.xyz * float3(-1, -1, 1);
				return matcap_base * dot(matcap_base, matcap_detail) / matcap_base.z - matcap_detail;
			}

			float2x2 matcapUVRotMat() {
				float2 camera_up = mul((float3x3)UNITY_MATRIX_V, float3(0, 1, 0)).xy;
				if (any(camera_up)) camera_up = normalize(camera_up);
				float2x2 ret_mat = { camera_up.y, camera_up.x, -camera_up.x, camera_up.y };
				return ret_mat;
			}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c_clip = tex2D(_MochiMask2, IN.uv_MainTex);
				//clip(c_clip.a - 0.01);
				clip(c_clip.r - 0.01);

#ifdef USING_STEREO_MATRICES
				float3 w_vdir = normalize((unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5 - IN.worldPos);
#else
				float3 w_vdir = normalize(_WorldSpaceCameraPos - IN.worldPos);
#endif
				float3 v_normal = normalize(mul(float4(IN.worldNormal, 1), UNITY_MATRIX_I_V).xyz);
				float3 v_vdir = mul((float3x3)UNITY_MATRIX_V, w_vdir);
				float3 matcap_uv = getMatcapUV(v_normal, v_vdir);
				matcap_uv.xy = mul(matcap_uv.xy, matcapUVRotMat());
				float4 matcap_col = tex2D(_MatCap, matcap_uv.xy * 0.5 + 0.5);

				float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

				fixed4 c_mochi = tex2D(_MainTex, IN.uv_MainTex);
				c_mochi.rgb = lerp(c_mochi, _BlendColorMochi.rgb, _BlendColorMochi.a);
				o.Albedo = c.rgb;
				o.Emission = clamp(_MatCapPower * matcap_col.rgb, 0.0, 1.0);

				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				//o.Specular = _Glossiness + 0.00001;
				//o.Gloss = 1;
				o.Alpha = c.a * (1.0 - _Transparency);
				o.Occlusion = o.Albedo;
			}
			ENDCG

			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }
			Cull Back
			ZTest LEqual
			ZWrite On
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows vertex:vert addshadow tessellate:tessEdge nolightmap keepalpha
			
			#include "Tessellation.cginc"
			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 5.0
			sampler2D _MainTex;
			sampler2D _MochiTex;
			//float4 _MochiTex_ST;
			sampler2D _MochiMask;
			sampler2D _MochiMask2;
			float4 _MochiMask_ST;
			sampler2D _MatCap;
			float _MatCapPower;

			half _Glossiness;
			half _Metallic;
			half _Transparency;
			fixed4 _Color;
			fixed4 _BlendColorMochi;
			fixed3 _mochi_dir;

			float2 temp;
			static float _mochi_wid = -1.0;
			static float _mochi_fade = 0.95;
			static float mochi_hei = 1.0;
			static float mochi_min = 0.0;

			struct Input {
				float2 uv_MainTex;
				float3 worldNormal;
				float3 worldPos;
				//float2 uv_MochiMask;
			};

			float _Tess;

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 tessEdge(appdata v0, appdata v1, appdata v2)
			{
				if (mochi_hei < 0.0002)
				{
					return 1;
				}
				return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, 10 - (_Tess * 10));
			}
			float4 tessFixed()
			{
				return _Tess;
			}

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)


			void vert(inout appdata v) {
				//UNITY_SETUP_INSTANCE_ID(v);

				float3 sa = _mochi_dir;

				float2 uv_pos;
				uv_pos = float2(0, 0);

				v.texcoord.x *= _MochiMask_ST.x;
				v.texcoord.y *= _MochiMask_ST.y;
				if (v.texcoord.x < 0) { v.texcoord.x += 1; }
				if (v.texcoord.y < 0) { v.texcoord.y += 1; }
				float4 c_sm = tex2Dlod(_MochiMask, float4(v.texcoord.xy, 0, 0));
				
				float d = 4.0 / 1024.0;
				float mx1 = tex2Dlod(_MochiMask2, float4(v.texcoord.x + d, v.texcoord.y, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				float mx2 = tex2Dlod(_MochiMask2, float4(v.texcoord.x - d, v.texcoord.y, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				float my1 = tex2Dlod(_MochiMask2, float4(v.texcoord.x, v.texcoord.y - d, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				float my2 = tex2Dlod(_MochiMask2, float4(v.texcoord.x, v.texcoord.y + d, 0, 0)) >= 0.99 ? 1.0 : 0.0;
				d = 1.0 / 1024.0;
				float dx = (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x + d, v.texcoord.y, 0, 0))) * mx1 - (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x - d, v.texcoord.y, 0, 0))) * mx2;
				float dz = (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x, v.texcoord.y - d, 0, 0))) * my1 - (1.0 - tex2Dlod(_MochiMask, float4(v.texcoord.x, v.texcoord.y + d, 0, 0))) * my2;
				v.normal = normalize(float3(dx * abs(unity_ObjectToWorld[0][0]), d * 4.0 * abs(unity_ObjectToWorld[1][1]), -dz * abs(unity_ObjectToWorld[2][2])));


				float _mochi_hei_conv = mochi_hei;
				fixed4 mask2 = tex2Dlod(_MochiMask2, float4(v.texcoord.xy, 0, 0));
				c_sm.r = mask2.r >= 0.99 ? c_sm.r : 1.0;
				_mochi_hei_conv *= (1 - c_sm.r);
				_mochi_hei_conv = max(_mochi_hei_conv, mochi_hei*mochi_min);
				v.vertex.xyz += sa * _mochi_hei_conv;
				
			}

			float3 getMatcapUV(float3 v_normal, float3 v_view_dir)
			{
				float3 matcap_base = v_view_dir * float3(-1, -1, 1) + float3(0, 0, 1);
				float3 matcap_detail = v_normal.xyz * float3(-1, -1, 1);
				return matcap_base * dot(matcap_base, matcap_detail) / matcap_base.z - matcap_detail;
			}

			float2x2 matcapUVRotMat() {
				float2 camera_up = mul((float3x3)UNITY_MATRIX_V, float3(0, 1, 0)).xy;
				if (any(camera_up)) camera_up = normalize(camera_up);
				float2x2 ret_mat = { camera_up.y, camera_up.x, -camera_up.x, camera_up.y };
				return ret_mat;
			}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c_clip = tex2D(_MochiMask2, IN.uv_MainTex);
				//clip(c_clip.a - 0.01);
				clip(c_clip.r - 0.01);

#ifdef USING_STEREO_MATRICES
				float3 w_vdir = normalize((unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5 - IN.worldPos);
#else
				float3 w_vdir = normalize(_WorldSpaceCameraPos - IN.worldPos);
#endif
				float3 v_normal = normalize(mul(float4(IN.worldNormal, 1), UNITY_MATRIX_I_V).xyz);
				float3 v_vdir = mul((float3x3)UNITY_MATRIX_V, w_vdir);
				float3 matcap_uv = getMatcapUV(v_normal, v_vdir);
				matcap_uv.xy = mul(matcap_uv.xy, matcapUVRotMat());
				float4 matcap_col = tex2D(_MatCap, matcap_uv.xy * 0.5 + 0.5);

				float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

				fixed4 c_mochi = tex2D(_MainTex, IN.uv_MainTex);
				c_mochi.rgb = lerp(c_mochi,_BlendColorMochi.rgb,_BlendColorMochi.a);
				o.Albedo = c.rgb;
				o.Emission = clamp(_MatCapPower * matcap_col.rgb, 0.0, 1.0);

				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				//o.Specular = _Glossiness + 0.00001;
				//o.Gloss = 1;
				o.Alpha = c.a * (1.0 - _Transparency);
				o.Occlusion = o.Albedo;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
