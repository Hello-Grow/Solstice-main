Shader "Custom/GodRaysComposite"
{
    Properties{
        [HideInInspector] _BlitTexture("Blit Texture", 2D) = "white" {}
        [HideInInspector] _GodRaysTex("God Rays Texture", 2D) = "black" {}
    }
    SubShader{
        Tags{ "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        LOD 100
        ZWrite Off
        Cull Off
        ZTest Always

        Pass{
            Name "Composite"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #define MAX_GODRAY_LIGHTS 8

            TEXTURE2D_X(_GodRaysTex);
            SAMPLER(sampler_GodRaysTex);

            int _LightCount;
            float _RayIntensity;
            float _LightRadiuses[MAX_GODRAY_LIGHTS];
            float2 _LightScreenPoses[MAX_GODRAY_LIGHTS];

            float4 Frag(Varyings input) : SV_Target
            {
                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                float4 godRays = SAMPLE_TEXTURE2D_X(_GodRaysTex, sampler_LinearClamp, input.texcoord);

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float falloff = 0.0;

                for (int i = 0; i < _LightCount; i++)
                {
                    float2 diff = input.texcoord - _LightScreenPoses[i];
                    diff.x *= aspect;
                    float dist = length(diff);
                    float mask = 1.0 - smoothstep(_LightRadiuses[i] * 0.7, _LightRadiuses[i] * 6.0, dist);
                    falloff = max(falloff, mask);
                }

                float3 rayColor = godRays.rgb * falloff;
                float3 finalColor = sceneColor.rgb + rayColor * _RayIntensity;

                return float4(finalColor, sceneColor.a);
            }
            ENDHLSL
        }
    }
}