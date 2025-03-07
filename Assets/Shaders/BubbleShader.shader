Shader "Custom/BubbleShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.4, 0.6, 1.0, 0.7)
        _ActiveColor("Active Color", Color) = (1.0, 0.5, 0.5, 0.8)
        _ColorIntensity("Color Intensity", Range(0, 1)) = 0
        _DeformAmount("Deform Amount", Range(0, 1)) = 0.2
        _PulseTime("Pulse Time", Float) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.8
        _Metallic("Metallic", Range(0, 1)) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
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
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ActiveColor;
                float _ColorIntensity;
                float _DeformAmount;
                float _PulseTime;
                float _Smoothness;
                float _Metallic;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                float3 positionOS = input.positionOS.xyz;
                
                // Déformation basée sur le sin du temps et la position
                float deform = sin(_PulseTime * 2 + positionOS.x * 10) * sin(_PulseTime * 1.5 + positionOS.y * 8) * sin(_PulseTime + positionOS.z * 6);
                positionOS += input.normalOS * deform * _DeformAmount;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Mélange des couleurs basé sur l'intensité
                half4 color = lerp(_BaseColor, _ActiveColor, _ColorIntensity);
                
                // Effet Fresnel (brillance sur les bords)
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), 4);
                
                // Ajout de la brillance des bords
                color.rgb += fresnel * color.rgb * 0.5;
                
                // Effet d'ondulation basé sur la position et le temps
                float ripple = sin(input.positionWS.x * 5 + _PulseTime) * sin(input.positionWS.y * 3 + _PulseTime * 0.8) * 0.05;
                color.rgb += ripple;
                
                // Atténuation par le brouillard
                color.rgb = MixFog(color.rgb, input.fogFactor);
                
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}