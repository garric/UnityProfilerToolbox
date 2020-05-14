Shader "PixelOverdraw/Apply"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WhiteNumber ("White Number", Float) = 0 
        _FlipY("White Number", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }

        ZTest Always Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One Zero

        Pass
        {
            CGPROGRAM

            #pragma vertex VertUV
            #pragma fragment frag

            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WhiteNumber;
            float _FlipY;

            // note: no SV_POSITION in this struct
            struct v2f {
                float2 uv : TEXCOORD0;
                //float2 ndc : TEXCOORD1;
            };

            v2f VertUV(
                float4 vertex : POSITION, /*vertex position input*/
                float2 uv : TEXCOORD0, /*texture coordinate input*/
                out float4 outpos : SV_POSITION /*clip space position output*/
            ){
                v2f o;
                o.uv = uv;
                outpos = float4(vertex.xy, 0.0, 1.0);

                // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html

                if (_ProjectionParams.x < 0) // upside-down flipped projection
                    outpos.y = -1 * outpos.y;

                float flipY = 0;
            #if UNITY_UV_STARTS_AT_TOP
                flipY = 1;
            #endif
                if (flipY > 0 || _FlipY > 0)
                    o.uv.y = 1 - o.uv.y;
                //o.ndc = vertex.xy * 0.5 + 0.5;

                return o;
            }

            fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                //// screenPos.xy will contain pixel integer coordinates.
                //// use them to implement a checkerboard pattern that skips rendering
                //// 4x4 blocks of pixels

                ///* checker value will be negative for 4x4 blocks of pixels in a checkerboard pattern */
                //screenPos.xy = floor(screenPos.xy * 0.25) * 0.5;
                //float checker = -frac(screenPos.r + screenPos.g);

                ///* clip HLSL instruction stops rendering a pixel if value is negative */
                ////clip(checker);

                //return float4(i.ndc.x, i.ndc.x, i.ndc.x, 1);

                /*for pixels that were kept, read the texture and output it*/
                fixed4 c = tex2D (_MainTex, i.uv);
                c.rgb = _WhiteNumber * (float3(1, 1, 1) - c.rgb) + (1 - _WhiteNumber) * c.rgb;

                return c;
            }

            ENDCG
        }
    }
}

