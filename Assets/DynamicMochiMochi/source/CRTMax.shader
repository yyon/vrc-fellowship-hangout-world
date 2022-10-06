Shader "CRTMax"
{
	Properties{
		_Tex1("Tex1", 2D) = "black" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
	Pass
	{
		Name "Combine"

		CGPROGRAM

#include "UnityCustomRenderTexture.cginc"

		sampler2D _Tex1;

#pragma vertex CustomRenderTextureVertexShader
#pragma fragment frag

		float4 frag(v2f_customrendertexture i) : SV_Target
	{
		float2 uv1 = float2(i.globalTexcoord.x * 0.5, i.globalTexcoord.y * 0.5);
		float2 uv2 = float2(i.globalTexcoord.x * 0.5 + 0.5, i.globalTexcoord.y * 0.5);
		float2 uv3 = float2(i.globalTexcoord.x * 0.5, i.globalTexcoord.y * 0.5 + 0.5);
		float2 uv4 = float2(i.globalTexcoord.x * 0.5 + 0.5, i.globalTexcoord.y * 0.5 + 0.5);
		
		fixed4 t1 = tex2D(_Tex1, uv1);
		fixed4 t2 = tex2D(_Tex1, uv2);
		fixed4 t3 = tex2D(_Tex1, uv3);
		fixed4 t4 = tex2D(_Tex1, uv4);
		float4 ret = max(t1, t2);
		ret = max(ret, t3);
		ret = max(ret, t4);

		return ret;
		
	}

		ENDCG
	}

	}
}