// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "omochi/MochiMochiShaderVolumeRefractionBackup" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_refractiveIndex("Refractive Index",  Range(1,2)) = 1
		_FogColor("FogColor", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MochiMask2("Mask for Clipping", 2D) = "black" {}
		_MatCap("MatCap (Add)", 2D) = "black" {}
		_MatCapPower("MatCap Power", Range(0,1)) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Transparency("Transparency", Range(0,1)) = 0.0
		[Space(30)]
		_MochiMask ("Custom Render Texture", 2D) = "black" {}
		_SubCameraTex("SubCameraTex", 2D) = "black" {}
		_SubCameraDepth("SubCameraDepth", 2D) = "white" {}
		_SubCameraMatrixTex("SubCameraMatrix", 2D) = "white" {}
		[HideInInspector]_mochi_dir ("Direction", vector) = (0,1,0,0)
	}
	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }
		LOD 200

		GrabPass
		{
			"_BackgroundTexture2"
		}

		Cull Front
		ZTest Always
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 5.0
			//#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase noshadowmask nodynlightmap nolightmap
			#include "HLSLSupport.cginc"
			#define UNITY_INSTANCED_LOD_FADE
			#define UNITY_INSTANCED_SH
			#define UNITY_INSTANCED_LIGHTMAPSTS
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			#if !defined(INSTANCING_ON)
			#include "UnityCG.cginc"
			#undef UNITY_SHOULD_SAMPLE_SH
			#define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"
			#define INTERNAL_DATA
			#define WorldReflectionVector(data,normal) data.worldRefl
			#define WorldNormalVector(data,normal) normal
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _MochiTex;
			sampler2D _MochiMask;
			sampler2D _MochiMask2;
			float4 _MochiMask_ST;
			sampler2D _MatCap;
			sampler2D _BackgroundTexture2;

			sampler2D _SubCameraTex;
			sampler2D _SubCameraDepth;
			sampler2D _SubCameraMatrixTex;

			//static const int _StepNum = 10;
			#define _StepNum 64
			#define _SurfaceSearchIteration 5
			#define _TexResoution 1024

			half _Glossiness;
			half _Metallic;
			half _Transparency;
			float _MatCapPower;
			fixed4 _Color;
			fixed4 _FogColor;
			fixed4 _BlendColorMochi;

			float _refractiveIndex;
			
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f{
                //float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 scrPos : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 localPos : TEXCOORD2;
				float2 tminmax : TEXCOORD3;
				float mirrorFlagF : TEXCOORD5;

				UNITY_VERTEX_OUTPUT_STEREO
            };
			
            v2f vert (appdata v){
                v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//bounds of the defaut cube
				float3 corner[8];
				corner[0] = float3(-0.5, -0.5, -0.5);
				corner[1] = float3( 0.5, -0.5, -0.5);
				corner[2] = float3(-0.5,  0.5, -0.5);
				corner[3] = float3( 0.5,  0.5, -0.5);
				corner[4] = float3(-0.5, -0.5,  0.5);
				corner[5] = float3( 0.5, -0.5,  0.5);
				corner[6] = float3(-0.5,  0.5,  0.5);
				corner[7] = float3( 0.5,  0.5,  0.5);

				float3 world_camdir = unity_CameraToWorld._m02_m12_m22;
				float3 local_slicedir = normalize(mul(transpose((float3x3)unity_ObjectToWorld), world_camdir));
				float3 local_campos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

				float tmin = dot(corner[0] - local_campos, local_slicedir);
				float tmax = tmin;
				[unroll]
				for (int i = 1; i < 8; i++)
				{
					float d = dot(corner[i] - local_campos, local_slicedir);
					tmin = tmin > d ? d : tmin;
					tmax = tmax < d ? d : tmax;
				}
				o.tminmax = max(float2(tmin, tmax), float2(0.00001, 0.00001));

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.localPos = v.vertex.xyz;
				o.scrPos = ComputeScreenPos(o.vertex);

				float3 crossFwd = cross(UNITY_MATRIX_V[0], UNITY_MATRIX_V[1]);
				o.mirrorFlagF = dot(crossFwd, UNITY_MATRIX_V[2]) < 0 ? 1.0 : -1.0;

                return o;
            }

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

