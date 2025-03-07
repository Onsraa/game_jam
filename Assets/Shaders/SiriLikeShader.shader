Shader "Custom/SiriLikeEffect"
{
    Properties
    {
        _MainColor("Main Color", Color) = (0.0, 0.6, 1.0, 1.0)
        _SecondColor("Second Color", Color) = (0.9, 0.2, 0.5, 1.0)
        _ThirdColor("Third Color", Color) = (0.2, 0.9, 0.7, 1.0)
        [Space(10)]
        _ColorBlend("Color Blend", Range(0, 1)) = 0.5
        _ColorResponseIntensity("Color Response Intensity", Range(0.1, 5)) = 2.0
        [Space(10)]
        _Glossiness("Smoothness", Range(0, 1)) = 0.9
        _NoiseScale("Noise Scale", Range(1, 50)) = 3.3
        _NoiseIntensity("Noise Intensity", Range(0, 0.1)) = 0.02
        [Space(10)]
        _WaveSpeed("Wave Speed", Range(0.1, 5)) = 2.71
        _WaveIntensity("Wave Intensity", Range(0, 0.2)) = 0.02
        _AudioWaveIntensity("Audio Wave Intensity", Range(0, 1)) = 0.5
        _WaveSharpness("Wave Sharpness", Range(0.1, 5)) = 1.0
        _WaveFrequency("Wave Frequency", Range(0.5, 5)) = 1.0
        [Space(10)]
        _FresnelPower("Fresnel Power", Range(1, 10)) = 10
        _Brightness("Brightness", Range(0.5, 3)) = 1.22
        _AudioBrightnessBoost("Audio Brightness Boost", Range(0, 1)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _MainColor;
                half4 _SecondColor;
                half4 _ThirdColor;
                float _ColorBlend;
                float _ColorResponseIntensity;
                float _Glossiness;
                float _NoiseScale;
                float _NoiseIntensity;
                float _WaveSpeed;
                float _WaveIntensity;
                float _AudioWaveIntensity;
                float _WaveSharpness;
                float _WaveFrequency;
                float _FresnelPower;
                float _Brightness;
                float _AudioBrightnessBoost;
                
                // Ces valeurs sont mises à jour par le script
                float _BassLevel;
                float _MidLevel;
                float _HighLevel;
                float _VolumeLevel;
            CBUFFER_END
            
            // Simple noise functions
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Perfectly round sphere with slight wave distortion
                float3 pos = input.positionOS.xyz;
                float time = _Time.y * _WaveSpeed;
                
                // Very subtle noise-based distortion
                float2 noiseCoord = pos.xy * _NoiseScale + time;
                float noiseValue = noise(noiseCoord) * 2.0 - 1.0;
                noiseValue *= _NoiseIntensity;
                
                // Wave effect - plus réactif au son
                float waveVal = sin(pos.y * 8.0 * _WaveFrequency + time) * 
                               cos(pos.x * 6.0 * _WaveFrequency + time * 0.7) * 
                               sin(pos.z * 7.0 * _WaveFrequency + time * 0.8);
                
                // Application de la sharpness pour contrôler l'aspect pointu/doux
                float sign = waveVal >= 0 ? 1.0 : -1.0;
                waveVal = sign * pow(abs(waveVal), 1.0 / _WaveSharpness);
                
                // Amplifier les vagues avec le niveau de volume + moyenne des fréquences
                float audioModulation = (_BassLevel * 0.6 + _MidLevel * 0.3 + _HighLevel * 0.1) * _AudioWaveIntensity;
                waveVal *= _WaveIntensity * (1.0 + audioModulation * 5.0);
                
                // Apply the distortion along normal direction
                float3 newPos = pos + input.normalOS * (noiseValue + waveVal);
                
                // Transform to clip space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(newPos);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                // Pass UV and normal
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag (Varyings input) : SV_Target
            {
                // Normalized view direction and normal
                float3 viewDir = normalize(input.viewDirWS);
                float3 normal = normalize(input.normalWS);
                
                // Fresnel effect to make the edges glow
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);
                
                // Time-based wave effect for color blending
                float time = _Time.y * _WaveSpeed * 0.5;
                float waveEffect = sin(input.positionWS.x * 3.0 + time) * 
                                  cos(input.positionWS.y * 2.5 + time * 0.8) * 
                                  sin(input.positionWS.z * 2.8 + time * 0.6);
                waveEffect = waveEffect * 0.5 + 0.5; // Map to 0-1
                
                // Audio-enhanced color blending
                float audioColorModulation = _BassLevel * 0.7 + _MidLevel * 0.3;
                audioColorModulation *= _ColorResponseIntensity; // Intensifier l'effet
                
                float blend1 = _ColorBlend + audioColorModulation * 0.5; // Modulation du blend par le son
                blend1 = saturate(blend1); // Limiter entre 0 et 1
                
                float blend2 = waveEffect + _HighLevel * 0.3; // Ajouter un peu de hautes fréquences
                blend2 = saturate(blend2);
                
                // Color blending using all three colors
                half4 color1 = lerp(_MainColor, _SecondColor, blend1);
                half4 color2 = lerp(color1, _ThirdColor, blend2);
                
                // Apply fresnel to the color
                half4 finalColor = color2;
                
                // Boost brightness based on audio
                float audioBrightness = (_BassLevel * 0.3 + _MidLevel * 0.3 + _HighLevel * 0.4) * _AudioBrightnessBoost;
                finalColor.rgb += fresnel * (_Brightness + audioBrightness);
                
    			float centerMask = 1.0 - pow(1.0 - fresnel, 2.0);
    			finalColor.a *= lerp(0.3, 1.0, centerMask); // Plus transparent au centre (0.3), opaque sur les bords (1.0)
    
                
                // Add subtle noise texture
                float2 noiseCoord = input.positionWS.xy * _NoiseScale + time;
                float noiseVal = noise(noiseCoord) * noise(noiseCoord * 2.0) * 0.1;
                finalColor.rgb += noiseVal;
                
                // Apply fog
                finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}