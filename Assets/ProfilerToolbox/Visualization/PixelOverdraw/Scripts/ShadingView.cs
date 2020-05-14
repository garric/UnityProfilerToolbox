namespace ProfilerToolbox.Visualization 
{
    namespace PixelOverdraw 
    {
        using UnityEngine;
        using UnityEngine.Rendering;
        using System.Collections.Generic;

        [DisallowMultipleComponent]
        [ExecuteAlways]
        internal class ShadingView : MonoBehaviour 
        {
            public Camera targetCamera;

            public Material materialClearOpaque;
            public Material materialDisplayShowAlpha;
            public Material materialApply;

            public CommandBuffer commandBufferBeforeForwardAlpha;
            public CommandBuffer commandBufferBeforeImageEffects;

            private List<ShadingRenderer> shadingRenderers = new List<ShadingRenderer>();
            public ShadingSetting setting;

            public struct SizeInt 
            {
                public int x;
                public int y;

                public SizeInt(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                }
            }

            public Texture textureNumber;

            public Mesh meshGrids;
            public Vector2[] meshGridsUV;
            public Vector2 gridSize = new Vector2(ShadingSetting.GRIDSIZE_MIN, ShadingSetting.GRIDSIZE_MIN);
            public SizeInt gridSizeInt = new SizeInt(ShadingSetting.GRIDSIZE_MIN, ShadingSetting.GRIDSIZE_MIN);
            public int[] gridsAlphaCount;
            public float averageOverdraw = 0;
            public int realtimeStatisticsFrequencyIndex = 0;
            public int gridH;
            public int gridV;            

            public void Enter(Camera camera)
            {
                if (textureNumber == null)
                    textureNumber = Resources.Load<Texture>("Numbers_64pt_alpha");

                if (materialClearOpaque == null)
                    materialClearOpaque = new Material(ShadingMaterial.pixelOverdrawClear);
                if (materialDisplayShowAlpha == null)
                    materialDisplayShowAlpha = new Material(ShadingMaterial.pixelOverdrawShowAlpha);
                if (materialApply == null)
                    materialApply = new Material(ShadingMaterial.pixelOverdrawApply);

                targetCamera = camera;

                // shading renderers
                List<IReplacementRenderer> replacementRenderers = UtilityReplacementRenderer.FindReplacementRenderers(ReplacementRendererSource.Renderer | ReplacementRendererSource.CanvasRenderer);
                shadingRenderers = new List<ShadingRenderer>(replacementRenderers.Count);
                foreach (IReplacementRenderer replacementRenderer in replacementRenderers)
                {
                    ShadingRenderer shadingRenderer = new ShadingRenderer(replacementRenderer);
                    shadingRenderer.Shading();

                    shadingRenderers.Add(shadingRenderer);
                }
                RefreshShading();

                // command buffer
                commandBufferBeforeForwardAlpha = new CommandBuffer() { name = "PixelOvedraw ForwardAlpha" };
                targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBufferBeforeForwardAlpha);
                commandBufferBeforeImageEffects = new CommandBuffer() { name = "PixelOvedraw BeforeImageEffects" };
                targetCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBufferBeforeImageEffects);

                // calculation appropriate
                int sizeX = 60;
                int sizeY = (int)(60 / targetCamera.aspect);
                gridSize = new Vector2(sizeX, sizeY);
                RecalculateGrids();

                setting.showAlpha = false;
                setting.refreshStatistics = true;
                setting.checkPlatformRenderingConvension = false;
            }

            public void Exit()
            {
                UtilityCommon.Destroy(materialClearOpaque);
                UtilityCommon.Destroy(materialApply);
                UtilityCommon.Destroy(materialDisplayShowAlpha);

                if (commandBufferBeforeForwardAlpha != null)
                    targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBufferBeforeForwardAlpha);
                if (commandBufferBeforeImageEffects != null)
                    targetCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, commandBufferBeforeImageEffects);

                foreach (ShadingRenderer renderer in shadingRenderers)
                    renderer.Reset();
                shadingRenderers.Clear();

                UtilityCommon.Destroy(this);
            }

            public void RefreshShading()
            {
                materialClearOpaque.SetFloat("_ColorSrcBlend", setting.opaqueBlack ? (int)BlendMode.One : (int)BlendMode.Zero);
                materialClearOpaque.SetFloat("_ColorDstBlend", setting.opaqueBlack ? (int)BlendMode.Zero : (int)BlendMode.One);

                materialApply.SetFloat("_WhiteNumber", setting.whiteNumber ? 1 : 0);
#if UNITY_EDITOR_WIN
                materialApply.SetFloat("_FlipY", 1);
#endif


                Color color = new Color(setting.overdrawColorSaturation * ShadingSetting.ALPHA_ONE, 0, 0, ShadingSetting.ALPHA_ONE);
                foreach (ShadingRenderer renderer in shadingRenderers)
                    renderer.SetColor(color);
            }

            private void OnPreCull()
            {
                if (commandBufferBeforeForwardAlpha == null)
                    return;

                // clear FrameBuffer contents accumulated before transparent rendering
                {
                    commandBufferBeforeForwardAlpha.Clear();
                    var cameraTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
                    commandBufferBeforeForwardAlpha.DrawMesh(UtilityGeometry.fullscreenTriangle, Matrix4x4.identity, materialClearOpaque);
                }

                // 
                {
                    commandBufferBeforeImageEffects.Clear();
                    if (setting.showAlpha)
                    {
                        //var cameraTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
                        //RenderTexture renderTexture = RenderTexture.GetTemporary(targetCamera.pixelWidth, targetCamera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                        //commandBufferBeforeImageEffects.Blit(cameraTarget, renderTexture);
                        //materialDisplayShowAlpha.SetTexture("_MainTex", renderTexture);
                        //commandBufferBeforeImageEffects.SetRenderTarget(cameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                        //commandBufferBeforeImageEffects.DrawMesh(UtilityGeometry.fullscreenTriangle, Matrix4x4.identity, materialDisplayShowAlpha);
                        //RenderTexture.ReleaseTemporary(renderTexture);

                        materialDisplayShowAlpha.SetTexture("_MainTex", setting.textureScreen);
                        commandBufferBeforeImageEffects.DrawMesh(UtilityGeometry.fullscreenTriangle, Matrix4x4.identity, materialDisplayShowAlpha);
                    }
                    else
                        commandBufferBeforeImageEffects.DrawMesh(meshGrids, Matrix4x4.identity, materialApply);
                }
            }

            private void GetNumberUV(int number, out float uvL, out float uvR, out float uvB, out float uvT)
            {
                uvL = (number % 10) * 0.1f;
                uvR = (number % 10 + 1) * 0.1f;
//#if !UNITY_IOS
                uvT = (number / 10) * 0.1f;
                uvB = (number / 10 + 1) * 0.1f;
//#else
//                uvB = (number / 10) * 0.1f;
//                uvT = (number / 10 + 1) * 0.1f;
//#endif
            }

            public void RecalculateGrids()
            {
                materialApply.SetTexture("_MainTex", textureNumber);

                gridSizeInt.x = Mathf.Max(ShadingSetting.GRIDSIZE_MIN, (int)gridSize.x);
                gridSizeInt.y = Mathf.Max(ShadingSetting.GRIDSIZE_MIN, (int)gridSize.y);

                gridH = targetCamera.pixelWidth / gridSizeInt.x + ((targetCamera.pixelWidth % gridSizeInt.x == 0) ? 0 : 1);
                gridV = targetCamera.pixelHeight / gridSizeInt.y + ((targetCamera.pixelHeight % gridSizeInt.y == 0) ? 0 : 1);

                if (gridsAlphaCount == null || gridsAlphaCount.Length != gridH * gridV)
                    gridsAlphaCount = new int[gridH * gridV];

                // Reconstruct Mesh
                meshGrids = new Mesh { name = $"Overdraw Display {gridSizeInt}" };
                int quadVertices = 4 * gridH * gridV;
                Vector3[] vertices = new Vector3[quadVertices];
                meshGridsUV = new Vector2[quadVertices];
                int[] triangles = new int[(quadVertices / 2) * 3];

                int number = 0;
                //// Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
                //// this directly in the vertex shader using vertex ids :(
                float normalizeGridSizeH = ((gridH * gridSizeInt.x) / (float)targetCamera.pixelWidth) / gridH;
                float normalizeGridSizeV = ((gridV * gridSizeInt.y) / (float)targetCamera.pixelHeight) / gridV;
                for (int v = 0; v < gridV; v++)
                {
                    for (int h = 0; h < gridH; h++)
                    {
                        int index = v * gridH + h;

                        // from  -1 to 1
                        float left = -1 + 2 * h * normalizeGridSizeH;
                        float right = -1 + 2 * (h + 1) * normalizeGridSizeH;
                        // from  -1 to 1
                        float bottom = -1 + 2 * v * normalizeGridSizeV;
                        float top = -1 + 2 * (v + 1) * normalizeGridSizeV;

                        vertices[4 * index + 0] = new Vector3(left, bottom, 0);
                        vertices[4 * index + 1] = new Vector3(left, top, 0);
                        vertices[4 * index + 2] = new Vector3(right, top, 0);
                        vertices[4 * index + 3] = new Vector3(right, bottom, 0);

                        if (!setting.checkPlatformRenderingConvension)
                            number = gridsAlphaCount[index];
                        float uvL, uvR, uvB, uvT;
                        GetNumberUV(number, out uvL, out uvR, out uvB, out uvT);
                        meshGridsUV[4 * index + 0] = new Vector2(uvL, uvB);
                        meshGridsUV[4 * index + 1] = new Vector2(uvL, uvT);
                        meshGridsUV[4 * index + 2] = new Vector2(uvR, uvT);
                        meshGridsUV[4 * index + 3] = new Vector2(uvR, uvB);
                        if (setting.checkPlatformRenderingConvension)
                            number = (number + 1) % 100;

                        // lower left triangle, then upper right triangle
                        triangles[6 * index + 0] = 4 * index + 0;
                        triangles[6 * index + 1] = 4 * index + 1;
                        triangles[6 * index + 2] = 4 * index + 3;
                        triangles[6 * index + 3] = 4 * index + 1;
                        triangles[6 * index + 4] = 4 * index + 2;
                        triangles[6 * index + 5] = 4 * index + 3;
                    }
                }

                meshGrids.vertices = vertices;
                meshGrids.uv = meshGridsUV;
                meshGrids.triangles = triangles;
                meshGrids.RecalculateBounds();
                meshGrids.UploadMeshData(false);

                RefreshShading();
            }

            private void OnPostRender()
            {
                if (setting.textureScreen == null || setting.textureScreen.width != targetCamera.pixelWidth || setting.textureScreen.height != targetCamera.pixelHeight)
                {
                    UtilityCommon.Destroy(setting.textureScreen);

                    setting.textureScreen = new Texture2D(targetCamera.pixelWidth, targetCamera.pixelHeight, TextureFormat.RGBA32, false, true);
                    RecalculateGrids();
                }

                if (setting.resetStatistics)
                {
                    setting.resetStatistics = false;
                    RecalculateGrids();
                    RefreshStatistics();
                }
                else if (setting.refreshStatistics)
                {
                    RefreshStatistics();
                }
                else if (setting.realtimeStatistics)
                {
                    realtimeStatisticsFrequencyIndex = (realtimeStatisticsFrequencyIndex + 1) % setting.realtimeStatisticsFrequency;
                    if (realtimeStatisticsFrequencyIndex == 0)
                        RefreshStatistics();
                }
            }

            private void RefreshStatistics()
            {
                setting.refreshStatistics = false;

                setting.textureScreen.ReadPixels(new Rect(0, 0, setting.textureScreen.width, setting.textureScreen.height), 0, 0);
                setting.textureScreen.Apply();
                Color32[] pixels = setting.textureScreen.GetPixels32(0);
                int width = targetCamera.pixelWidth;
                int height = targetCamera.pixelHeight;
                int gridArea = gridSizeInt.x * gridSizeInt.y;
                int totalLength = width * height;
                averageOverdraw = 0;
                // left-right, bottom-up
                int number = 0;
                for (int v = 0; v < gridV; v++)
                {
                    for (int h = 0; h < gridH; h++)
                    {
                        int gridIndex = v * gridH + h;
                        int count = 0;
                        for (int i = 0; i < gridSizeInt.y; i++)
                        {
                            for (int j = 0; j < gridSizeInt.x; j++)
                            {
                                int hIndex = h * gridSizeInt.x + j;
                                if (hIndex >= width)
                                    continue;

                                int pixelIndex = (v * gridSizeInt.y + i) * width + hIndex;
                                if (pixelIndex >= totalLength)
                                    break;

                                count += pixels[pixelIndex].a;
                            }
                        }
                        averageOverdraw += count;
                        count = Mathf.CeilToInt((float)count / gridArea);
                        gridsAlphaCount[gridIndex] = count;

                        if (setting.checkPlatformRenderingConvension)
                            count = number;
                        float uvL, uvR, uvB, uvT;
                        GetNumberUV(count, out uvL, out uvR, out uvB, out uvT);
                        meshGridsUV[4 * gridIndex + 0] = new Vector2(uvL, uvB);
                        meshGridsUV[4 * gridIndex + 1] = new Vector2(uvL, uvT);
                        meshGridsUV[4 * gridIndex + 2] = new Vector2(uvR, uvT);
                        meshGridsUV[4 * gridIndex + 3] = new Vector2(uvR, uvB);
                        if (setting.checkPlatformRenderingConvension)
                            number = (number + 1) % 100;
                    }
                }
                meshGrids.uv = meshGridsUV;
                meshGrids.UploadMeshData(false);
                averageOverdraw = averageOverdraw / totalLength;
            }

            #region CustomizedRenderer
            private static bool m_usingCustomizedRenderer = false;
            public static bool usingCustomizedRenderer {
                get { return m_usingCustomizedRenderer; }
                set {
                    if (value == false)
                    {
                        Dictionary<Renderer, Material>.Enumerator iterator = m_replacedCustomizedRenderers.GetEnumerator();
                        while (iterator.MoveNext())
                        {
                            Renderer renderer = iterator.Current.Key;
                            Material material = iterator.Current.Value;
                            if (renderer)
                                renderer.material = material;
                        }
                        m_replacedCustomizedRenderers.Clear();
                    }
                    m_usingCustomizedRenderer = value;
                }
            }

            private static Dictionary<Renderer, Material> m_replacedCustomizedRenderers = new Dictionary<Renderer, Material>();
            public static void ReplaceCustomizedRendererMaterial(Renderer renderer, Material material)
            {
                m_replacedCustomizedRenderers[renderer] = material;
                renderer.material = new Material(SingleColorMaterial.material);
            }
            #endregion

            #region Editor Tools
