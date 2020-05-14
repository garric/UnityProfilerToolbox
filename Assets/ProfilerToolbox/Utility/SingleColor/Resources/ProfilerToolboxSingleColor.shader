Shader "ProfilerToolbox/Utility/SingleColor"
{
    Properties
    {
        // Color property for material inspector, default to white
        _SingleColor("Main Color", Color) = (1,1,1,1)
        _AlphaSrcBlend("AlphaSrcBlend", float) = 1 /*BlendMode.One*/
        _AlphaSrcBlend("AlphaDstBlend", float) = 1 /*BlendMode.One*/
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }

        Pass
        {
            Fog { Mode Off }
            ZWrite Off
            Cull Off
            Blend One One, [_AlphaSrcBlend] [_AlphaSrcBlend]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // vertex shader
            // this time instead of using "appdata" struct, just spell inputs manually,
            // and instead of returning v2f struct, also just return a single output
            // float4 clip position
            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            // color from the material
            fixed4 _SingleColor;

            // pixel shader, no inputs needed
            fixed4 frag() : SV_Target
            {
                return _SingleColor; // just return it
            }
            ENDCG
        }
    }
}