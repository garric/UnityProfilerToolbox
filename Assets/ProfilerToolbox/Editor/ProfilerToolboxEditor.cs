namespace ProfilerToolbox 
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(ProfilerToolbox))]
    internal class ProfilerToolboxEditor : BaseEditor<ProfilerToolbox> 
    {
        public SerializedProperty camera;

        public SerializedProperty visualization;
        public SerializedProperty visualizationQuadOverdraw;
        public SerializedProperty visualizationPixelOverdraw;
        public SerializedProperty visualizationParticleFlag;

        void OnEnable()
        {
            camera = FindProperty(x => x.targetCamera);

            visualization = FindProperty(x => x.visualization);
            visualizationQuadOverdraw = FindProperty(x => x.visualizationQuadOverdraw);
            visualizationPixelOverdraw = FindProperty(x => x.visualizationPixelOverdraw);
            visualizationParticleFlag = FindProperty(x => x.visualizationParticleFlat);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // camera
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(camera, new GUIContent("camera"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                m_Target.OnCameraChange();
                return;
            }

            EditorGUILayout.Space();
            UtilityEditor.DrawSplitter();

            EditorGUI.BeginDisabledGroup(camera.objectReferenceValue == null);
            // visualization
            UtilityEditor.DrawHeader("Visualization");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(visualization, new GUIContent("Type"));
            bool visualizationChanged = EditorGUI.EndChangeCheck();
            switch(visualization.intValue)
            {
                case (int)Visualization.Type.QuadOverdraw:
                    EditorGUILayout.PropertyField(visualizationQuadOverdraw, true);
                    break;
                case (int)Visualization.Type.PixelOverdraw:
                    EditorGUILayout.PropertyField(visualizationPixelOverdraw, true);
                    break;
                case (int)Visualization.Type.ParticleFlat:
                    EditorGUILayout.PropertyField(visualizationParticleFlag, true);
                    break;
            }
            if (visualizationChanged)
                m_Target.SetVisualization((Visualization.Type)visualization.intValue);
            
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
