namespace ProfilerToolbox 
{
    using UnityEngine;

    internal class ReplacementRendererCanvas : IReplacementRenderer 
    {
        private CanvasRenderer renderer;
        private Material material;

        public ReplacementRendererCanvas(CanvasRenderer renderer)
        {
            this.renderer = renderer;
            material = renderer.GetMaterial();
        }

        public void Replace(Material replacementMaterial)
        {
            if (renderer && material)
                renderer.SetMaterial(replacementMaterial, 0);
        }

        public void Restore()
        {
            if (renderer)
                renderer.SetMaterial(material, 0);
        }

        public void SetMaterialPropertyColor(string property, Color value, object context)
        {
            if (renderer && material != null)
            {
                Material tempMaterial = renderer.GetMaterial();
                tempMaterial.SetColor(property, value);
                renderer.SetMaterial(tempMaterial, 0);
            }
        }
    }
}
