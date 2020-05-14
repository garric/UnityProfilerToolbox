namespace ProfilerToolbox.Visualization 
{
    namespace QuadOverdraw 
    {
        using UnityEngine;

        internal class ShadingMaterial 
        {
            public const string ID_RWBUFFER_DIMENSION = "_RWQuadBuffer_Dimension";

            private static Shader m_quadOverdrawClear;
            public static Shader quadOverdrawClear {
                get {
                    if (m_quadOverdrawClear == null)
                        m_quadOverdrawClear = Resources.Load<Shader>("QuadOverdrawClear");
                    return m_quadOverdrawClear;
                }
            }

            private static Shader m_quadOverdrawAccumulate;
            public static Shader quadOverdrawAccumulate {
                get {
                    if (m_quadOverdrawAccumulate == null)
                        m_quadOverdrawAccumulate = Resources.Load<Shader>("QuadOverdrawAccumulate");
                    return m_quadOverdrawAccumulate;
                }
            }

            private static Shader m_quadOverdrawApply;
            public static Shader quadOverdrawApply {
                get {
                    if (m_quadOverdrawApply == null)
                        m_quadOverdrawApply = Resources.Load<Shader>("QuadOverdrawApply");
                    return m_quadOverdrawApply;
                }
            }
        }
    }
}
