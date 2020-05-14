namespace ProfilerToolbox.Visualization
{ 
    namespace QuadOverdraw 
    {
        using UnityEngine;
        using System.Collections.Generic;

        [ExecuteAlways]
        internal class ShadingView : MonoBehaviour 
        {
            public RenderTexture overdrawBuffer;

            public Material materialApply;
            public Material materialClear;

            private List<ShadingRenderer> shadingRenderers = new List<ShadingRenderer>();

            private void CreateOverdrawBuffer(Camera camera)
            {
                if (overdrawBuffer == null || overdrawBuffer.width != camera.pixelWidth || overdrawBuffer.height != camera.pixelHeight)
                {
                    if (overdrawBuffer)
                    {
                        UtilityCommon.Destroy(overdrawBuffer);
                    }

#if UNITY_EDITOR
                    overdrawBuffer = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.RInt);
#else
                    // Must be ARGB32 but will get automatically converted to float or float4 or int or half, from your shader code declaration.
                    overdrawBuffer = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
#endif
                    overdrawBuffer.name = $"Overdraw Buffer {camera.name}";
                    overdrawBuffer.enableRandomWrite = true;
                    overdrawBuffer.Create();
                }
            }

            private void ClearOverdrawBuffer()
            {
                if (materialClear == null)
                {
                    materialClear = new Material(ShadingMaterial.quadOverdrawClear);
                    materialClear.SetVector(ShadingMaterial.ID_RWBUFFER_DIMENSION, new Vector4(overdrawBuffer.width, overdrawBuffer.height, 0, 0));
                }

                RenderTexture temporary = RenderTexture.GetTemporary(Camera.current.pixelWidth, Camera.current.pixelHeight);
                Graphics.Blit(Camera.current.activeTexture, temporary, materialClear);
                RenderTexture.ReleaseTemporary(temporary);
            }

            public void Enter(Camera camera)
            {
                CreateOverdrawBuffer(camera);

                // collect all materials of sceneView
                // although this is not a good method, but we can't directly replace renderer's shader with uniforms
                // no UI and CommandBuffer now
                List<Renderer> allRenderers = UtilityCommon.FindObjectsOfAll<Renderer>();
                shadingRenderers = new List<ShadingRenderer>(allRenderers.Count);
                foreach (Renderer renderer in allRenderers)
                {
                    ShadingRenderer shadingRenderer = new ShadingRenderer(renderer, new Vector2(overdrawBuffer.width, overdrawBuffer.height));
                    shadingRenderer.Shading();

                    shadingRenderers.Add(shadingRenderer);
                }
            }

            public void Exit()
            {
                foreach (ShadingRenderer renderer in shadingRenderers)
                    renderer.Reset();
                shadingRenderers.Clear();

                if (overdrawBuffer != null)
                    UtilityCommon.Destroy(overdrawBuffer);

                UtilityCommon.Destroy(materialClear);
                UtilityCommon.Destroy(materialApply);
                UtilityCommon.Destroy(this);
            }

            private void OnPreRender()
            {
#if UNITY_EDITOR
                if (Camera.current.name.Contains("SceneCamera"))
                    return;
#endif
                Graphics.SetRandomWriteTarget(1, overdrawBuffer);
                ClearOverdrawBuffer();
            }

            private void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                if (overdrawBuffer != null)
                {
                    // apply
                    if (materialApply == null)
                    {
                        materialApply = new Material(ShadingMaterial.quadOverdrawApply);
                        materialApply.SetVector(ShadingMaterial.ID_RWBUFFER_DIMENSION, new Vector4(overdrawBuffer.width, overdrawBuffer.height, 0, 0));
                    }                        
                    materialApply.SetTexture("RWQuadBuffer", overdrawBuffer);
                    Graphics.Blit(source, destination, materialApply);
                }
                else
                    Graphics.Blit(source, destination);
            }
        }
    }
}