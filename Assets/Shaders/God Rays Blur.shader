Shader "Custom/GodRaysBlur"
{
    Properties{
        [HideInInspector] _BlitTexture("Blit Texture", 2D) = "white" {}
    }
    SubShader{
        Tags{ "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        LOD 100
        ZWrite Off
        Cull Off
        ZTest Always

        Pass{
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #define MAX_GODRAY_LIGHTS 8
            int _LightCount;
            float _Intensity;
            float _TotalDistance;
            uint _Samples;
            float _Decay;
            float2 _LightScreenPoses[MAX_GODRAY_LIGHTS];

            float4 Frag(Varyings input):SV_Target{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 dir = float2(0,0);
                for(int i=0;i<_LightCount;i++){
                    dir=(input.texcoord - _LightScreenPoses[i]);
                }
                float illuminationDecay = 1.0;
                float4 finalColor = float4(0,0,0,0);
                float weightSum = 0.0;
                float stepSize = _TotalDistance/_Samples;
                for(uint i = 0; i < _Samples; i++){
                    float2 sampleUV = input.texcoord - dir * stepSize * (float)i;
                    // float blueNoiseOffset = frac(sin(dot(sampleUV, float2(12.9898f, 78.233f))) * 43758.5453f)*0.002;
                    // sampleUV+=blueNoiseOffset;
                    float4 sampleColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, sampleUV);        
                    float weight = illuminationDecay;
                    finalColor += sampleColor * weight;
                    weightSum += weight;

                    illuminationDecay *= _Decay;
                }

                return finalColor*_Intensity;
            }
            ENDHLSL
        }
    }
}