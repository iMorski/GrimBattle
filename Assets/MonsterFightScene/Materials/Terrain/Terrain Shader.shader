Shader "Custom/TerrainStatic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Time ("Time", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.vertex = float4(sign(o.vertex.x), sign(o.vertex.y), 0.6, 1.0); 
                return o;
            }

            float2 scaleTexCoords(float2 oldCoords, float2 scaleFactor) {
            	float2 newCoords = oldCoords * scaleFactor;
            	return newCoords;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                // float b = _Time * 10.0;
                // float2 newUv = scaleTexCoords(i.uv, float2(20.0, 20.0)) - float2(0.0, b);
                fixed4 col = tex2D(_MainTex, i.uv);
                
                return col;
            }
            ENDCG
        }
    }
}
