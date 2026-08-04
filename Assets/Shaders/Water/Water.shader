Shader "Custom/Water"
{
    Properties
    {
        [HideInInspector]_LightDir ("Main Light Direction",Vector) = (0.5,1.0,0.5)
        [HideInInspector] _SunsetLerp ("Sunset Lerp", float) = 0.0
        _BaseSeaColor ("Deep Sea Color", Color) = (0.0,0.09,0.18,1.0)
        _SeaWaterColor ("Sea Water Color", Color) = (0.48,0.54,0.36,1.0)
        _Glossiness ("Smoothness", Range(0,1)) = 1.0
        _WaterSpeed ("Wave Speed", Range(0,10)) = 3.0
        [NoScaleOffset] _CubeMap ("Reflection Cubemap", Cube) = "" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                   
                float3 objectPos : TEXCOORD0;
                float mapHeight : TEXCOORD1;
                float4 positionHCS : SV_POSITION;
            };
            float3 _LightDir;
            float4 _BaseSeaColor;
            float4 _SeaWaterColor;
            float _Glossiness;
            float _WaterSpeed;
            float _SunsetLerp;
            TEXTURECUBE(_CubeMap);
            SAMPLER(sampler_CubeMap);
            float hash( float2 p ) {
                float h = dot(p,float2(127.1,311.7));	
                return frac(sin(h)*43758.5453123);
            }
            float noise( in float2 p ) {
                float2 i = floor( p );
                float2 f = frac( p );	
                float2 u = f*f*(3.0-2.0*f);
                return -1.0+2.0*lerp( 
                            lerp( hash( i + float2(0.0,0.0) ), 
                                 hash( i + float2(1.0,0.0) ), 
                                    u.x),
                            lerp( hash( i + float2(0.0,1.0) ), 
                                 hash( i + float2(1.0,1.0) ), 
                                    u.x), 
                            u.y);
            }
            float sea_octave(float2 uv, float choppy) {
                uv += noise(uv);
                float2 wv = 1.0-abs(sin(uv)); 
                float2 swv = abs(cos(uv));  
                wv = lerp(wv,swv,wv);
                return pow(1.0-pow(wv.x * wv.y,0.65),choppy);
            }
            float map(float3 p) {
                float freq = 0.16;
                float amp = 0.6;
                float choppy = 4.0;
                float2 uv = p.xz;
                uv.x *= 0.75;
    
                float d, h = 0.0;    
                for(int i = 0; i < 5; i++) {
    	            d = sea_octave((uv+_Time.y*_WaterSpeed)*freq,choppy);
    	            d += sea_octave((uv-_Time.y*_WaterSpeed)*freq,choppy);

                    h += d * amp; 
     
    	            uv = mul(uv,float2x2(1.6,1.2,-1.2,1.6));
        
                    freq *= 1.9;
                    amp *= 0.22;
                    choppy = lerp(choppy,1.0,0.2);
                }
                return h;
            }
            float diffuse(float3 n,float3 l,float p) {
                return pow(dot(n,l) * 0.4 + 0.6,p);
            }
            float specular(float3 n,float3 l,float3 e,float s) {    
                float nrm = (s + 8.0) / (3.1415 * 8.0);
                return pow(max(dot(reflect(e,n),l),0.0),s) * nrm;
            }
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 position = IN.positionOS.xyz;
                float height =  map(position);
                position.y +=height;
                OUT.positionHCS = TransformObjectToHClip(position);
                OUT.objectPos =IN.positionOS.xyz;
                OUT.mapHeight = height;
                return OUT;
            }
            float3 getNormal(float3 p, float eps) {
                float3 n;
                n.y = map(p);
                n.x = map(float3(p.x+eps,p.y,p.z)) - n.y;
                n.z = map(float3(p.x,p.y,p.z+eps)) - n.y;
                n.y = eps; 
                return normalize(n);
            }
            float3 SampleReflections(float3 normalWS, float3 viewDirWS)
            {
                float3 reflectVector = reflect(viewDirWS, normalWS);
                float mip = (1.0 - _Glossiness) * 6.0;
                float3 reflection = SAMPLE_TEXTURECUBE_LOD(_CubeMap, sampler_CubeMap, reflectVector, mip);
                return reflection;
            }
            float3 GetSkyColor(float3 normalWS, float3 viewDirWS,float sunsetLerp){
                float3 reflectVector = reflect(viewDirWS, normalWS);    
                float h = max(reflectVector.y+0.4, 0.0);                
                float3 horizonColor = float3(0.47, 0.79, 0.98);
                float3 midSkyColor  = float3(0.29, 0.5, 0.89);
                float3 zenithColor  = float3(0.06, 0.1, 0.78);
                
                float3 sky = lerp(horizonColor, midSkyColor, smoothstep(0.0, 0.5, h));
                sky = lerp(sky, zenithColor, smoothstep(0.2, 1.0, h-0.3));
                return lerp(sky,float3(1.00,0.1,0.05),sunsetLerp);
            }
            float4 frag(Varyings IN) : SV_Target
            {
                Light mainLight = GetMainLight(); 
                float3 lightDir = normalize(_LightDir);
                float3 position = IN.objectPos;
                float3 worldPos = TransformObjectToWorld(position);
                float3 cameraDir = normalize(worldPos - _WorldSpaceCameraPos);
                float3 normal = getNormal(position,0.1);
                float3 worldNormal = TransformObjectToWorldNormal(normal);
                float dist = distance(_WorldSpaceCameraPos, worldPos);
                float fresnel = 1.0 - max(dot(worldNormal,-cameraDir),0.0);
                fresnel = pow(fresnel,3.0)*1.5;
                float sunsetLerp= _SunsetLerp;
                float3 refracted = diffuse(worldNormal,lightDir,1.0) * _SeaWaterColor.rgb;
                float3 reflected = GetSkyColor(worldNormal, cameraDir,sunsetLerp)*1.5;
                float3 color = lerp(refracted,reflected,fresnel);
                float3 specularColor =specular(worldNormal,lightDir,cameraDir,256)*float3( 1.00, 0.75, 0.4)/3;
                color+=specularColor;
                float3 fogColor = float3(0.1,0.1,0.43);
                color = lerp(fogColor,color,max(1.0-sunsetLerp,exp(-pow(dist/150,2.0))));
                return float4(color ,1.0);
            }
            ENDHLSL
        }
    }
}
