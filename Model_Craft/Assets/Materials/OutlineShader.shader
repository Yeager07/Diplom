Shader "Custom/OutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
        _BaseMap ("Base Map", 2D) = "white" {} // Для URP
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // Первый проход: рисуем обводку
        Pass
        {
            Cull Front
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Расширяем вершины по нормалям
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * _OutlineWidth;
                float3 pos = v.vertex + outlineOffset;
                
                o.pos = UnityObjectToClipPos(float4(pos, 1.0));
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
        
        // Второй проход: рисуем основной объект
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _BaseMap;
            float4 _MainTex_ST;
            float4 _BaseMap_ST;
            float4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // Если MainTex не найден, пробуем BaseMap
                if (col.a == 0) 
                {
                    col = tex2D(_BaseMap, i.uv) * _Color;
                }
                return col;
            }
            ENDCG
        }
    }
}
