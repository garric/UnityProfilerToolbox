namespace ProfilerToolbox 
{
    using UnityEngine;

    internal class ReplacementRendererGeneric : IReplacementRenderer 
    {
        private Renderer renderer;
        private Material[] sharedMaterials;

        public ReplacementRendererGeneric(Renderer renderer)
        {
            this.renderer = renderer;
            sharedMaterials = renderer.sharedMaterials;
        }

        public void Replace(Material replacementMaterial)
        {
            if (renderer && sharedMaterials != null)
            {
                Material[] materials = new Material[sharedMaterials.Length];
                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    if (sharedMaterials[i] == null)
                        continue;

                    materials[i] = replacementMaterial;
                }
                renderer.sharedMaterials = materials;
            }
        }

        public void Restore()
        {
            if (renderer)
                renderer.sharedMaterials = sharedMaterials;
        }

        public void SetMaterialPropertyColor(string property, Color value, object context)
        {
            if (renderer && sharedMaterials != null)
            {
                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    if (sharedMaterials[i] == null)
                        continue;

                    renderer.sharedMaterials[i].SetColor(property, value);
                }
            }
        }
    }
}
