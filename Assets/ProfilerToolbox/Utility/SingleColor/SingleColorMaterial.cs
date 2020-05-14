namespace ProfilerToolbox 
{
    using UnityEngine;

    internal static class SingleColorMaterial 
    {
        private static Material m_material;
        public static Material material {
            get {
                if (m_material == null)
                    m_material = new Material(Resources.Load<Shader>("ProfilerToolboxSingleColor"));
                return m_material;
            }
        }

        public static string COLOR_PROPERTY = "_SingleColor";
    }
}