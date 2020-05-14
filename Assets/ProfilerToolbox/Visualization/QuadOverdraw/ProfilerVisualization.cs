namespace ProfilerToolbox.Visualization 
{
    namespace QuadOverdraw 
    {
        using UnityEngine;
        using System.Collections.Generic;

        /// <summary>
        /// In order to make shading more correctly, we should launch a pre-depth pass to write opaque depth first!
        /// 
        /// ref:
        /// Quad Overdraw
        /// https://blog.selfshadow.com/2012/11/12/counting-quads/#meat
        /// UE QuadComplexityAccumulatePixelShader.usf
        /// 
        /// VPOS Screen Position
        /// https://docs.unity3d.com/Manual/SL-ShaderSemantics.html
        /// </summary>
        [System.Serializable]
        public class ProfilerVisualization : IProfilerVisualization 
        {
            public bool enable { get; set; }

            private ShadingView shadingView;

            private void Clear()
            {
                if (shadingView)
                {
                    shadingView.Exit();
                    shadingView = null;

                    Graphics.ClearRandomWriteTargets();
                }
            }

            public void Enter(Camera camera)
            {
                shadingView = UtilityCommon.TryAddComponent<ShadingView>(camera.gameObject);
                shadingView.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector | HideFlags.HideInInspector;
                shadingView.Enter(camera);
            }

            public void Exit()
            {
                Clear();
            }
        }
    }
}
