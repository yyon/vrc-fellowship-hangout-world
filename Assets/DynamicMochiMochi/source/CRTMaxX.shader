Shader "CRTMaxX"
{
	Properties{
		_MainTex("Depth", 2D) = "white" {}
		_MainTex2("Depth2", 2D) = "black" {}
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

#define KERNEL_RAD 6
#define PI 3.14159265
#define R2 1.41421356

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
	{ 
		float2 uv = i.globalTexcoord;
		//uv.x = 1.0 - uv.x;

		fixed4 r = /*m.a > 0.5 ? */tex2D(_MainTex, uv)/* : fixed4(0.0, 0.0, 0.0, 0.0)*/;
		fixed4 n = /*m.a > 0.5 ? */tex2D(_MainTex2, uv)/* : fixed4(0.0, 0.0, 0.0, 0.0)*/;

		float2 tc = uv;
		float blur = 1.0 / (float)_resolution;

		float4 maxval = float4(0.0, 0.0, 0.0, 0.0);
		for (int xx = -KERNEL_RAD; xx <= KERNEL_RAD; xx++)
		{
			float4 val = tex2D(_MainTex, float2(tc.x + xx * blur, tc.y));
			maxval = max(maxval, val);
		}
		return max(n, maxval);
	}

		ENDCG
	}

	}
}