#if UNITY_SINGLE_PASS_STEREO
#define MF_PROJMAT unity_StereoMatrixP[unity_StereoEyeIndex]
#define EYE_IDX unity_StereoEyeIndex
#else
#define MF_PROJMAT UNITY_MATRIX_P
#define EYE_IDX 0
#endif


// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
			// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_WORLD_POS
			struct v2f_surf {
				UNITY_POSITION(pos);
				float2 pack0 : TEXCOORD0; // _MainTex
				float3 worldNormal : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				fixed4 color : COLOR0;
#if UNITY_SHOULD_SAMPLE_SH
				half3 sh : TEXCOORD3; // SH
#endif
				UNITY_LIGHTING_COORDS(4, 5)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
#endif
			// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
			struct v2f_surf {
				UNITY_POSITION(pos);
				float2 pack0 : TEXCOORD0; // _MainTex
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				fixed4 color : COLOR0;
#if UNITY_SHOULD_SAMPLE_SH
				half3 sh : TEXCOORD3; // SH
#endif
				UNITY_FOG_COORDS(4)
				UNITY_SHADOW_COORDS(5)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
#endif
#endif
			// with lightmaps:
#ifdef LIGHTMAP_ON
			// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_WORLD_POS
			struct v2f_surf {
				UNITY_POSITION(pos);
				float2 pack0 : TEXCOORD0; // _MainTex
				float3 worldNormal : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				fixed4 color : COLOR0;
				float4 lmap : TEXCOORD3;
				UNITY_LIGHTING_COORDS(4, 5)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
#endif
			// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
			struct v2f_surf {
				UNITY_POSITION(pos);
				float2 pack0 : TEXCOORD0; // _MainTex
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				fixed4 color : COLOR0;
				float4 lmap : TEXCOORD3;
				UNITY_FOG_COORDS(4)
				UNITY_SHADOW_COORDS(5)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
#endif
#endif
/*
			float2 depthTest_HighCost(float3 localPos, float mirrorFlagF)
			{
				float4 screenPos = ComputeScreenPos(UnityObjectToClipPos(localPos - float3(0.5, 0.5, 0.5)));

				float2 screenUV = screenPos.xy / screenPos.w;
				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);

				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
				#endif

				float2 uv = (screenUV - 0.5) * 2.0;
				float3 ray = float3((uv.x - mirrorFlagF * MF_PROJMAT[0][2] * 1.0) / MF_PROJMAT[0][0], (uv.y - MF_PROJMAT[1][2] * 1.0) / MF_PROJMAT[1][1], 1.0);
				float A = mirrorFlagF * MF_PROJMAT[2][0] * ray.x + MF_PROJMAT[2][1] * ray.y + MF_PROJMAT[2][2] * ray.z;
				float d = 1.0 * MF_PROJMAT[2][3];
				float linear_view_depth = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);

				float dd = abs(UnityObjectToViewPos(localPos - float3(0.5, 0.5, 0.5)).z);
				float dtest = (dd <= linear_view_depth) ? dd : -dd;

				return float2(dtest, linear_view_depth);
			}
*/
			float2 depthTest_HighCost_Approx(float3 localPos, float mirrorFlagF, float A, float d)
			{
				float4 screenPos = ComputeScreenPos(UnityObjectToClipPos(localPos - float3(0.5, 0.5, 0.5)));

				float2 screenUV = screenPos.xy / screenPos.w;
				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);

				float linear_view_depth = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);

				float dd = abs(UnityObjectToViewPos(localPos - float3(0.5, 0.5, 0.5)).z);
				float dtest = (dd <= linear_view_depth) ? dd : -dd;

				return float2(dtest, linear_view_depth);
			}
/*
			float4 GetPrevGrabUV1px(float3 localPos2, float3 rayStep, float zTolerance, float mirrorFlagF)
			{
				float4 clipPos = UnityObjectToClipPos(float4(localPos2 - rayStep - float3(0.5, 0.5, 0.5), 1.0));
				float4 clipPos2 = UnityObjectToClipPos(float4(localPos2 - float3(0.5, 0.5, 0.5), 1.0));
				
				float4 screenPos = ComputeScreenPos(clipPos);
				float4 screenPos2 = ComputeScreenPos(clipPos2);
				float2 screenUV = screenPos.xy / screenPos.w;
				float2 screenUV2 = screenPos2.xy / screenPos2.w;
				float2 screenUV_dir_1px = normalize(screenUV2*_ScreenParams.xy - screenUV*_ScreenParams.xy) / _ScreenParams.xy * 1.5;

				float4 grabPos = ComputeGrabScreenPos(clipPos);
				float4 grabPos2 = ComputeGrabScreenPos(clipPos2);
				float2 grabUV = grabPos.xy / grabPos.w;
				float2 grabUV2 = grabPos2.xy / grabPos2.w;
				float2 grabUV_dir_1px = normalize(grabUV2*_ScreenParams.xy - grabUV * _ScreenParams.xy) / _ScreenParams.xy * 1.5;

				screenUV = screenUV2 + screenUV_dir_1px;
				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
				float2 uv = (screenUV - 0.5) * 2.0;
				float3 ray = float3((uv.x - mirrorFlagF * MF_PROJMAT[0][2] * 1.0) / MF_PROJMAT[0][0], (uv.y - MF_PROJMAT[1][2] * 1.0) / MF_PROJMAT[1][1], 1.0);
				float A = mirrorFlagF * MF_PROJMAT[2][0] * ray.x + MF_PROJMAT[2][1] * ray.y + MF_PROJMAT[2][2] * ray.z;
				float d = 1.0 * MF_PROJMAT[2][3];
				float linear_view_depth = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);

				float next_viewdepth = abs(UnityObjectToViewPos(localPos2 - float3(0.5, 0.5, 0.5) + rayStep).z);
				float dtest = (next_viewdepth - linear_view_depth > zTolerance) ? 1.0 : 0.0;

				return float4(grabUV2 - dtest * grabUV_dir_1px, dtest, next_viewdepth - linear_view_depth);
			}
*/
			float2 GetPrevGrabUV1px2(float3 localPos, float zTolerance, float mirrorFlagF)
			{
				float4 clipPos = UnityObjectToClipPos(float4(localPos - float3(0.5, 0.5, 0.5), 1.0));

				float4 screenPos = ComputeScreenPos(clipPos);
				float2 screenUV = screenPos.xy / screenPos.w;

				float4 grabPos = ComputeGrabScreenPos(clipPos);
				float2 grabUV = grabPos.xy / grabPos.w;

				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				float2 uv = ( (screenUV - scaleOffset.zw) / scaleOffset.xy - 0.5 ) * 2.0;
				float2 screendim = float2(_ScreenParams.x * 2.0, _ScreenParams.y);
#else
				float2 uv = (screenUV - 0.5) * 2.0;
				float2 screendim = _ScreenParams.xy;
#endif
				float3 ray = float3((uv.x - mirrorFlagF * MF_PROJMAT[0][2] * 1.0) / MF_PROJMAT[0][0], (uv.y - MF_PROJMAT[1][2] * 1.0) / MF_PROJMAT[1][1], 1.0);
				float A = mirrorFlagF * MF_PROJMAT[2][0] * ray.x + MF_PROJMAT[2][1] * ray.y + MF_PROJMAT[2][2] * ray.z;
				float d = 1.0 * MF_PROJMAT[2][3];

				float2 screenUV_dir = 1.0 / screendim * 4.0;

				float linear_view_depth = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);
				
				sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV + float2(screenUV_dir.x, 0.0));
				float linear_view_depth1 = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);
				sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV - float2(screenUV_dir.x, 0.0));
				float linear_view_depth2 = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);
				sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV + float2(0.0, screenUV_dir.y));
				float linear_view_depth3 = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);
				sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV - float2(0.0, screenUV_dir.y));
				float linear_view_depth4 = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);

				float viewdepth = abs(UnityObjectToViewPos(localPos - float3(0.5, 0.5, 0.5)).z);
				float dtest1 = (viewdepth - linear_view_depth1 > zTolerance) ? 1.0 : 0.0;
				float dtest2 = (viewdepth - linear_view_depth2 > zTolerance) ? 1.0 : 0.0;
				float dtest3 = (viewdepth - linear_view_depth3 > zTolerance) ? 1.0 : 0.0;
				float dtest4 = (viewdepth - linear_view_depth4 > zTolerance) ? 1.0 : 0.0;

				float2 dif = float2(dtest1 - dtest2, dtest3 - dtest4) / screendim;

