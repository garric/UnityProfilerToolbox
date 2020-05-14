namespace ProfilerToolbox.Visualization 
{
    using UnityEngine;

    public enum Type 
    {
        None,
        QuadOverdraw,
        PixelOverdraw,
        ParticleFlat,
    }

    public interface IProfilerVisualization 
    {
        bool enable { get; set; }

        void Enter(Camera camera);
        void Exit();
    }
}
