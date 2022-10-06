// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "omochi/MochiMochiShaderVolume" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_FogColor("FogColor", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MochiMask2("Mask for Clipping", 2D) = "black" {}
		_MatCap("MatCap (Add)", 2D) = "black" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_RefractionStrength("Strength", Range(0,1)) = 0.1
		[PowerSlider(3.0)]_RefractionFresnelPower("Fresnel Power", Range(0.01, 10)) = 0.5
		_Transparency("Transparency", Range(0,1)) = 0.0
		[Space(30)]
		_MochiMask ("Custom Render Texture", 2D) = "black" {}
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
			#pragma multi_compile_instancing
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

			//static const int _StepNum = 10;
			#define _StepNum 128
			#define _SurfaceSearchIteration 4
			#define _TexResoution 1024

			half _Glossiness;
			half _Metallic;
			half _Transparency;
			fixed4 _Color;
			fixed4 _FogColor;
			fixed4 _BlendColorMochi;

			float _SamplingInterval;

			float _RefractionStrength;
			float _RefractionFresnelPower;
			
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
				UNITY_FOG_COORDS(6)

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

			float depthTest(float3 localPos, float linear_view_depth)
			{
				float dd = abs(UnityObjectToViewPos(localPos - float3(0.5, 0.5, 0.5)).z);
				float dtest = (dd <= linear_view_depth) ? dd : -dd;

				return dtest;
			}

			float getVolumeIntensity(float3 localPos)
			{
				float4 c_sm = 1.0 - tex2D(_MochiMask, localPos.xz);
				fixed4 c_clip = tex2D(_MochiMask2, localPos.xz);

				float inside = (localPos.x >= 0.0 && localPos.x <= 1.0 && localPos.y >= 0.0 && localPos.y <= 1.0 && localPos.z >= 0.0 && localPos.z <= 1.0 && c_clip.r > 0.99 && localPos.y <= c_sm.r) ? 1.0 : 0.0;
				
				return inside * c_sm.r;
			}

			float getY(float2 localPos)
			{
				float4 c_sm = 1.0 - tex2D(_MochiMask, localPos);
				fixed4 c_clip = tex2D(_MochiMask2, localPos);

				float inside = (localPos.x >= 0.0 && localPos.x <= 1.0 && localPos.y >= 0.0 && localPos.y <= 1.0 && c_clip.r > 0.99) ? 1.0 : 0.0;

				return inside * c_sm.r;
			}

            fixed4 frag (v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 screenUV = i.scrPos.xy / i.scrPos.w;
				float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);

