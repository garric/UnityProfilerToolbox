namespace ProfilerToolbox 
{
    ///
    /// 1.using sharedMaterials not materials in editor mode, or you will get following warnings on console
    /// 
    /// Instantiating material due to calling renderer.material during edit mode.
    /// This will leak materials into the scene. You most likely want to use renderer.sharedMaterial instead.
    ///
    /// 2.modifying sharedMaterials = newMaterials, not sharedMaterials[i] = newMaterials[i](this doesn't works at all!!!)
    ///

    using UnityEngine;


    [System.Flags]
    internal enum ReplacementRendererSource 
    {
        CanvasRenderer = 1,

        ParticleSystem = 2,
        Renderer = 1 << 30,
    }

    interface IReplacementRenderer 
    {
        void Replace(Material replacementMaterial);
        void Restore();

        #region Material SetProperty
        void SetMaterialPropertyColor(string property, Color value, object context = null);
        #endregion
    }
}
