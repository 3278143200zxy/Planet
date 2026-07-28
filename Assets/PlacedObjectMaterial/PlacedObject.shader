Shader "Custom/SpriteInnerWhiteOutline_DualFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _OutlineThickness ("Outline Thickness", Range(0.001, 0.05)) = 0.01

        _FillAmount_Original ("Fill Amount Original", Range(0,1)) = 1.0
        _FillAmount_White ("Fill Amount White", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex    : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                fixed4 color     : COLOR;
                float2 contentY  : TEXCOORD1; // x=minY, y=maxY
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _OutlineThickness;
            float _FillAmount_Original;
            float _FillAmount_White;

            // 粗采样找不透明区域的上下边界（无脚本）
            float2 ComputeContentBoundsY()
            {
                const int SCAN = 64; // 越大越准，越慢；一般 64 够用
                float minY = 1.0;
                float maxY = 0.0;

                [loop]
                for (int iy = 0; iy < SCAN; iy++)
                {
                    float y = (iy + 0.5) / SCAN;
                    [loop]
                    for (int ix = 0; ix < SCAN; ix++)
                    {
                        float x = (ix + 0.5) / SCAN;
                        float a = tex2Dlod(_MainTex, float4(x, y, 0, 0)).a;
                        if (a > 0.1)
                        {
                            minY = min(minY, y);
                            maxY = max(maxY, y);
                        }
                    }
                }

                if (maxY < minY)
                    return float2(0.0, 1.0);

                float halfStep = 0.5 / SCAN;
                return float2(max(minY - halfStep, 0.0), min(maxY + halfStep, 1.0));
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                OUT.contentY = ComputeContentBoundsY();
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 SampleSafe(float2 uv)
            {
                if (uv.x < 0.0f || uv.x > 1.0f || uv.y < 0.0f || uv.y > 1.0f)
                    return fixed4(0,0,0,0);
                return tex2D(_MainTex, uv);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                fixed4 centerCol = SampleSafe(uv);
                float isOpaque = (centerCol.a > 0.1) ? 1.0 : 0.0;

                float hasTransparentNeighbor = 0.0;
                float hasMissingNeighbor = 0.0;

                float2 offsets[8] = {
                    float2( _OutlineThickness, 0),
                    float2(-_OutlineThickness, 0),
                    float2(0,  _OutlineThickness),
                    float2(0, -_OutlineThickness),
                    float2( _OutlineThickness,  _OutlineThickness),
                    float2(-_OutlineThickness,  _OutlineThickness),
                    float2( _OutlineThickness, -_OutlineThickness),
                    float2(-_OutlineThickness, -_OutlineThickness)
                };

                for (int i = 0; i < 8; i++)
                {
                    float2 sampleUV = uv + offsets[i];
                    if (sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0)
                        hasMissingNeighbor = 1.0;
                    else if (SampleSafe(sampleUV).a <= 0.1f)
                        hasTransparentNeighbor = 1.0;
                }

                float needOutline = max(hasTransparentNeighbor, hasMissingNeighbor);

                if (isOpaque > 0.5 && needOutline > 0.5)
                    return fixed4(1.0, 1.0, 1.0, _Color.a);

                float contentMinY = IN.contentY.x;
                float contentMaxY = IN.contentY.y;
                float contentH = max(contentMaxY - contentMinY, 1e-5);
                float fillY = saturate((uv.y - contentMinY) / contentH);

                if (centerCol.a > 0.01)
                {
                    if (fillY <= _FillAmount_Original)
                        return centerCol * _Color;
                    else if (fillY <= _FillAmount_White)
                        return fixed4(1, 1, 1, _Color.a);
                }

                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}