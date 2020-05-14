namespace ProfilerToolbox.Visualization
{
    namespace QuadOverdraw 
    {
        using UnityEngine;

        internal class ShadingRenderer
        {
            private Renderer renderer;
            private Material[] sharedMaterials;
            private Vector2 overdrawBufferDimension = Vector2.one;

            private bool m_reset = true;

            public ShadingRenderer(Renderer source, Vector2 overdrawBufferDimension)
            {
                renderer = source;
                sharedMaterials = renderer.sharedMaterials;
                this.overdrawBufferDimension = overdrawBufferDimension;
            }

            public void Shading()
            {
                m_reset = false;

                Material[] materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0, imax = renderer.sharedMaterials.Length; i < imax; i++)
                {
                    if (renderer.sharedMaterials[i] == null)
                        continue;

                    materials[i] = new Material(ShadingMaterial.quadOverdrawAccumulate);
                    materials[i].SetVector(ShadingMaterial.ID_RWBUFFER_DIMENSION, new Vector4(overdrawBufferDimension.x, overdrawBufferDimension.y, 0, 0));
                }
                renderer.sharedMaterials = materials;

                Refresh();
            }

            public void Refresh()
            {
                for (int i = 0, imax = sharedMaterials.Length; i < imax; i++)
                {
                    Material sharedMaterial = sharedMaterials[i];
                    if (sharedMaterial == null)
                        continue;

                    Material materialQuadOverdraw = renderer.sharedMaterials[i];
                    // Graphics API will auto bind the RWBuffer to given slot
                    //materialQuadOverdraw.SetTexture("RWQuadBuffer", overdrawBuffer);

                    // set render state
                    //materialQuadOverdraw.SetFloat("_Cull", UtilityRenderState.GetCullMode(material));
                    //materialQuadOverdraw.SetFloat("_ZWrite", UtilityRenderState.GetZWrite(material));
                    //materialQuadOverdraw.SetFloat("_ZTest", UtilityRenderState.GetZTest(material));
                }
            }

            public void Reset()
            {
                m_reset = true;
                if (renderer)
                    renderer.sharedMaterials = sharedMaterials;
            }

            [ContextMenu("Switch")]
            void Switch()
            {
                if (m_reset)
                    Shading();
                else
                    Reset();
            }
        }
    }
}