Shader "CRTBlurY"
{
	Properties{
		_MainTex("Depth", 2D) = "white" {}
		_MainTex2("Depth2", 2D) = "white" {}
		//_MaskTex("Mask", 2D) = "white" {}
		_sigma("Sigma", Range(0.01,100.0)) = 10.0
		_resolution("Resolution", Range(0,1024)) = 1024.0
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
	Pass
	{
		Name "Update"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _MainTex;
		sampler2D _MainTex2;
		//sampler2D _MaskTex;

		float _sigma;
		int _resolution;

#define KERNEL_RAD 8
#define PI 3.14159265
#define R2 1.41421356

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
	{
		float2 uv = i.globalTexcoord;
		uv.x = 1.0 - uv.x;
		
		//fixed4 m = tex2D(_MaskTex, uv);

		fixed4 r = /*m.a > 0.5 ? */tex2D(_MainTex, uv)/* : fixed4(0.0, 0.0, 0.0, 0.0)*/;
		fixed4 n = /*m.a > 0.5 ? */tex2D(_MainTex2, uv)/* : fixed4(0.0, 0.0, 0.0, 0.0)*/;

		float4 sum = float4(0.0, 0.0, 0.0, 0.0);
		float2 tc = uv;
		float blur = 1.0 / (float)_resolution;

		//blur radius in pixels
/*
		float weight_sum = 0.0;
		for (int yy = -KERNEL_RAD; yy <= KERNEL_RAD; yy++)
		{
			for (int xx = -KERNEL_RAD; xx <= KERNEL_RAD; xx++)
			{
				float weight = (float)exp(-((xx * xx + yy * yy) / (2 * _sigma * _sigma))) / (2 * PI * _sigma * _sigma);
				sum += tex2D(_MainTex, float2(tc.x + xx * blur, tc.y + yy * blur)) * weight * 1.0;
				weight_sum += weight;
			}
		}
		sum = (sum / weight_sum * 1.0);
*/
		float weight_sum = 0.0;
		for (int yy = -KERNEL_RAD; yy <= KERNEL_RAD; yy++)
		{
			float weight = (float)exp(-((yy * yy) / (2 * _sigma * _sigma))) / (2 * PI * _sigma * _sigma);
			sum += tex2D(_MainTex, float2(tc.x , tc.y + yy * blur)) * weight * 1.0;
			weight_sum += weight;
		}
		sum = (sum / weight_sum * 1.0);
		sum = max(n, sum);
		return fixed4(sum.r, 0.0, 0.0, sum.a);
	}

		ENDCG
	}

	}
}