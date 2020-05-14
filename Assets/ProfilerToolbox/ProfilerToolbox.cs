namespace ProfilerToolbox 
{    
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    [ExecuteAlways]
    public class ProfilerToolbox : MonoBehaviour
    {
#if UNITY_EDITOR
        private bool m_isOpeningScene = false;
        private bool m_isSavingScene = false;
        private bool m_editorApplicationFocused = false; // in Editor Not Playing Mode

        [UnityEditor.InitializeOnLoadMethod]
        public static void InitializeOnScriptsCompiled()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            UnityEditor.SceneManagement.EditorSceneManager.sceneClosing -= OnSceneClosing;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosing += OnSceneClosing;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= OnSceneClosed;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += OnSceneClosed;

            UnityEditor.SceneManagement.EditorSceneManager.sceneUnloaded -= OnSceneUnloaded;
            UnityEditor.SceneManagement.EditorSceneManager.sceneUnloaded += OnSceneUnloaded;
            UnityEditor.SceneManagement.EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEditor.SceneManagement.EditorSceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnPlayModeStateChange(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode || state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                Clear();
        }

        private static void OnBeforeAssemblyReload()
        {
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            // when???
            //Debug.LogError($"OnSceneClosing {scene.name} {removingScene}");
        }

        private static void OnSceneClosed(Scene scene)
        {
            // when???
            //Debug.LogError($"OnSceneClosed {scene.name}");
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            // when switched to Editor Playing Mode
            //Debug.LogError($"OnSceneUnload {scene.name}"); 
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // when switched to Editor Playing Mode
            //Debug.LogError($"OnSceneLoaded {scene.name} mode");
        }

        private void OnSceneOpening(string path, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            // when reopen scene/new scene in Editor Not-Playing Mode
            m_isOpeningScene = true;
            //Debug.LogError($"OnSceneOpening {path} {mode}");
        }

        private void OnSceneOpened(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            // when reopen scene/new scene in Editor Not-Playing Mode
            m_isOpeningScene = false;
            //Debug.LogError($"OnSceneOpened {scene.name} {mode}");
        }

        private void OnSceneSaving(Scene scene, string path)
        {
            m_isSavingScene = true;
            if (this != null)
                SetVisualization(Visualization.Type.None);
            //Debug.LogError($"OnSceneSaving {scene.name} {path}");
        }

        private void OnSceneSaved(Scene scene)
        {
            m_isSavingScene = false;
            //Debug.LogError($"OnSceneSaved {scene.name}");
        }

        private void CheckEditroApplicationFocus()
        {
            // when in Editor Not Playing Mode
            if (!m_editorApplicationFocused && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                m_editorApplicationFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                //Debug.Log("On focus window!");
                TryRecoverVisualizationQuadOverdraw();
            }
            else if (m_editorApplicationFocused && !UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                m_editorApplicationFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                //Debug.Log("On lost focus");
            }
        }
#endif

        public Camera targetCamera;

        /// <summary>
        /// modify this by invoking SetVisualization
        /// </summary>
        public Visualization.Type visualization = Visualization.Type.None;
        private Visualization.IProfilerVisualization currVisualization { get {
                switch(visualization)
                {
                    case Visualization.Type.QuadOverdraw:
                        return visualizationQuadOverdraw;

                    case Visualization.Type.PixelOverdraw:
                        return visualizationPixelOverdraw;
                        
                    case Visualization.Type.ParticleFlat:
                        return visualizationParticleFlat;
                }
                return null;
            } }

        public Visualization.QuadOverdraw.ProfilerVisualization visualizationQuadOverdraw = new Visualization.QuadOverdraw.ProfilerVisualization();
        public Visualization.PixelOverdraw.ProfilerVisualization visualizationPixelOverdraw = new Visualization.PixelOverdraw.ProfilerVisualization();
        public Visualization.ParticleFlat.ProfilerVisualization visualizationParticleFlat = new Visualization.ParticleFlat.ProfilerVisualization();

        void OnAwake()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Clear()
        {
            if (currVisualization != null)
                currVisualization.Exit();
        }

        private void OnEnable()
        {   
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying)
                return;
#endif

            if (currVisualization != null)
                currVisualization.Enter(targetCamera);

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening -= OnSceneOpening;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += OnSceneOpening;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnSceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnSceneSaving;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;

            UnityEditor.EditorApplication.update -= CheckEditroApplicationFocus;
            UnityEditor.EditorApplication.update += CheckEditroApplicationFocus;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying)
                return;

            // disable error: Cannot destroy Component while GameObject is being activated or deactivated.
            if (m_isOpeningScene)
                return;
#endif

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening -= OnSceneOpening;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnSceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnSceneSaving;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
            UnityEditor.EditorApplication.update -= CheckEditroApplicationFocus;
#endif
            Clear();
        }

        //private bool m_ApplicationPause = false;
        //void OnApplicationPause(bool pauseStatus)
        //{
        //    if (!Application.isPlaying)
        //        return;

        //    //Debug.LogError($"OnApplicationPause {pauseStatus}");
        //    if (m_ApplicationPause != pauseStatus)
        //    {
        //        m_ApplicationPause = pauseStatus;
        //        if (m_ApplicationPause == false)
        //            TryRecoverVisualizationQuadOverdraw();
        //    }
        //}

        #region control
        public void OnCameraChange()
        {
            if (!enabled)
                return;

            SetVisualization(visualization);
        }

        public void SetVisualization(Visualization.Type newType)
        {
            if (currVisualization != null)
                currVisualization.Exit();

            visualization = newType;
            if (enabled && targetCamera && currVisualization != null)
                currVisualization.Enter(targetCamera);
        }

        private void TryRecoverVisualizationQuadOverdraw()
        {
            // hack for QuadOverdraw when Editor Application get focus again
            if (visualization == Visualization.Type.QuadOverdraw)
                SetVisualization(Visualization.Type.QuadOverdraw);
        }
        #endregion
    }
}