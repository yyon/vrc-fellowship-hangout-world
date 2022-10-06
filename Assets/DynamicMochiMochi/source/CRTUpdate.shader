Shader "CRTUpdate"
{
	Properties{
		_MaskTex("Mask", 2D) = "white" {}
		_HeightTex("Height", 2D) = "black" {}

		_max_height("MaxHeight", Range(0,1)) = 1.0
		
		_stiffness("Stiffness", Range(0.0, 0.999)) = 0.85
		_elasticity("Elasticity", Range(0.0, 0.5)) = 0.04
		_damping("Damping", Range(0.0, 1.0)) = 0.3
		
		[Space(30)]
		_MainTex("Depth", 2D) = "white" {}

		_sigma("Sigma", Range(0.01,100.0)) = 1.3
		_resolution1("Resolution1", Range(0,1024)) = 52.0
		_resolution2("Resolution2", Range(0,1024)) = 128.0
		_resolution3("Resolution3", Range(0,1024)) = 256.0
		_resolution4("Resolution4", Range(0,1024)) = 512.0

	}

		CGINCLUDE

#define KERNEL_RAD 4
#define PI 3.14159265f
#define R2 1.41421356f
#define EP 0.00048828125f

		float	_max_height;

		float4 update_physics(float2 uv, float2 mhuv, sampler2D depthTex, sampler2D heightTex, sampler2D prevTex, sampler2D maskTex, float stiffness, float elasticity, float damping, float resolution, float sigma)
		{
			float2 uv_xmirror = fixed2(1.0 - mhuv.x, mhuv.y);
			float4 m = tex2D(maskTex, uv_xmirror);

			float h = tex2D(heightTex, uv_xmirror).r + (1.0 - _max_height);

			float depth = tex2D(depthTex, mhuv).r;
			float4 r = m.r > 0.5 ? float4(depth, 0.0, 0.0, depth) : float4(0.0, 0.0, 0.0, 0.0);

			float4 c = tex2D(prevTex, uv);
			float4 sum = float4(0.0, 0.0, 0.0, 0.0);
			float blur = 0.5 / (float)resolution;

			float4 res = float4(0.0, 0.0, 0.0, 0.0);

			//Acceleration
			float dx1 = 0.0;
			float dx2 = 0.0;
			float dy1 = 0.0;
			float dy2 = 0.0;
			float weight_sum = 0.0;
			for (int k = 1; k <= KERNEL_RAD; k++)
			{
				float weight = (float)exp(-(((k - 1)*(k - 1)) / (2 * sigma * sigma))) / (2 * PI * sigma * sigma);
				dx1 += (tex2D(prevTex, float2(uv.x + k * blur, uv.y)).r - c.r) * weight;
				dx2 += (tex2D(prevTex, float2(uv.x - k * blur, uv.y)).r - c.r) * weight;
				dy1 += (tex2D(prevTex, float2(uv.x, uv.y + k * blur)).r - c.r) * weight;
				dy2 += (tex2D(prevTex, float2(uv.x, uv.y - k * blur)).r - c.r) * weight;
				weight_sum += weight;
			}

			dx1 = sign(dx1) * abs(dx1) / weight_sum;
			dx2 = sign(dx2) * abs(dx2) / weight_sum;
			dy1 = sign(dy1) * abs(dy1) / weight_sum;
			dy2 = sign(dy2) * abs(dy2) / weight_sum;

			float ap = (c.g - 0.5) * 2.0;
			float vp = (c.b - 0.5) * 2.0;

			float dx = 0.5 * -1.0 * (c.r - (dx1 + dx2) / 2.0) * 0.5;
			float dy = 0.5 * -1.0 * (c.r - (dy1 + dy2) / 2.0) * 0.5;
			float s = 0.25 * (dx1 + dx2 + dy1 + dy2);
			float e = h - c.r;
			float d = -vp;

			float a = stiffness * s + elasticity * e + damping * d;

			//Velocity
			float v = (a - ap) * 0.5 + ap;

			//Position
			float p = c.r + (a - ap) * 0.5 * 0.333333 + ap;

			res.r = max(max(r.r, p), h);
			res.a = max(max(r.a, p), h);

			res.g = (p > r.r && m.a > 0.5) ? (a + 1.0) * 0.5 : 0.5;
			res.b = (p > r.r && m.a > 0.5) ? (v + 1.0) * 0.5 : 0.5;

			return res;
		}

		float4 update_physics4(float2 uv, float2 mhuv, sampler2D depthTex, sampler2D heightTex, sampler2D prevTex, sampler2D maskTex, float stiffness, float elasticity, float damping, float resolution, float sigma)
		{
			float2 uv_xmirror = fixed2(1.0 - mhuv.x, mhuv.y);
			float4 m = tex2D(maskTex, uv_xmirror);

			float h = tex2D(heightTex, uv_xmirror).r + (1.0 - _max_height);

			float depth = tex2D(depthTex, mhuv).r;
			float4 r = m.r > 0.5 ? float4(depth, 0.0, 0.0, depth) : float4(0.0, 0.0, 0.0, 0.0);

			float4 c = tex2D(prevTex, uv);
			float4 sum = float4(0.0, 0.0, 0.0, 0.0);
			float blur = 0.5 / (float)resolution;

			float4 res = float4(0.0, 0.0, 0.0, 0.0);

			//Acceleration
			float dx1 = 0.0;
			float dx2 = 0.0;
			float dy1 = 0.0;
			float dy2 = 0.0;
			float weight_sum = 0.0;
			for (int k = 1; k <= KERNEL_RAD; k++)
			{
				float weight = (float)exp(-(((k - 1)*(k - 1)) / (2 * sigma * sigma))) / (2 * PI * sigma * sigma);
				dx1 += (tex2D(prevTex, float2(uv.x + k * blur > 0.5 - EP ? 0.5 - EP : uv.x + k * blur, uv.y)).r - c.r) * weight;
				dx2 += (tex2D(prevTex, float2(uv.x - k * blur, uv.y)).r - c.r) * weight;
				dy1 += (tex2D(prevTex, float2(uv.x, uv.y + k * blur)).r - c.r) * weight;
				dy2 += (tex2D(prevTex, float2(uv.x, uv.y - k * blur < 0.5 + EP ? 0.5 + EP : uv.y - k * blur)).r - c.r) * weight;
				weight_sum += weight;
			}

			dx1 = sign(dx1) * abs(dx1) / weight_sum;
			dx2 = sign(dx2) * abs(dx2) / weight_sum;
			dy1 = sign(dy1) * abs(dy1) / weight_sum;
			dy2 = sign(dy2) * abs(dy2) / weight_sum;

			float ap = (c.g - 0.5) * 2.0;
			float vp = (c.b - 0.5) * 2.0;

			float dx = 0.5 * -1.0 * (c.r - (dx1 + dx2) / 2.0) * 0.5;
			float dy = 0.5 * -1.0 * (c.r - (dy1 + dy2) / 2.0) * 0.5;
			float s = 0.25 * (dx1 + dx2 + dy1 + dy2);
			float e = h - c.r;
			float d = -vp;

			float a = stiffness * s + elasticity * e + damping * d;

			//Velocity
			float v = (a - ap) * 0.5 + ap;

			//Position
			float p = c.r + (a - ap) * 0.5 * 0.333333 + ap;

			res.r = max(max(r.r, p), h);
			res.a = max(max(r.a, p), h);

			res.g = (p > r.r && m.a > 0.5) ? (a + 1.0) * 0.5 : 0.5;
			res.b = (p > r.r && m.a > 0.5) ? (v + 1.0) * 0.5 : 0.5;

			return res;
		}

		float4 update_physics3(float2 uv, float2 mhuv, sampler2D depthTex, sampler2D heightTex, sampler2D prevTex, sampler2D maskTex, float stiffness, float elasticity, float damping, float resolution, float sigma)
		{
			float2 uv_xmirror = fixed2(1.0 - mhuv.x, mhuv.y);
			float4 m = tex2D(maskTex, uv_xmirror);

			float h = tex2D(heightTex, uv_xmirror).r + (1.0 - _max_height);

			float depth = tex2D(depthTex, mhuv).r;
			float4 r = m.r > 0.5 ? float4(depth, 0.0, 0.0, depth) : float4(0.0, 0.0, 0.0, 0.0);

			float4 c = tex2D(prevTex, uv);
			float4 sum = float4(0.0, 0.0, 0.0, 0.0);
			float blur = 0.5 / (float)resolution;

			float4 res = float4(0.0, 0.0, 0.0, 0.0);

			//Acceleration
			float dx1 = 0.0;
			float dx2 = 0.0;
			float dy1 = 0.0;
			float dy2 = 0.0;
			float weight_sum = 0.0;
			for (int k = 1; k <= KERNEL_RAD; k++)
			{
				float weight = (float)exp(-(((k - 1)*(k - 1)) / (2 * sigma * sigma))) / (2 * PI * sigma * sigma);
				dx1 += (tex2D(prevTex, float2(uv.x + k * blur, uv.y)).r - c.r) * weight;
				dx2 += (tex2D(prevTex, float2(uv.x - k * blur < 0.5 + EP ? 0.5 + EP : uv.x - k * blur, uv.y)).r - c.r) * weight;
				dy1 += (tex2D(prevTex, float2(uv.x, uv.y + k * blur)).r - c.r) * weight;
				dy2 += (tex2D(prevTex, float2(uv.x, uv.y - k * blur < 0.5 + EP ? 0.5 + EP : uv.y - k * blur)).r - c.r) * weight;
				weight_sum += weight;
			}

			dx1 = sign(dx1) * abs(dx1) / weight_sum;
			dx2 = sign(dx2) * abs(dx2) / weight_sum;
			dy1 = sign(dy1) * abs(dy1) / weight_sum;
			dy2 = sign(dy2) * abs(dy2) / weight_sum;

			float ap = (c.g - 0.5) * 2.0;
			float vp = (c.b - 0.5) * 2.0;

			float dx = 0.5 * -1.0 * (c.r - (dx1 + dx2) / 2.0) * 0.5;
			float dy = 0.5 * -1.0 * (c.r - (dy1 + dy2) / 2.0) * 0.5;
			float s = 0.25 * (dx1 + dx2 + dy1 + dy2);
			float e = h - c.r;
			float d = -vp;

			float a = stiffness * s + elasticity * e + damping * d;

			//Velocity
			float v = (a - ap) * 0.5 + ap;

			//Position
			float p = c.r + (a - ap) * 0.5 * 0.333333 + ap;

			res.r = max(max(r.r, p), h);
			res.a = max(max(r.a, p), h);

			res.g = (p > r.r && m.a > 0.5) ? (a + 1.0) * 0.5 : 0.5;
			res.b = (p > r.r && m.a > 0.5) ? (v + 1.0) * 0.5 : 0.5;

			return res;
		}

		float4 update_physics2(float2 uv, float2 mhuv, sampler2D depthTex, sampler2D heightTex, sampler2D prevTex, sampler2D maskTex, float stiffness, float elasticity, float damping, float resolution, float sigma)
		{
			float2 uv_xmirror = fixed2(1.0 - mhuv.x, mhuv.y);
			float4 m = tex2D(maskTex, uv_xmirror);

			float h = tex2D(heightTex, uv_xmirror).r + (1.0 - _max_height);

			float depth = tex2D(depthTex, mhuv).r;
			float4 r = m.r > 0.5 ? float4(depth, 0.0, 0.0, depth) : float4(0.0, 0.0, 0.0, 0.0);

			float4 c = tex2D(prevTex, uv);
			float4 sum = float4(0.0, 0.0, 0.0, 0.0);
			float blur = 0.5 / (float)resolution;

			float4 res = float4(0.0, 0.0, 0.0, 0.0);

			//Acceleration
			float dx1 = 0.0;
			float dx2 = 0.0;
			float dy1 = 0.0;
			float dy2 = 0.0;
			float weight_sum = 0.0;
			for (int k = 1; k <= KERNEL_RAD; k++)
			{
				float weight = (float)exp(-(((k - 1)*(k - 1)) / (2 * sigma * sigma))) / (2 * PI * sigma * sigma);
				dx1 += (tex2D(prevTex, float2(uv.x + k * blur > 0.5 - EP ? 0.5 - EP : uv.x + k * blur, uv.y)).r - c.r) * weight;
				dx2 += (tex2D(prevTex, float2(uv.x - k * blur, uv.y)).r - c.r) * weight;
				dy1 += (tex2D(prevTex, float2(uv.x, uv.y + k * blur > 0.5 - EP ? 0.5 - EP : uv.y + k * blur)).r - c.r) * weight;
				dy2 += (tex2D(prevTex, float2(uv.x, uv.y - k * blur)).r - c.r) * weight;
				weight_sum += weight;
			}

			dx1 = sign(dx1) * abs(dx1) / weight_sum;
			dx2 = sign(dx2) * abs(dx2) / weight_sum;
			dy1 = sign(dy1) * abs(dy1) / weight_sum;
			dy2 = sign(dy2) * abs(dy2) / weight_sum;

			float ap = (c.g - 0.5) * 2.0;
			float vp = (c.b - 0.5) * 2.0;

			float dx = 0.5 * -1.0 * (c.r - (dx1 + dx2) / 2.0) * 0.5;
			float dy = 0.5 * -1.0 * (c.r - (dy1 + dy2) / 2.0) * 0.5;
			float s = 0.25 * (dx1 + dx2 + dy1 + dy2);
			float e = h - c.r;
			float d = -vp;

			float a = stiffness * s + elasticity * e + damping * d;

			//Velocity
			float v = (a - ap) * 0.5 + ap;

			//Position
			float p = c.r + (a - ap) * 0.5 * 0.333333 + ap;

			res.r = max(max(r.r, p), h);
			res.a = max(max(r.a, p), h);

			res.g = (p > r.r && m.a > 0.5) ? (a + 1.0) * 0.5 : 0.5;
			res.b = (p > r.r && m.a > 0.5) ? (v + 1.0) * 0.5 : 0.5;

			return res;
		}

		float4 update_physics1(float2 uv, float2 mhuv, sampler2D depthTex, sampler2D heightTex, sampler2D prevTex, sampler2D maskTex, float stiffness, float elasticity, float damping, float resolution, float sigma)
		{
			float2 uv_xmirror = fixed2(1.0 - mhuv.x, mhuv.y);
			float4 m = tex2D(maskTex, uv_xmirror);

			float h = tex2D(heightTex, uv_xmirror).r + (1.0 - _max_height);

			float depth = tex2D(depthTex, mhuv).r;
			float4 r = m.r > 0.5 ? float4(depth, 0.0, 0.0, depth) : float4(0.0, 0.0, 0.0, 0.0);

			float4 c = tex2D(prevTex, uv);
			float4 sum = float4(0.0, 0.0, 0.0, 0.0);
			float blur = 0.5 / (float)resolution;

			float4 res = float4(0.0, 0.0, 0.0, 0.0);

			//Acceleration
			float dx1 = 0.0;
			float dx2 = 0.0;
			float dy1 = 0.0;
			float dy2 = 0.0;
			float weight_sum = 0.0;
			for (int k = 1; k <= KERNEL_RAD; k++)
			{
				float weight = (float)exp(-(((k - 1)*(k - 1)) / (2 * sigma * sigma))) / (2 * PI * sigma * sigma);
				dx1 += (tex2D(prevTex, float2(uv.x + k * blur, uv.y)).r - c.r) * weight;
				dx2 += (tex2D(prevTex, float2(uv.x - k * blur < 0.5 + EP ? 0.5 + EP : uv.x - k * blur, uv.y)).r - c.r) * weight;
				dy1 += (tex2D(prevTex, float2(uv.x, uv.y + k * blur > 0.5 - EP ? 0.5 - EP : uv.y + k * blur)).r - c.r) * weight;
				dy2 += (tex2D(prevTex, float2(uv.x, uv.y - k * blur)).r - c.r) * weight;
				weight_sum += weight;
			}

			dx1 = sign(dx1) * abs(dx1) / weight_sum;
			dx2 = sign(dx2) * abs(dx2) / weight_sum;
			dy1 = sign(dy1) * abs(dy1) / weight_sum;
			dy2 = sign(dy2) * abs(dy2) / weight_sum;

			float ap = (c.g - 0.5) * 2.0;
			float vp = (c.b - 0.5) * 2.0;

			float dx = 0.5 * -1.0 * (c.r - (dx1 + dx2) / 2.0) * 0.5;
			float dy = 0.5 * -1.0 * (c.r - (dy1 + dy2) / 2.0) * 0.5;
			float s = 0.25 * (dx1 + dx2 + dy1 + dy2);
			float e = h - c.r;
			float d = -vp;

			float a = stiffness * s + elasticity * e + damping * d;

			//Velocity
			float v = (a - ap) * 0.5 + ap;

			//Position
			float p = c.r + (a - ap) * 0.5 * 0.333333 + ap;

			res.r = max(max(r.r, p), h);
			res.a = max(max(r.a, p), h);

			res.g = (p > r.r && m.a > 0.5) ? (a + 1.0) * 0.5 : 0.5;
			res.b = (p > r.r && m.a > 0.5) ? (v + 1.0) * 0.5 : 0.5;

			return res;
		}
			
	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
	Pass
	{
		Name "Update1"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _HeightTex;

		float _sigma;
		int _resolution1;
		float _stiffness;
		float _elasticity;
		float _damping;

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
		{
			float2 loc_uv = i.localTexcoord;
			float2 g_uv = i.globalTexcoord;
		
			return update_physics1(g_uv, loc_uv, _MainTex, _HeightTex, _SelfTexture2D, _MaskTex, _stiffness, _elasticity, _damping, _resolution1, _sigma);
		
		}

		ENDCG
	}

	Pass
	{
		Name "Update2"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _HeightTex;

		float _sigma;
		int _resolution2;
		float _stiffness;
		float _elasticity;
		float _damping;

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
		{
			float2 loc_uv = i.localTexcoord;
			float2 g_uv = i.globalTexcoord;

			return update_physics2(g_uv, loc_uv, _MainTex, _HeightTex, _SelfTexture2D, _MaskTex, _stiffness, _elasticity, _damping, _resolution2, _sigma);

		}

		ENDCG
	}

	Pass
	{
		Name "Update3"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _HeightTex;

		float _sigma;
		int _resolution3;
		float _stiffness;
		float _elasticity;
		float _damping;

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
		{
			float2 loc_uv = i.localTexcoord;
			float2 g_uv = i.globalTexcoord;

			return update_physics3(g_uv, loc_uv, _MainTex, _HeightTex, _SelfTexture2D, _MaskTex, _stiffness, _elasticity, _damping, _resolution3, _sigma);

		}

		ENDCG
	}

	Pass
	{
		Name "Update4"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _HeightTex;

		float _sigma;
		int _resolution4;
		float _stiffness;
		float _elasticity;
		float _damping;

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
		{
			float2 loc_uv = i.localTexcoord;
			float2 g_uv = i.globalTexcoord;

			return update_physics4(g_uv, loc_uv, _MainTex, _HeightTex, _SelfTexture2D, _MaskTex, _stiffness, _elasticity, _damping, _resolution4, _sigma);

		}

		ENDCG
	}
/*
	Pass
	{
		Name "Combine"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _Tex1;
		sampler2D _Tex2;
		sampler2D _Tex3;
		sampler2D _Tex4;

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
		{
			float2 uv = i.globalTexcoord;

			fixed4 t1 = tex2D(_Tex1, uv);
			fixed4 t2 = tex2D(_Tex2, uv);
			fixed4 t3 = tex2D(_Tex3, uv);
			fixed4 t4 = tex2D(_Tex4, uv);
			float4 ret = max(t1, t2);
			ret = max(ret, t3);
			ret = max(ret, t4);
			
			return ret;
		}

		ENDCG
	}

	Pass
	{
			Name "Blur"

			CGPROGRAM

	#include "UnityCustomRenderTexture.cginc"

			sampler2D _MainTex;
			sampler2D _Tex5;
			sampler2D _MaskTex;

			float _sigma;
			int _resolution4;

	#define KERNEL_RAD 10
	#define PI 3.14159265

	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment frag

			float4 frag(v2f_customrendertexture i) : SV_Target
			{
				float2 uv = i.globalTexcoord;

				fixed4 m = tex2D(_MaskTex, uv);

				fixed4 n = m.a > 0.5 ? tex2D(_MainTex, uv) : fixed4(0.0, 0.0, 0.0, 0.0);
				fixed4 r = m.a > 0.5 ? tex2D(_Tex5, uv) : fixed4(0.0, 0.0, 0.0, 0.0);

				float4 sum = float4(0.0, 0.0, 0.0, 0.0);
				float2 tc = uv;
				float blur = 1.0 / (float)_resolution4;

				float sigma = _sigma * 2;

				//blur radius in pixels
				float weight_sum = 0.0;
				for (int yy = -KERNEL_RAD; yy <= KERNEL_RAD; yy++)
				{
					for (int xx = -KERNEL_RAD; xx <= KERNEL_RAD; xx++)
					{
						float weight = (float)exp(-((xx * xx + yy * yy) / (2 * sigma * sigma))) / (2 * PI * sigma * sigma);
						sum += tex2D(_Tex5, float2(tc.x + xx * blur, tc.y + yy * blur)) * weight * 1.0;
						weight_sum += weight;
					}
				}
				sum = (sum / weight_sum * 1.0);

				return max(n, sum);
			}

			ENDCG
	}
*/
	}
}