#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
#else
				float scale = 1.0;
#endif
				dif.y = dif.y * scale * _ProjectionParams.x;

				return grabUV - dif * 4.0;
			}
/*
			float depthTest(float3 localPos, float linear_view_depth)
			{
				float dd = abs(UnityObjectToViewPos(localPos - float3(0.5, 0.5, 0.5)).z);
				float dtest = (dd <= linear_view_depth) ? dd : -dd;

				return dtest;
			}
*/
			float getVolumeIntensity(float3 localPos)
			{
				float4 c_sm = 1.0 - tex2Dlod(_MochiMask, float4(localPos.xz, 0.0, 0.0));
				fixed4 c_clip = tex2Dlod(_MochiMask2, float4(localPos.xz, 0.0, 0.0));

				float inside = (localPos.x >= 0.0 && localPos.x <= 1.0 && localPos.y >= 0.0 && localPos.y <= 1.0 && localPos.z >= 0.0 && localPos.z <= 1.0 && c_clip.r > 0.99 && localPos.y <= c_sm.r) ? 1.0 : 0.0;
				
				return inside * c_sm.r;
			}

			float getY(float2 localPos)
			{
				float4 c_sm = 1.0 - tex2Dlod(_MochiMask, float4(localPos, 0.0, 0.0));
				fixed4 c_clip = tex2Dlod(_MochiMask2, float4(localPos, 0.0, 0.0));

				float inside = (localPos.x >= 0.0 && localPos.x <= 1.0 && localPos.y >= 0.0 && localPos.y <= 1.0 && c_clip.r > 0.99) ? 1.0 : 0.0;

				return inside * c_sm.r;
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

			fixed4 MyLightingStandard(float3 worldPos, float3 worldNormal, float2 uv)
			{
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf, o);
				//UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
				fixed3 worldTangent = UnityObjectToWorldDir(float3(0.0, 1.0, 0.0));
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
#endif
#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
#endif
				o.worldPos.xyz = worldPos;
				o.worldNormal = worldNormal;
				o.color = fixed4(1.0, 1.0, 1.0, 1.0);
#ifdef LIGHTMAP_ON
				o.lmap.xy = half2(0.0, 0.0) * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
				// SH/ambient and vertex lights
#ifndef LIGHTMAP_ON
#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				o.sh = 0;
				// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
				o.sh += Shade4PointLights(
					unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, worldPos, worldNormal);
#endif
				o.sh = ShadeSHPerVertex(worldNormal, o.sh);
#endif
#endif // !LIGHTMAP_ON

				UNITY_TRANSFER_LIGHTING(o, half2(0.0, 0.0)); // pass shadow and, possibly, light cookie coordinates to pixel shader
#ifdef FOG_COMBINED_WITH_TSPACE
				UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o, o.pos); // pass fog coordinates to pixel shader
