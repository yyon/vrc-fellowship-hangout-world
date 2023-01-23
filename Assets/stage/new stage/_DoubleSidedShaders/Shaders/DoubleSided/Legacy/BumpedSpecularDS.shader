﻿// © 2015 Mario Lelas
Shader "DoubleSided/Legacy Shaders/Bumped SpecularDS"
 {
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
	}

	CGINCLUDE
	sampler2D _MainTex;
	sampler2D _BumpMap;
	fixed4 _Color;
	half _Shininess;

	

	struct Input
	 {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float face : VFACE;
	};


	void surf (Input IN, inout SurfaceOutput o) 
	{
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = tex.rgb * _Color.rgb;
		o.Gloss = tex.a;
		o.Alpha = tex.a * _Color.a;
		o.Specular = _Shininess;
		float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		o.Normal = normal * sign(IN.face);
	}



	ENDCG

	SubShader 
	{ 
		Tags { "RenderType"="Opaque" }
		Cull Off
		LOD 400
	
	


		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0
		ENDCG


	}

	SubShader 
	{ 
		Tags { "RenderType"="Opaque" }
		Cull Off
		LOD 400
	
		CGPROGRAM
		#pragma surface surf BlinnPhong nodynlightmap
		#pragma target 3.0
		ENDCG
	}


	FallBack "DoubleSided/Legacy Shaders/VertexLitCullOff"
}
