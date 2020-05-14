namespace ProfilerToolbox.Visualization 
{
    namespace PixelOverdraw 
    {
        using UnityEngine;
        using System.Collections.Generic;

        [System.Serializable]
        public class ProfilerVisualization : IProfilerVisualization 
        {
            public bool enable { get; set; }

            public ShadingSetting setting = new ShadingSetting();

            private ShadingView shadingView;

            public void Enter(Camera camera)
            {
                Clear();
                shadingView = UtilityCommon.TryAddComponent<ShadingView>(camera.gameObject);
                shadingView.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector | HideFlags.HideInInspector;
                shadingView.setting = setting;
                shadingView.Enter(camera);


                //if (false/*targetCamera is UICamera*/)
                //{
                //    OptionOpaqueBlack();
                //    OptionShowWhiteNumber();
                //    ShadingView.usingCustomizedRenderer = true;
                //}
            }

            private void Clear()
            {
                if (shadingView)
                {
                    //if (false/*targetCamera is UICamera*/)
                    //{
                    //    ShadingView.usingCustomizedRenderer = false;
                    //}

                    shadingView.Exit();
                    shadingView = null;
                }
            }

            public void Exit()
            {
                Clear();
            }

            #region Options
            public static bool IsOverdrawDisplayOn()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return false;

                return true;
            }

            private static ShadingView FindActiveShadingView()
            {
                foreach (Camera camera in Camera.allCameras)
                {
                    ShadingView shadingView = camera.GetComponent<ShadingView>();
                    if (shadingView == null || !shadingView.isActiveAndEnabled)
                        continue;

                    return shadingView;
                }
                return null;
            }

            public static void OptionToolgeRealtimeStatistics()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                shadingView.setting.realtimeStatistics = !shadingView.setting.realtimeStatistics;
            }

            public static void OptionRefresh()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                shadingView.setting.refreshStatistics = true;
            }

            public static void OptionShowAlpha()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                shadingView.setting.showAlpha = !shadingView.setting.showAlpha;
            }

            public static void OptionAdjustGridSize(bool increase, int option)
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                int sizeX = shadingView.gridSizeInt.x;
                int sizeY = shadingView.gridSizeInt.y;
                if (increase)
                {
                    if (option == 1)
                    {
                        sizeX = Mathf.CeilToInt(sizeX * 1.5f);
                    }
                    else if (option == 2)
                    {
                        sizeY = Mathf.CeilToInt(sizeY * 1.5f);
                    }
                    else
                    {
                        sizeX = Mathf.CeilToInt(sizeX * 1.5f);
                        sizeY = Mathf.CeilToInt(sizeY * 1.5f);
                    }
                    sizeX = Mathf.Min(shadingView.targetCamera.pixelWidth / 4, sizeX);
                    sizeY = Mathf.Min(shadingView.targetCamera.pixelHeight / 4, sizeY);
                }
                else
                {
                    if (option == 1)
                    {
                        sizeX = Mathf.CeilToInt(sizeX * 0.8f);
                    }
                    else if (option == 2)
                    {
                        sizeY = Mathf.CeilToInt(sizeY * 0.8f);
                    }
                    else
                    {
                        sizeX = Mathf.CeilToInt(sizeX * 0.8f);
                        sizeY = Mathf.CeilToInt(sizeY * 0.8f);
                    }
                    sizeX = Mathf.Max(ShadingSetting.GRIDSIZE_MIN, sizeX);
                    sizeY = Mathf.Max(ShadingSetting.GRIDSIZE_MIN, sizeY);
                }
                shadingView.gridSize = new Vector2(sizeX, sizeY);
                shadingView.setting.resetStatistics = true;
            }

            public static void OptionSetGridSize(int sizeX, int sizeY)
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                sizeX = Mathf.Min(shadingView.targetCamera.pixelWidth / 4, Mathf.Max(ShadingSetting.GRIDSIZE_MIN, sizeX));
                sizeY = Mathf.Min(shadingView.targetCamera.pixelHeight / 4, Mathf.Max(ShadingSetting.GRIDSIZE_MIN, sizeY));
                shadingView.gridSize = new Vector2(sizeX, sizeY);
                shadingView.setting.resetStatistics = true;
            }

            public static void OptionCheckPlatformRenderingConvension()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                shadingView.setting.checkPlatformRenderingConvension = !shadingView.setting.checkPlatformRenderingConvension;
                if (shadingView.setting.checkPlatformRenderingConvension)
                {
                    shadingView.setting.lastGridSize = shadingView.gridSize;
                    shadingView.gridSize = new Vector2(shadingView.targetCamera.pixelWidth / 10, shadingView.targetCamera.pixelHeight / 10);
                }
                else
                {
                    shadingView.gridSize = shadingView.setting.lastGridSize;
                }
                shadingView.setting.resetStatistics = true;
            }

            public static void OptionAdjustSaturation(bool increase)
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                float saturation = shadingView.setting.overdrawColorSaturation;
                if (increase)
                {
                    if (saturation > 30)
                        saturation += 10;
                    else if (saturation > 10)
                        saturation += 5;
                    else
                        saturation += 1;
                    saturation = Mathf.Min(100, saturation);
                }
                else
                {
                    if (saturation <= 10)
                        saturation -= 1;
                    else if (saturation <= 30)
                        saturation -= 5;
                    else
                        saturation -= 10;
                    saturation = Mathf.Max(1, saturation);
                    shadingView.setting.overdrawColorSaturation = saturation;
                }

                shadingView.setting.overdrawColorSaturation = saturation;
                shadingView.RefreshShading();
            }

            public static void OptionShowWhiteNumber()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                shadingView.setting.whiteNumber = !shadingView.setting.whiteNumber;
                shadingView.RefreshShading();
            }

            public static void OptionOpaqueBlack()
            {
                ShadingView shadingView = FindActiveShadingView();
                if (shadingView == null)
                    return;

                shadingView.setting.opaqueBlack = !shadingView.setting.opaqueBlack;
                shadingView.RefreshShading();
            }
            #endregion
        }
    }
}