#if UNITY_EDITOR
            //public bool convertAlpha;
            //public Texture2D sourceTexture;

            //public void Update()
            //{
            //    if (!convertAlpha || sourceTexture == null)
            //        return;

            //    convertAlpha = !convertAlpha;

            //    float threshold = 0.95f;
            //    Texture2D alphaTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
            //    alphaTexture.alphaIsTransparency = true;
            //    alphaTexture.anisoLevel = 0;
            //    for (int i = 0; i < sourceTexture.width; i++)
            //    {
            //        for (int j = 0; j < sourceTexture.height; j++)
            //        {
            //            Color color = sourceTexture.GetPixel(i, j);
            //            if (color.r >= threshold && color.g >= threshold && color.b >= threshold)
            //                color.a = 0;
            //            alphaTexture.SetPixel(i, j, color);
            //        }
            //    }
            //    alphaTexture.Apply();

            //    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(sourceTexture);
            //    assetPath = assetPath.Substring(0, assetPath.LastIndexOf(".")) + "_alpha.png";
            //    string filePath = Application.dataPath.Replace("Assets", string.Empty) + assetPath;
            //    Debug.LogError(filePath);
            //    if (System.IO.File.Exists(filePath))
            //        System.IO.File.Delete(filePath);
            //    System.IO.File.WriteAllBytes(filePath, alphaTexture.EncodeToPNG());

            //    UnityEditor.AssetDatabase.Refresh();
            //}
#endif
#endregion
        }
    }
}