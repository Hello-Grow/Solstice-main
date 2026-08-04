Shader "Custom/GodRaysOcclusion"
{
    Properties
    {
        _SunRadius ("Sun Radius", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Occlusion"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #define MAX_GODRAY_LIGHTS 8

            int _LightCount;
            float _LightRadiuses[MAX_GODRAY_LIGHTS];
            float2 _LightScreenPoses[MAX_GODRAY_LIGHTS];

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float rawDepth = SampleSceneDepth(uv);

                #if UNITY_REVERSED_Z
                    bool isBackground = rawDepth <= 0.0001;
                #else
                    bool isBackground = rawDepth >= 0.9999;
                #endif

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float sunMask = 0.0;

                if (isBackground)
                {
                    for (int i = 0; i < _LightCount; i++)
                    {
                        float2 diff = uv - _LightScreenPoses[i];
                        diff.x *= aspect;
                        float dist = length(diff);

                        float mask = 1.0 - smoothstep(_LightRadiuses[i] * 0.7, _LightRadiuses[i], dist);
                        sunMask = max(sunMask, mask);
                    }
                }

                return float4(sunMask.xxx, 1);
            }
            ENDHLSL
        }
    }
}