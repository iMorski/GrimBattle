Shader "Unlit/hpBarShader"
{
    Properties
    {
        _hpPercentage("hpPercentage", float) = 0.6
        _lastHpPercentage("lastHpPercentage", float) = 0.8
        _alpha("alpha", float) = 1.0
        _maxHP("maxHP", float) = 150.0
        _hpBlock("hpBlock", float) = 50.0


        _hpColor ("HP Color", Color) = (0.2, 0.6, 0.3)
        _backgroundColor ("Underlay Color", Color) = (0.1, 0.2, 0.3)
        _damageColor ("Damage Color", Color) = (0.85, 0.75, 0.8)
        _separatorColor ("Separator Color", Color) = (0.1, 0.1, 0.1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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
                o.uv = v.uv;
                
                return o;
            }

            // properties passed from script
            float _hpPercentage;
            float _lastHpPercentage;
            float _alpha;
            float _maxHP;
            float _hpBlock;

            // user-set properties
            float3 _hpColor;
            float3 _backgroundColor;
            float3 _damageColor;
            float3 _separatorColor;

            fixed4 frag (v2f i) : SV_Target
            {
                // draw background
                float4 col = float4(_backgroundColor, _alpha);

                // draw damage on top
                col.rgb = lerp(col.rgb, _damageColor, step(i.uv.x, _lastHpPercentage));

                // draw hp on top
                float modVal = _hpBlock / _maxHP;
                float inputSeparator = ((fmod(i.uv.x, modVal) < 0.014) || (modVal - fmod(i.uv.x, modVal) < 0.014))  ? 1.0 : 0.0;
                float3 hpColWithSeparators = lerp(_hpColor, _separatorColor, inputSeparator);
                col.rgb = lerp(col.rgb, hpColWithSeparators, step(i.uv.x, _hpPercentage));     

                return col;
            }
            ENDCG
        }
    }
}