#elif defined (FOG_COMBINED_WITH_WORLD_POS)
				UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o, o.pos); // pass fog coordinates to pixel shader
#else
				UNITY_TRANSFER_FOG(o, o.pos); // pass fog coordinates to pixel shader
#endif


#ifdef USING_STEREO_MATRICES
				float3 w_vdir = normalize((unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5 - worldPos);
#else
				float3 w_vdir = normalize(_WorldSpaceCameraPos - worldPos);
#endif
				float3 v_normal = normalize(mul(float4(worldNormal, 1), UNITY_MATRIX_I_V).xyz);
				float3 v_vdir = mul((float3x3)UNITY_MATRIX_V, w_vdir);
				float3 matcap_uv = getMatcapUV(v_normal, v_vdir);
				matcap_uv.xy = mul(matcap_uv.xy, matcapUVRotMat());
				float4 matcap_col = tex2Dlod(_MatCap, float4(matcap_uv.xy * 0.5 + 0.5, 0.0, 0.0));

				float4 texcol = tex2Dlod(_MainTex, float4(uv, 0.0, 0.0)) * _Color;

				SurfaceOutputStandard so;
				UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, so);
				so.Albedo = clamp(texcol.rgb, 0.0, 1.0);
				so.Metallic = _Metallic;
				so.Smoothness = _Glossiness;
				so.Emission = clamp(_MatCapPower * matcap_col, 0.0, 1.0);
				so.Alpha = 1.0 - _Transparency;
				so.Occlusion = 1.0;
				so.Normal = worldNormal;

