Shader "Custom/BuildingEmission_Lit2D"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 5)) = 1
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1
        _EdgeSoftness ("Edge Softness", Range(0.01, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Brightness;
            float _EmissionStrength;
            float _EdgeSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 从 Unity 光源获取主方向光信息
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 normal = normalize(i.worldNormal);

                // 基本光照 (0 ~ 1)
                float ndotl = saturate(dot(normal, lightDir));

                // 模糊边界（通过 smoothstep 形成柔和交界线）
                float softNdotL = smoothstep(0, _EdgeSoftness, ndotl);

                // 环境光（防止暗面太黑）
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * 0.5;

                // 最终亮度
                float3 lighting = (softNdotL * _Brightness + ambient) * _LightColor0.rgb;

                // 发光效果（增加可见度）
                float3 emission = col.rgb * softNdotL * _EmissionStrength;

                col.rgb = col.rgb * lighting + emission;

                return col;
            }
            ENDCG
        }
    }
}
