Shader "PixelOverdraw/ClearOpaque"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [HideInInspector] _ColorSrcBlend("_ColorSrcBlend", float) = 0 /*BlendMode.Zero*/
        [HideInInspector] _ColorDstBlend("_ColorDstBlend", float) = 1 /*BlendMode.One*/        
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }

        LOD 100
        Blend Zero One, One Zero
        Blend [_ColorSrcBlend][_ColorDstBlend], One Zero
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vertFullTriangle
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata {
                float4 vertex : POSITION;
            };

            v2f vertFullTriangle(appdata v) 
            {
                v2f o;
                o.pos = float4(v.vertex.xy, 0.0, 1.0);
                o.uv = (v.vertex.xy + 1.0) * 0.5;

                // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
            #if UNITY_UV_STARTS_AT_TOP    
                if (_ProjectionParams.x < 0)
                    o.uv.y = 1.0 - o.uv.y;
            #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alpha = 0.0/255;
                return float4(0, 0, 0, alpha);

            }
            ENDCG
        }
    }
}

