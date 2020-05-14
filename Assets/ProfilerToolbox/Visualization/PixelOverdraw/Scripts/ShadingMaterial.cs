namespace ProfilerToolbox.Visualization 
{
    namespace PixelOverdraw 
    {
        using UnityEngine;

        internal class ShadingMaterial 
        {
            private static Shader m_pixelOverdrawClear;
            public static Shader pixelOverdrawClear {
                get {
                    if (m_pixelOverdrawClear == null)
                        m_pixelOverdrawClear = Resources.Load<Shader>("PixelOverdrawClearOpaque");
                    return m_pixelOverdrawClear;
                }
            }

            private static Shader m_pixelOverdrawShowAlpha;
            public static Shader pixelOverdrawShowAlpha {
                get {
                    if (m_pixelOverdrawShowAlpha == null)
                        m_pixelOverdrawShowAlpha = Resources.Load<Shader>("PixelOverdrawShowAlpha");
                    return m_pixelOverdrawShowAlpha;
                }
            }

            private static Shader m_pixelOverdrawApply;
            public static Shader pixelOverdrawApply {
                get {
                    if (m_pixelOverdrawApply == null)
                        m_pixelOverdrawApply = Resources.Load<Shader>("PixelOverdrawApply");
                    return m_pixelOverdrawApply;
                }
            }
        }
    }
}