#ifndef USING_DIRECTIONAL_LIGHT
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
#else
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;
#endif
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				UNITY_LIGHT_ATTENUATION(atten, o, worldPos)
				fixed4 c = 0;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = o.lmap;
#else
				giInput.lightmapUV = 0.0;
#endif
#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				giInput.ambient = o.sh;
#else
				giInput.ambient.rgb = 0.0;
#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif
#ifdef UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
#endif
				LightingStandard_GI(so, giInput, gi);

				c += LightingStandard(so, worldViewDir, gi);
				c.rgb += so.Emission;
				//UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
				
				return c;
			}

            fixed4 frag (v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float mirrorFlagF = i.mirrorFlagF;

				float2 screenUV = i.scrPos.xy / i.scrPos.w;
				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
				float2 uv = (screenUV - 0.5) * 2.0;
				float3 ray = float3((uv.x - mirrorFlagF * MF_PROJMAT[0][2] * 1.0) / MF_PROJMAT[0][0], (uv.y - MF_PROJMAT[1][2] * 1.0) / MF_PROJMAT[1][1], 1.0);
				float A = mirrorFlagF * MF_PROJMAT[2][0] * ray.x + MF_PROJMAT[2][1] * ray.y + MF_PROJMAT[2][2] * ray.z;
				float d = 1.0 * MF_PROJMAT[2][3];


				float3 worldPos = i.worldPos;
				float3 worldDir = normalize(i.worldPos - _WorldSpaceCameraPos);
				float3 localDir = normalize(mul((float3x3)unity_WorldToObject, worldDir));
				float3 local_slicedir = normalize(mul(transpose((float3x3)unity_ObjectToWorld), unity_CameraToWorld._m02_m12_m22));
				float3 local_campos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

				float2 tminmax = i.tminmax;
				float3 startPos = local_campos + localDir * (tminmax.x / dot(local_slicedir, localDir)) + float3(0.5, 0.5, 0.5);
				float sample_rate = (tminmax.y - tminmax.x) / _StepNum;
				float step = ((tminmax.y - tminmax.x) / _StepNum) / dot(local_slicedir, localDir);

				float4 outcol = float4(0.0, 0.0, 0.0, 0.0);
				float3 dir = float3(1.0, 1.0, 1.0) / _TexResoution;
				float3 localPos[2];
				float val[2];
				float2 dtest[2];
				localPos[1] = startPos + localDir * step * -1.0;
				val[1] = getVolumeIntensity(localPos[1]);
				dtest[1] = float2(1.0, 1.0);
/*
				float4 subtexval;
				float3 sub_campos;
				float4x4 sub_projmat;
				float4x4 sub_viewmat;
				subtexval = tex2Dlod(_SubCameraMatrixTex, float4(0.125, 0.125, 0.0, 0.0));
				sub_campos = subtexval.xyz;
				sub_projmat[3][2] = subtexval.w;
				subtexval = tex2Dlod(_SubCameraMatrixTex, float4(0.125 + 0.25 * 1, 0.125, 0.0, 0.0));
				sub_projmat[0][0] = subtexval.x; sub_projmat[1][1] = subtexval.y; sub_projmat[2][2] = subtexval.z; sub_projmat[2][3] = subtexval.w;
				subtexval = tex2Dlod(_SubCameraMatrixTex, float4(0.125 + 0.25 * 2, 0.125, 0.0, 0.0));
				sub_viewmat[0][0] = subtexval.x; sub_viewmat[0][1] = subtexval.y; sub_viewmat[0][2] = subtexval.z; sub_viewmat[0][3] = subtexval.w;
				subtexval = tex2Dlod(_SubCameraMatrixTex, float4(0.125 + 0.25 * 3, 0.125, 0.0, 0.0));
				sub_viewmat[1][0] = subtexval.x; sub_viewmat[1][1] = subtexval.y; sub_viewmat[1][2] = subtexval.z; sub_viewmat[1][3] = subtexval.w;
				subtexval = tex2Dlod(_SubCameraMatrixTex, float4(0.125, 0.125 + 0.25 * 1, 0.0, 0.0));
				sub_viewmat[2][0] = subtexval.x; sub_viewmat[2][1] = subtexval.y; sub_viewmat[2][2] = subtexval.z; sub_viewmat[2][3] = subtexval.w;
				sub_viewmat[3][0] = 0.0; sub_viewmat[3][1] = 0.0; sub_viewmat[3][2] = 0.0; sub_viewmat[3][3] = 1.0;
*/				
				for (int i = 0; i <= _StepNum; i++)
				{
					float4 c = float4(0.0, 0.0, 0.0, 0.0);
					localPos[0] = localPos[1];
					localPos[1] = localPos[1] + localDir * step;

					val[0] = val[1];
					val[1] = getVolumeIntensity(localPos[1]);

					dtest[0] = dtest[1];


					if (dtest[1].x > 0.0)
					{
						if (val[0] * val[1] > 0.000001) // inside
						{
							dtest[1] = depthTest_HighCost_Approx(localPos[1], mirrorFlagF, A, d);
							if (dtest[1].x > 0.0)
							{
								c = _FogColor;
								float alpha = 1.0 - pow(clamp(1.0 - c.w, 0.0, 1.0), sample_rate);
								c = float4(c.rgb * alpha, c.w*alpha);
							}
							else if (-dtest[1].x - dtest[1].y > (-dtest[1].x - dtest[0].x)*2.0)
							{
								dtest[1].x = -dtest[1].x;

								localPos[1] = localPos[0];
								val[1] = val[0];
								dtest[1] = dtest[0];

								worldDir = normalize(worldPos - _WorldSpaceCameraPos);
								localDir = normalize(mul((float3x3)unity_WorldToObject, worldDir));
								step = ((tminmax.y - tminmax.x) / _StepNum) / dot(local_slicedir, localDir);

								c = float4(0.0, 0.0, 0.0, 0.0);
							}
							else if (dtest[0].x > 0.0)
							{
								c = _FogColor;
								float depth_fix = abs(dtest[1].y - dtest[0].x) / abs(-dtest[1].x - dtest[0].x);
								float alpha = 1.0 - pow(clamp(1.0 - c.w, 0.0, 1.0), sample_rate * depth_fix);
								c = float4(c.rgb * alpha, c.w*alpha);

								outcol = outcol + (1.0 - outcol.w) * c;

								float3 final_loc_pos = localPos[1] - localDir * step * abs(-dtest[1].x - dtest[1].y) / abs(-dtest[1].x - dtest[0].x);
								float2 bgpos = GetPrevGrabUV1px2(localPos[0], (-dtest[1].x - dtest[0].x)*2.0, mirrorFlagF);
								float4 bgcolor = tex2Dlod(_BackgroundTexture2, float4(bgpos, 0.0, 0.0));
								//float4 bgpos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(final_loc_pos - float3(0.5, 0.5, 0.5), 1.0)));
								//float4 bgcolor = tex2Dlod(_BackgroundTexture2, float4(bgpos.xy / bgpos.w, 0.0, 0.0));
								c = /*bgpos.z > 0.0 ? float4(1.0, 0.0, 1.0, 0.5) : */bgcolor;
							}
						}
						else if ( val[0] + val[1] > 0.000001) // surface
						{
							float3 tmp_pos[2];
							float3 mpos;

							tmp_pos[0] = localPos[0];
							tmp_pos[1] = localPos[1];
							localPos[0] = (val[0] == 0.0) ? tmp_pos[0] : tmp_pos[1];
							localPos[1] = (val[0] == 0.0) ? tmp_pos[1] : tmp_pos[0];

							for (int j = 0; j < _SurfaceSearchIteration; j++)
							{
								mpos = (localPos[0] + localPos[1]) * 0.5;
								float mval = getVolumeIntensity(mpos);
								localPos[0] = (mval == 0) ? mpos : localPos[0];
								localPos[1] = (mval == 0) ? localPos[1] : mpos;
							}

							dtest[1] = depthTest_HighCost_Approx(localPos[1], mirrorFlagF, A, d);

							if (dtest[1].x > 0.0)
							{
								float2 texCoord = localPos[1].xz;
								float3 dx, dy;
								float2 w;

								w = float2(dir.x, 0.0);
								dx = float3(dir.x * 2.0, getY(texCoord + w) - getY(texCoord - w), 0.0);
								w = float2(0, dir.y);
								dy = float3(0.0, getY(texCoord + w) - getY(texCoord - w), dir.y * 2.0);
								
								float3 local_normal = tmp_pos[0].y*tmp_pos[1].y > 0.000001 ? normalize(cross(dy, dx)) : float3(0.0, -1.0, 0.0);
								float3 world_normal = UnityObjectToWorldNormal(local_normal); //mul(transpose((float3x3)unity_WorldToObject), local_normal);

								c = MyLightingStandard(mul(unity_ObjectToWorld, float4(localPos[1] - float3(0.5, 0.5, 0.5), 1.0)).xyz, world_normal, texCoord);
								c = float4(c.rgb * c.w, c.w);

								//refraction
								worldDir = refract(worldDir, val[1] > 0.000001 ? normalize(world_normal) : -normalize(world_normal), 1.0 / _refractiveIndex);
								localDir = normalize(mul((float3x3)unity_WorldToObject, worldDir));
								step = ((tminmax.y - tminmax.x) / _StepNum) / dot(local_slicedir, localDir);
							}
							else if (dtest[0].x > 0.0)
							{
								float3 final_loc_pos = localPos[1] - localDir * step * abs(-dtest[1].x - dtest[1].y) / abs(-dtest[1].x - dtest[0].x) - float3(0.5, 0.5, 0.5);
								float4 bgpos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(final_loc_pos, 1.0)));
								float4 bgcolor = tex2Dlod(_BackgroundTexture2, float4(bgpos.xy / bgpos.w, 0.0, 0.0));
								c = bgcolor;
							}
						}
						else
						{
							dtest[1] = depthTest_HighCost_Approx(localPos[1], mirrorFlagF, A, d);
							if (dtest[1].x <= 0.0 && dtest[0].x > 0.0)
							{
								float3 final_loc_pos = localPos[1] - localDir * step * abs(-dtest[1].x - dtest[1].y) / abs(-dtest[1].x - dtest[0].x) - float3(0.5, 0.5, 0.5);
								float4 bgpos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(final_loc_pos, 1.0)));
								float4 bgcolor = tex2Dlod(_BackgroundTexture2, float4(bgpos.xy / bgpos.w, 0.0, 0.0));
								c = bgcolor;
							}
						}
					}
					outcol = outcol + (1.0 - outcol.w) * c;
				}

				if ((outcol.x*outcol.x + outcol.y*outcol.y + outcol.z*outcol.z + outcol.w*outcol.w) > 0.000001 && dtest[1].x > 0.0)
				{
					float3 final_loc_pos = localPos[1] - float3(0.5, 0.5, 0.5);
					float4 bgpos = ComputeGrabScreenPos(UnityObjectToClipPos(float4(final_loc_pos, 1.0)));
					float4 bgcolor = tex2Dlod(_BackgroundTexture2, float4(bgpos.xy / bgpos.w, 0.0, 0.0));
					float4 c = bgcolor;
					outcol = outcol + (1.0 - outcol.w) * c;
				}
			
				return (outcol.x*outcol.x + outcol.y*outcol.y + outcol.z*outcol.z + outcol.w*outcol.w) > 0.000001 ? fixed4(outcol.rgb, 1.0) : fixed4(0.0, 0.0, 0.0, 0.0);
            }
			#endif
            ENDCG
		}
	}
		FallBack "Diffuse"
}
