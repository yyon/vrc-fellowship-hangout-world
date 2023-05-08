Shader "Custom/ControlCubeShader"
{
    Properties
    {
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _ShiftTex("Emission Shift Texture", 2D) = "white" {}
        _ShiftSpeed ("Shift Speed", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ShiftTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _EmissionColor;
        float _ShiftSpeed;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = fixed4(0, 0, 0, 1);

            fixed4 textureColor = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 shiftTextureColor = tex2D(_ShiftTex, IN.uv_MainTex + float2(_Time.y * _ShiftSpeed + IN.worldPos.x + IN.worldPos.y, _SinTime.w * _ShiftSpeed + IN.worldPos.z));

            o.Albedo = textureColor.r > 0.7 ? fixed4(0.6, 0.6, 0.6, 1) : fixed4(0, 0, 0, 1);
            o.Metallic = textureColor.r > 0.1 ? 1 : 0;
            o.Smoothness = textureColor.r > 0.1 ? 0.5 : 0;
            o.Occlusion = textureColor.r > 0.1 ? 1 : 0;
            o.Emission = textureColor.r < 0.7 && textureColor.r > 0.1 ? _EmissionColor * (shiftTextureColor.r + 0.85) : 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
