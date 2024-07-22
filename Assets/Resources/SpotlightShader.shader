Shader "Custom/SpotlightEffect"
{
    Properties
    {
        _Spotlight ("Spotlight (RGBA)", 2D) = "white" {}
        _Intensity ("Intensity", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _Spotlight;
            float _Intensity;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 spotlight = tex2D(_Spotlight, i.uv);

                return half4(spotlight.rgb * _Intensity, spotlight.a);
            }
            ENDCG
        }
    }
}
