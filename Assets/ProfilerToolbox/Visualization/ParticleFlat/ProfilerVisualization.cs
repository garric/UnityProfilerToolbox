namespace ProfilerToolbox.Visualization 
{
    namespace ParticleFlat 
    {
        using UnityEngine;

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
            }

            private void Clear()
            {
                if (shadingView)
                {
                    shadingView.Exit();
                    shadingView = null;
                }
            }

            public void Exit()
            {
                Clear();
            }
        }
    }
}
