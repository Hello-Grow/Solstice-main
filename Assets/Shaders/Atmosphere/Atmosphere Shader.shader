Shader "Custom/AtmosphereShader" {
    Properties { 
        [HideInInspector] _SunsetLerp ("Sunset Lerp", float) = 0.0
        [HideInInspector] _SunPos ("Suns Position", Vector) = (0.0, 0.0, 0.0, 0.0)
    }
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass {
            Name "Occlusion"
            ZTest Always ZWrite Off Cull Off Blend Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            float _SunsetLerp;
            float3 _SunPos;
            float3 GetSkyColor(float3 viewDir,float sunsetLerp){
                float h = max(viewDir.y+lerp(0.4,0.1,sunsetLerp), 0.0);
                
                float3 horizonColor = lerp(float3(0.47, 0.79, 0.98),float3(0.1,0.1,0.43),sunsetLerp);
                float3 midSkyColor  = lerp(float3(0.29, 0.5, 0.89),float3(1.00,0.1,0.05),sunsetLerp);
                float3 zenithColor  =lerp(float3(0.06, 0.1, 0.78),float3( 0.94,0.43,0.57),sunsetLerp);
                
                float3 sky = lerp(horizonColor, midSkyColor, smoothstep(0.0, 0.5, h));
                sky = lerp(sky, zenithColor, smoothstep(0.2, 1.0, h-0.3));
                return sky;
            }
            float4 Frag(Varyings input) : SV_Target {
                float2 sunPos = _SunPos.xy;
                float2 uv = input.texcoord;
                float3 viewDir = float3((uv * 2.0 - 1.0) * _ScreenParams.xy / _ScreenParams.y, 1.0);
                viewDir.x *= -_ProjectionParams.x; 
                float3 rayDirection = normalize(mul((float3x3)unity_CameraToWorld, viewDir));
                float3 sunDir = float3(0.0,0.0,1.0);
                float3 color = GetSkyColor(rayDirection,_SunsetLerp);
                float aspect =  _ScreenParams.x / _ScreenParams.y;
                uv.x*=aspect;   
                uv.x-=0.4;
                float sunCore = 1.0-saturate(distance(sunPos,uv));
                float outerGlow = pow(sunCore,2);
                sunCore = pow(sunCore,20)*5;
                float3 sunColor = sunCore*lerp(float3(1.0, 1.0, 0.95),float3(0.99,0.1,0.1),_SunsetLerp)+outerGlow*lerp(float3(1.0, 0.9, 0.5),float3(0.9,0.35,0.16),_SunsetLerp);
                color+=sunColor;
                return float4(color*1.5, 1.0);
            }
            ENDHLSL
        }
    }
}
