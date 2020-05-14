namespace ProfilerToolbox.Visualization 
{
    namespace ParticleFlat 
    {
        using UnityEngine;
        using System.Collections.Generic;

        [DisallowMultipleComponent]
        [ExecuteAlways]
        internal class ShadingView : MonoBehaviour 
        {
            private List<ShadingRenderer> shadingRenderers = new List<ShadingRenderer>();
            public ShadingSetting setting;
         
            public void Enter(Camera camera)
            {
                // shading renderers
                List<IReplacementRenderer> replacementRenderers = UtilityReplacementRenderer.FindReplacementRenderers(ReplacementRendererSource.ParticleSystem);
                shadingRenderers = new List<ShadingRenderer>(replacementRenderers.Count);
                foreach (IReplacementRenderer replacementRenderer in replacementRenderers)
                {
                    ShadingRenderer shadingRenderer = new ShadingRenderer(replacementRenderer);
                    shadingRenderer.Shading();

                    shadingRenderers.Add(shadingRenderer);
                }
                RefreshShading();
            }

            public void Exit()
            {
                foreach (ShadingRenderer renderer in shadingRenderers)
                    renderer.Reset();
                shadingRenderers.Clear();

                UtilityCommon.Destroy(this);
            }

            public void RefreshShading()
            {
                foreach (ShadingRenderer renderer in shadingRenderers)
                    renderer.SetColor(setting.colorMateria, setting.colorTrailMaterial);
            }
        }
    }
}