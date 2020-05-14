namespace ProfilerToolbox 
{
    using System.Collections.Generic;
    using UnityEngine;

    internal static class UtilityReplacementRenderer
    {
        private static IReplacementRenderer CreateReplacementRenderer(Object renderer)
        {
            if (renderer is ParticleSystemRenderer)
            {
                ReplacementRendererParticle particle = new ReplacementRendererParticle((ParticleSystemRenderer)renderer);
                return particle;
            }
            else if (renderer is CanvasRenderer)
            {
                ReplacementRendererCanvas canvas = new ReplacementRendererCanvas((CanvasRenderer)renderer);
                return canvas;
            }

            if (renderer is Renderer)
            {
                ReplacementRendererGeneric generic = new ReplacementRendererGeneric((Renderer)renderer);
                return generic;
            }
            else
            {
                Debug.LogError($"CreateRenderer not supported renderer {renderer.name} {renderer.GetType()}");
                return null;
            }
        }

        public static List<IReplacementRenderer> FindReplacementRenderers(ReplacementRendererSource source, bool transparent = true)
        {
            List<IReplacementRenderer> replacementRenderers = new List<IReplacementRenderer>();
            if ((source & ReplacementRendererSource.Renderer) > 0)
            {
                List<Renderer> renderers = UtilityCommon.FindObjectsOfAll<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (transparent && !IsTransparentShader(renderer.sharedMaterial))
                        continue;

                    replacementRenderers.Add(CreateReplacementRenderer(renderer));
                }                    
            }
            else
            {
                if ((source & ReplacementRendererSource.ParticleSystem) > 0)
                {
                    List<ParticleSystem> particleSystems = UtilityCommon.FindObjectsOfAll<ParticleSystem>();
                    foreach (ParticleSystem particleSystem in particleSystems)
                    {
                        replacementRenderers.Add(CreateReplacementRenderer(particleSystem.GetComponent<ParticleSystemRenderer>()));
                    }
                }
            }
            if ((source & ReplacementRendererSource.CanvasRenderer) > 0)
            {
                List<CanvasRenderer> canvasRenderers = UtilityCommon.FindObjectsOfAll<CanvasRenderer>();
                foreach (CanvasRenderer canvasRender in canvasRenderers)
                {
                    replacementRenderers.Add(CreateReplacementRenderer(canvasRender));
                }
            }

            return replacementRenderers;
        }

        public static bool IsTransparentShader(Material material)
        {
            if (material == null || material.shader == null)
                return false;

            // better method
            if (material.renderQueue < 3000)
                return false;

            return true;
        }
    }
}