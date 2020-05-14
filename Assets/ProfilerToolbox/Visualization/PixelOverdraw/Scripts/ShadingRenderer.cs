namespace ProfilerToolbox.Visualization 
{
    namespace PixelOverdraw 
    {
        using UnityEngine;

        internal class ShadingRenderer 
        {
            private IReplacementRenderer replacementRenderer;

            internal ShadingRenderer(IReplacementRenderer replacementRenderer)
            {
                this.replacementRenderer = replacementRenderer;
            }

            public void Shading()
            {
                Material material = new Material(SingleColorMaterial.material);
                replacementRenderer.Replace(material);
            }

            public void SetColor(Color color)
            {
                replacementRenderer.SetMaterialPropertyColor(SingleColorMaterial.COLOR_PROPERTY, color);
            }

            public void Reset()
            {
                replacementRenderer.Restore();
            }
        }
    }
}
