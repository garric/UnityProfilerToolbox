namespace ProfilerToolbox.Visualization 
{
    namespace ParticleFlat
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

            public void SetColor(Color colorMaterial, Color colorTrailMaterial)
            {
                replacementRenderer.SetMaterialPropertyColor(SingleColorMaterial.COLOR_PROPERTY, colorMaterial, ReplacementRendererParticle.MaterialType.Material);
                replacementRenderer.SetMaterialPropertyColor(SingleColorMaterial.COLOR_PROPERTY, colorTrailMaterial, ReplacementRendererParticle.MaterialType.TrailMaterial);
            }

            public void Reset()
            {
                replacementRenderer.Restore();
            }
        }
    }
}
