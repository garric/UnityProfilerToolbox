Shader "QuadOverdraw/Clear"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {} }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        //LOD 100
        Cull Off
        Blend One Zero
        ZWrite Off
        ZTest Always
        ZClip  false

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0 // support interlocked operation

            #include "UnityCG.cginc"

            // note: no SV_POSITION in this struct
            struct v2f {
                float2 placeholder : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform float4 _RWQuadBuffer_Dimension;

            v2f vert(float4 vertex : POSITION, /*vertex position input*/
                out float4 outpos : SV_POSITION /*clip space position output*/)
            {
                v2f o;
                o.placeholder = float2(0, 0);

                outpos = UnityObjectToClipPos(vertex);
                return o;
            }

            // The temporary buffer used to synchronize and exchange data between quad sub-pixels.
            // Left half hold QuadDescriptor, right half hold QuadComplexity
            // Both are halfres here.
            RWTexture2D<uint> RWQuadBuffer : register(u1);

            uint2 QuadComplexityOffset()
            {
                //uint QuadBufferWidth, QuadBufferHeight;
                //RWQuadBuffer.GetDimensions(QuadBufferWidth, QuadBufferHeight);
                //return uint2(QuadBufferWidth / 2, 0);
                return uint2(_RWQuadBuffer_Dimension.x / 2, 0);
            }

            fixed4 frag(v2f i, UNITY_VPOS_TYPE SvPosition : VPOS) : SV_Target
            {
                uint2 quadId = SvPosition.xy / 2;

                RWQuadBuffer[quadId.xy] = 0;
                RWQuadBuffer[quadId.xy + QuadComplexityOffset()] = 0;

                return float4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}
