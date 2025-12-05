Shader "Custom/PlayerFogRadial"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _FogRadius ("Fog Radius", Float) = 5
        _FogColor ("Fog Color", Color) = (0,0,0,1)
        _GrayscaleAmount ("Grayscale Amount", Range(0, 1)) = 0
        _FogStartDistance ("Fog Start Distance", Float) = 8
        _MaxFogDensity ("Max Fog Density", Range(0, 1)) = 0.85
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "PlayerFogPass"
            ZTest Always
            Cull Off
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            
            float3 _PlayerPos;
            float _FogRadius;
            float4 _FogColor;
            float _GrayscaleAmount;
            float _FogStartDistance;
            float _MaxFogDensity;
            
            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewRay : TEXCOORD1;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Generate fullscreen triangle
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                output.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                output.uv = uv;
                
                // Calculate view ray for this vertex
                float3 viewRay = mul(UNITY_MATRIX_I_P, float4(output.positionCS.xy, 1.0, 1.0)).xyz;
                output.viewRay = mul(UNITY_MATRIX_I_V, float4(viewRay, 0.0)).xyz;
                
                #if UNITY_UV_STARTS_AT_TOP
                    output.uv.y = 1.0 - output.uv.y;
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample the source texture (the scene we've already rendered)
                half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Sample depth
                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv).r;
                
                // Reconstruct world position from depth
                float3 worldPos = _WorldSpaceCameraPos + normalize(input.viewRay) * LinearEyeDepth(depth, _ZBufferParams);
                
                // Calculate horizontal distance from player (xz plane only)
                float dist = distance(worldPos.xz, _PlayerPos.xz);
                
                float fogAmount = 1.0; // Start with no fog (fully clear)
                
                // Only apply fog if beyond the start distance
                if (dist > _FogStartDistance)
                {
                    // Calculate distance into the fog zone
                    float fogDist = dist - _FogStartDistance;
                    
                    // 20-zone fog system for ultra-smooth gradual fog
                    float zoneSize = _FogRadius / 20.0;
                    
                    // Determine which zone we're in
                    float zoneIndex = fogDist / zoneSize;
                    float zoneFloor = floor(zoneIndex);
                    float zoneFraction = zoneIndex - zoneFloor; // 0-1 within current zone
                    
                    // Smooth the transition within each zone
                    zoneFraction = smoothstep(0.0, 1.0, zoneFraction);
                    
                    // Calculate fog amount based on zone
                    // Zone 0 = 100% clear (1.0), Zone 20 = MaxFogDensity
                    float startClearness = 1.0 - (zoneFloor / 20.0) * _MaxFogDensity;
                    float endClearness = 1.0 - ((zoneFloor + 1.0) / 20.0) * _MaxFogDensity;
                    
                    // Clamp to valid range
                    startClearness = saturate(startClearness);
                    endClearness = saturate(endClearness);
                    
                    // Interpolate between the start and end of this zone
                    fogAmount = lerp(startClearness, endClearness, zoneFraction);
                    
                    // Optional: extend fog beyond _FogRadius for complete coverage
                    if (fogDist > _FogRadius)
                    {
                        float extraDist = (fogDist - _FogRadius) / (_FogRadius * 0.2);
                        extraDist = saturate(extraDist);
                        extraDist = smoothstep(0.0, 1.0, extraDist);
                        fogAmount = lerp(fogAmount, 1.0 - _MaxFogDensity, extraDist);
                    }
                }
                
                // Blend: fogAmount = 1.0 means scene visible, lower values mean more fog
                half3 finalColor = lerp(_FogColor.rgb, sceneColor.rgb, fogAmount);
                
                // Apply grayscale effect
                float luminance = dot(finalColor, float3(0.299, 0.587, 0.114));
                finalColor = lerp(finalColor, luminance.xxx, _GrayscaleAmount);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}