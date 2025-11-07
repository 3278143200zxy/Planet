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
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _OutlineThickness;
            float _FillAmount_Original;
            float _FillAmount_White;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
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

                // 描边优先显示
                if (isOpaque > 0.5 && needOutline > 0.5)
                {
                    return fixed4(1.0, 1.0, 1.0, _Color.a);
                }

                // 非描边区域，分层显示原图和白色区间
                if (centerCol.a > 0.01)
                {
                    if (uv.y <= _FillAmount_Original)
                    {
                        // 显示原图
                        return centerCol * _Color;
                    }
                    else if (uv.y >= _FillAmount_Original && uv.y <= _FillAmount_White)
                    {
                        // 超出原图显示范围但在白色区间内，显示白色，透明度同材质
                        return fixed4(1, 1, 1, _Color.a);
                    }
                }

                // 其余透明
                return fixed4(0, 0, 0, 0);
            }

            ENDCG
        }
    }
}
