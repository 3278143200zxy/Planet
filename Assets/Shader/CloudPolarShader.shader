Shader "Custom/CloudPolarShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("Main Texture (Sprite)", 2D) = "white" {}
        _Threshold ("Base Threshold", Range(0.01, 0.99)) = 0.5
        
        // 将原 _NoiseScale 拆分为 X 和 Y 方向的拉伸/缩放
        _NoiseScaleX ("Noise Scale X (Angular)", Float) = 5.0
        _NoiseScaleY ("Noise Scale Y (Radial)", Float) = 5.0
        
        _NoiseOffset ("Noise Offset", Vector) = (0,0,0,0)
        _ClockwiseSpeed ("Clockwise Speed", Float) = 0.5
        _CounterClockwiseSpeed ("Counter Clockwise Speed", Float) = 0.3
        _MinRadius ("Min Radius (Inner)", Float) = 0.5
        _MaxRadius ("Max Radius (Outer)", Float) = 4.5
        _EdgeDistortion ("Edge Distortion", Range(0, 2)) = 0.5
        _CircleProgress ("Circle Progress", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // 声明新参数，移除旧的 _NoiseScale
            float _Threshold, _NoiseScaleX, _NoiseScaleY, _ClockwiseSpeed, _CounterClockwiseSpeed;
            float _MinRadius, _MaxRadius, _EdgeDistortion, _CircleProgress;
            float4 _NoiseOffset;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash(float2 p) {
                return frac(sin(dot(p, float2(12.1, 78.233))) * 43758.5453);
            }

            float ValueNoise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0,0)), hash(i + float2(1,0)), u.x),
                            lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x), u.y);
            }

            // 修改此函数，使其分别接受 X 和 Y 的缩放值
            float GetSeamlessNoise(float2 uv, float offset, float scaleX, float scaleY, float2 startPos) {
                float angle = uv.x * 6.2831853 + offset + startPos.x;
                // scaleX 影响圆周方向的采样半径，值越小，环向越被拉伸
                // scaleY 影响纵向（半径方向）的采样密度，值越小，向外扩散方向越被拉伸
                float2 p1 = float2(cos(angle) * scaleX, uv.y * scaleY + startPos.y);
                float2 p2 = float2(sin(angle) * scaleX, uv.y * scaleY + startPos.y);
                return (ValueNoise(p1) + ValueNoise(p2)) * 0.5;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 dir = i.uv - float2(0.5, 0.5);
                float radius = length(dir) * 2.0; 
                float angle = atan2(dir.y, dir.x);
                if (angle < 0) angle += 6.2831853;
                float angleProgress = angle / 6.2831853;

                if (angleProgress > _CircleProgress || radius > 1.0) return fixed4(0,0,0,0);

                float2 targetUV = float2(angleProgress / _CircleProgress, radius);
                
                float cwOffset = _Time.y * _ClockwiseSpeed;
                float ccwOffset = -_Time.y * _CounterClockwiseSpeed;

                // 对第二层噪声图按比例缩放
                float n1 = GetSeamlessNoise(targetUV, cwOffset, _NoiseScaleX, _NoiseScaleY, _NoiseOffset.xy);
                float n2 = GetSeamlessNoise(targetUV, ccwOffset, _NoiseScaleX * 1.35, _NoiseScaleY * 1.35, _NoiseOffset.zw);
                float combinedNoise = (n1 + n2) * 0.5;

                // 这里的变形基础距离使用 Y 方向的缩放
                float currentDist = targetUV.y * _NoiseScaleY;
                float distortion = (combinedNoise - 0.5) * _EdgeDistortion;
                float distortedDist = currentDist + distortion;

                float rangeMask = step(_MinRadius, distortedDist) * step(distortedDist, _MaxRadius);
                float finalAlpha = step(_Threshold, combinedNoise) * rangeMask;

                return fixed4(1, 1, 1, finalAlpha);
            }
            ENDCG
        }
    }
}