#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
#endif
				float2 uv = (screenUV - 0.5) * 2.0;
				float3 ray = float3((uv.x - i.mirrorFlagF * MF_PROJMAT[0][2] * 1.0) / MF_PROJMAT[0][0], (uv.y - MF_PROJMAT[1][2] * 1.0) / MF_PROJMAT[1][1], 1.0);
				float A = i.mirrorFlagF * MF_PROJMAT[2][0] * ray.x + MF_PROJMAT[2][1] * ray.y + MF_PROJMAT[2][2] * ray.z;
				float d = 1.0 * MF_PROJMAT[2][3];
				float linear_view_depth = (sceneZ + A < 0.00001) ? 100000.0 : d / (sceneZ + A);

				float3 worldDir = normalize(i.worldPos - _WorldSpaceCameraPos);
				float3 localDir = normalize(mul((float3x3)unity_WorldToObject, worldDir));
				float3 local_slicedir = normalize(mul(transpose((float3x3)unity_ObjectToWorld), unity_CameraToWorld._m02_m12_m22));
				float3 local_campos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

				float3 startPos = local_campos + localDir * (i.tminmax.x / dot(local_slicedir, localDir)) + float3(0.5, 0.5, 0.5);
				float sample_rate = (i.tminmax.y - i.tminmax.x) / _StepNum;
				float step = ((i.tminmax.y - i.tminmax.x) / _StepNum) / dot(local_slicedir, localDir);

				float4 outcol = float4(0.0, 0.0, 0.0, 0.0);
				float3 dir = float3(1.0, 1.0, 1.0) / _TexResoution;
				float3 localPos[2];
				float val[2];
				float dtest[2];
				localPos[1] = startPos + localDir * step * -1.0;
				val[1] = getVolumeIntensity(localPos[1]);
				dtest[1] = 1.0;
				for (int i = 0; i <= _StepNum; i++)
				{
					float4 c = float4(0.0, 0.0, 0.0, 0.0);
					localPos[0] = localPos[1];
					localPos[1] = startPos + localDir * step * float(i);

					val[0] = val[1];
					val[1] = getVolumeIntensity(localPos[1]);

					dtest[0] = dtest[1];
					dtest[1] = 1.0;

					if (val[0] * val[1] > 0.000001) // inside
					{
						dtest[1] = depthTest(localPos[1], linear_view_depth);
						if (dtest[1] > 0.0)
						{
							c = _FogColor;
							float alpha = 1.0 - pow(clamp(1.0 - c.w, 0.0, 1.0), sample_rate);
							c = float4(c.rgb * alpha, c.w*alpha);
						}
						else if (dtest[0] > 0.0)
						{
							c = _FogColor;
							float depth_fix = abs(linear_view_depth - dtest[0]) / abs(-dtest[1] - dtest[0]);
							float alpha = 1.0 - pow(clamp(1.0 - c.w, 0.0, 1.0), sample_rate * depth_fix);
							c = float4(c.rgb * alpha, c.w*alpha);
						}
					}
					else if (val[0] + val[1] > 0.000001) // surface
					{
						float3 tmp_pos[2];
						float3 mpos;

						tmp_pos[0] = localPos[0];
						tmp_pos[1] = localPos[1];
						localPos[0] = (val[0] == 0.0) ? tmp_pos[0] : tmp_pos[1];
						localPos[1] = (val[0] == 0.0) ? tmp_pos[1] : tmp_pos[0];

						[unroll]
						for (int j = 0; j < _SurfaceSearchIteration; j++)
						{
							mpos = (localPos[0] + localPos[1]) * 0.5;
							float mval = getVolumeIntensity(mpos);
							localPos[0] = (mval == 0) ? mpos : localPos[0];
							localPos[1] = (mval == 0) ? localPos[1] : mpos;
						}

						dtest[1] = depthTest(localPos[1], linear_view_depth);

						if (dtest[1] > 0.0)
						{
							float2 texCoord = localPos[1].xz;
							float3 e1, e2, e3, e4;
							float2 w, p;

							float v = getY(texCoord);

							w = float2(0.0, 0.0);
							w.x = dir.x;
							p = clamp(texCoord + w, 0.0, 1.0);
							e1 = float3(w.x, getY(p) - v, w.y);
							p = clamp(texCoord - w, 0.0, 1.0);
							e3 = float3(-w.x, getY(p) - v, w.y);

							w = float2(0.0, 0.0);
							w.y = dir.y;
							p = clamp(texCoord + w, 0.0, 1.0);
							e4 = float3(w.x, getY(p) - v, w.y);
							p = clamp(texCoord - w, 0.0, 1.0);
							e2 = float3(w.x, getY(p) - v, -w.y);

							//float3 world_normal = mul(transpose((float3x3)unity_WorldToObject), (cross(e1, e2) + cross(e2, e3) + cross(e3, e4) + cross(e4, e1)) * 0.25);
							float3 world_normal = (normalize(cross(e1, e2)) + normalize(cross(e2, e3)) + normalize(cross(e3, e4)) + normalize(cross(e4, e1))) * 0.25;

							c = float4(world_normal, 1.0 - _Transparency);

							float alpha = c.w;
							c = float4(c.rgb * alpha, alpha);

							outcol = outcol + (1.0 - outcol.w) * c;
							
							float suf_fix = abs(dot(localPos[1] - tmp_pos[1], localDir));
							c = _FogColor;
							alpha = 1.0 - pow(clamp(1.0 - c.w, 0.0, 1.0), (sample_rate * suf_fix / step));
							c = float4(c.rgb * alpha, c.w*alpha);
						}
					}
					outcol = outcol + (1.0 - outcol.w) * c;
				}
	
				return fixed4(outcol);
            }
			#endif
            ENDCG
		}
	}
		FallBack "Diffuse"
}
