// https://docs.unity3d.com/Manual/SL-ShaderCompileTargets.html

Shader "QuadOverdraw/Accumulate"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2 /* Default Back */
        [Enum(ProfilerToolbox.ZWrite)] _ZWrite("Z Write", float) = 1 /* ZWrite Off */
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", float) = 4 /* Default LessEqual */
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        
        Cull [_Cull]
        ZWrite [_ZWrite]
        ZTest [_ZTest]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0 // support interlocked operation

            #include "UnityCG.cginc"
            #include "QuadOverdrawAccumulate.cginc"

            // note: no SV_POSITION in this struct
            struct v2f
            {
                float2 placeholder : TEXCOORD0;
            };

            float4 _NormalizedComplexity;
            int _NumIteration;

            v2f vert(float4 vertex : POSITION, /*vertex position input*/
                out float4 outpos : SV_POSITION /*clip space position output*/)
            {
                v2f o;
                o.placeholder = float2(0, 0);

                outpos = UnityObjectToClipPos(vertex);
                return o;
            }

            [earlydepthstencil]
            fixed4 frag(v2f i, UNITY_VPOS_TYPE SvPosition : VPOS, uint PID : SV_PrimitiveID) : SV_Target
            {
                float complexity = ComputeQuadCoverage(SvPosition, PID, 64, true, true, 1);
                return float4(complexity, complexity, complexity, 1);
            }

            ENDCG
        }
    }
}
