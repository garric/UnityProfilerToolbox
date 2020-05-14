namespace ProfilerToolbox 
{
    using UnityEngine;

    internal class ReplacementRendererParticle : IReplacementRenderer 
    {
        public enum MaterialType 
        {
            Material,
            TrailMaterial,
        }

        private ParticleSystemRenderer renderer;
        private Material[] sharedMaterials;
        private Material trailMaterial;

        public ReplacementRendererParticle(ParticleSystemRenderer renderer)
        {
            this.renderer = renderer;
            sharedMaterials = renderer.sharedMaterials;
            trailMaterial = renderer.trailMaterial;
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
            if (renderer && trailMaterial)
                renderer.trailMaterial = replacementMaterial;
        }

        public void Restore()
        {
            if (renderer)
                renderer.sharedMaterials = sharedMaterials;
            if (renderer)
                renderer.trailMaterial = trailMaterial;
        }

        public void SetMaterialPropertyColor(string property, Color value, object context)
        {
            bool replaceMaterialColor = false;
            bool replaceTrailMaterialColor = false;
            if (context != null)
            {
                if ((MaterialType)context == MaterialType.Material)
                    replaceMaterialColor = true;
                else
                    replaceTrailMaterialColor = true;
            }
            else
            {
                replaceMaterialColor = true;
                replaceTrailMaterialColor = true;
            }

            if (replaceMaterialColor && renderer && sharedMaterials != null)
            {
                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    if (sharedMaterials[i] == null)
                        continue;

                    renderer.sharedMaterials[i].SetColor(property, value);
                }
            }
            if (replaceTrailMaterialColor && renderer && trailMaterial)
                renderer.trailMaterial.SetColor(property, value);
        }
    }
}
