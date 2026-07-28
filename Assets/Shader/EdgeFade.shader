Shader "EdgeFade"
{
    Properties
    {
        [MainTexture] _BaseMap("Base (RGB)", 2D) = "white" {}
        [MainColor] _BaseColor("Tint Color", Color) = (1, 1, 1, 1)
        
        // Controls how far from the edge the gradient starts (0.0 to 0.5)
        _FadeDistance("Fade Distance", Range(0.0, 0.5)) = 0.15
        // Controls the smoothness/falloff of the gradient
        _FadePower("Fade Smoothness", Range(0.1, 5.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        // 开启透明混合
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            Texture2D _BaseMap;
            SamplerState sampler_BaseMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _FadeDistance;
                float _FadePower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. 采样原图颜色与 Alpha 
                half4 texColor = _BaseMap.Sample(sampler_BaseMap, input.uv) * _BaseColor;
                
                // 2. 计算到边缘的距离
                float2 distToEdge = min(input.uv, 1.0 - input.uv);
                float minDistance = min(distToEdge.x, distToEdge.y);

                // 3. 映射到 0-1 的渐变系数 (1 代表中心完全不透明，0 代表最边缘完全透明)
                float edgeFactor = saturate(minDistance / max(_FadeDistance, 0.0001));
                edgeFactor = pow(edgeFactor, _FadePower);

                // 4. 【核心修改】：保持 RGB 颜色完全不变，只改变 Alpha 通道
                half4 finalColor = texColor;
                finalColor.a *= edgeFactor; // 将原图透明度乘上渐变系数

                return finalColor;
            }
            ENDHLSL
        }
    }
}