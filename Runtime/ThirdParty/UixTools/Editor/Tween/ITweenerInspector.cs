
using UnityEngine;
using UnityEditor;

namespace UixFramework
{
    [CustomEditor(typeof(ITweener))]
    public class ITweenerInspector : InspectorBase
    {
        SerializedProperty playMode;
        SerializedProperty finishedPlay; 

        protected virtual void OnEnable()
        {
            playMode = serializedObject.FindProperty("playMode");
            finishedPlay= serializedObject.FindProperty("finishedPlay");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawGUI();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawGUI()
        {
            EditorGUILayout.PropertyField(playMode);
            EditorGUILayout.PropertyField(finishedPlay);

        }
    }
}
