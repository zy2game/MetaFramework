using UnityEditor;
using UnityEngine;

namespace UixFramework
{
    [CustomEditor(typeof(TweenPosition))]
    public class TweenPositionInspector : TweenAnimationInspector 
    {

        SerializedProperty isLocal;

        protected override void OnEnable()
        {
            base.OnEnable();
            isLocal = serializedObject.FindProperty("isLocal");
        }

        protected override void DrawGUI()
        {
            base.DrawGUI();
            EditorGUILayout.PropertyField(isLocal);
        }

        protected override void SetEndCurValue()
        {
            Transform transform = (target as TweenScale).transform;
            end.vector3Value = isLocal.boolValue?transform.localPosition: transform.position;
        }

        protected override void SetStartCurValue()
        {
            Transform transform = (target as TweenScale).transform; 
            start.vector3Value = isLocal.boolValue ? transform.localPosition : transform.position;
        }
    }
}
